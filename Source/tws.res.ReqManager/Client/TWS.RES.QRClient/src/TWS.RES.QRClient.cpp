#include "TWS.RES.QRClient.h"
#include "TWS.RES.QRDefine.h"
#include "WSReqClient.h"
#include "levstring.h"
#include "reqstruct.h"
#include "Logger.h"
#include "stdexcept"

char HexToChar(char hex_);

#if defined(UNDER_CE)
#define LOG_FILE "\\CF\\micros\\etc\\QRPromotionClientLog.txt"
#else
#define LOG_FILE "QRPromotionClientLog.log"
#endif

TWSRESQRPROMOTIONCLIENT_API_EXPORTS void Test(int* intArray_)
{
	int index = 0;
	for (index = 0; index < 10; index++)
	{
		intArray_[index] = index;
	}
}

TWSRESQRPROMOTIONCLIENT_API_EXPORTS
//void Validate(IN char* ip_, IN int port_, IN char* qrCode_, IN int responseSize_, OUT char* response_, OUT char* res_)
void Validate(IN  char* ip_,            IN  int  port_,         IN  char* qrCode_,       INOUT int* arraySize_, OUT int* status_,
              OUT int*  itemTypeArray_, OUT int* itemPluArray_, OUT int*  itemQtyArray_, IN    int  msgSize_,   OUT char* responseMsg_, OUT char* res_) 
{
	ReqMessage	reqMsg;
	ByteStream	msgChain;

	lev::string	strBody;
	lev::string strResponse;

	try
	{
		Logger::LogEvent(LOG_FILE, "Enter to Validate()", true);

		strBody << qrCode_;

		msgChain.Assign((Byte*)(const char*)strBody, strBody.Longitud());

		ReqClient reqClient(ip_, port_, ReqMessage(QRPROMOTION_MSG::VALIDATE, msgChain.Length(), (Byte*)msgChain));

		if (reqClient.RequestCommand(reqMsg))
		{
			lev::string str((char*)reqMsg.Body, reqMsg.BodySize);
			lev::SafeList<lev::string> responseParts = str.Split('|');

			memset(responseMsg_, 0, msgSize_);
			if (responseParts.Size() > 2)
			{
				lev::SafeList<lev::string> keyVal;

				*status_ = atoi(responseParts[0]);
				memcpy(responseMsg_, responseParts[1], min(responseParts[1].Longitud(), msgSize_ - 1));

				int index = 2;
				while (index < responseParts.Size() && index < *arraySize_)
				{
					itemTypeArray_[(index - 2) / 3] = atoi(responseParts[index + 0]);
					itemPluArray_[(index - 2) / 3]  = atoi(responseParts[index + 1]);
					itemQtyArray_[(index - 2) / 3]  = atoi(responseParts[index + 2]);

					index += 3;
				}

				*arraySize_ = (index - 2) / 3;				
				*res_ = RES_OK;
			}
			else if (responseParts.Size() == 2)
			{
				*status_ = atoi(responseParts[0]);
				memcpy(responseMsg_, (char*)responseParts[1], min(responseParts[1].Longitud(), msgSize_-1));
	
				strResponse = responseParts[0];
				Logger::LogEvent(LOG_FILE, lev::string() << "In Validate(): " << strResponse, true);

				*res_ = RES_OK;
			}
			else
			{
				strResponse = "BAD RESPONSE FORM [0]";
				Logger::LogEvent(LOG_FILE, lev::string() << "In Validate(): " << strResponse, true);
				*res_ = EXECUTION_ERROR;
			}
		}
		else
		{ 
			*arraySize_ = 0;
			//devuelvo codigo de error		
			*res_ = (char)reqMsg.MessageType;
		}
	}
	catch (const std::exception ex)
	{
		lev::string strErr = "";
		strErr << "In Validate(), exception caught: " << ex.what();
		strResponse = strErr;

		Logger::LogEvent(LOG_FILE, strErr, true);
		*res_ = 0;
	}
	catch (...)
	{
		lev::string strErr = "";
		strErr << "In Validate(), unhandled exception caught";
		strResponse = strErr;

		Logger::LogEvent(LOG_FILE, strErr, true);
		*res_ = 0;
	}

	Logger::LogEvent(LOG_FILE, "Exit from Validate()", true);
}

TWSRESQRPROMOTIONCLIENT_API_EXPORTS
void Redeem(IN  char* ip_,			IN  int   port_,	IN  char* qrCode_,		IN  char* reference_,	IN  int   msgSize_, 
			IN  long  amount_,		IN  char* store_,	IN  char* terminal_,   
			OUT char* responseMsg_, OUT int*  status_,	OUT int*  voucherId_,	OUT int*  transId_,		OUT char* res_)
{
	ReqMessage	reqMsg;
	ByteStream	msgChain;

	lev::string	strBody;
	lev::string strResponse;

	try
	{
		Logger::LogEvent(LOG_FILE, "Enter to Redeem()", true);

		strBody << qrCode_ << "|" << reference_ << "|" << amount_ << "|" << store_ << "|" << terminal_;

		msgChain.Assign((Byte*)(const char*)strBody, strBody.Longitud());

		ReqClient reqClient(ip_, port_, ReqMessage(QRPROMOTION_MSG::REDEEM, msgChain.Length(), (Byte*)msgChain));

		if (reqClient.RequestCommand(reqMsg))
		{
			lev::string str((char*)reqMsg.Body, reqMsg.BodySize);
			lev::SafeList<lev::string> responseParts = str.Split('|');

			if (responseParts.Size() > 1)
			{
				memcpy(responseMsg_, responseParts[1], min(responseParts[1].Longitud(), msgSize_ - 1));
			}

			if (responseParts.Size() > 4)
			{
				lev::SafeList<lev::string> keyVal;

				*status_ = atoi(responseParts[0]);
				*transId_ = atoi(responseParts[3]);
				*voucherId_ = atoi(responseParts[4]);
				*res_ = RES_OK;
			}
			else if (responseParts.Size() == 1)
			{
				*status_ = atoi(responseParts[0]);

				strResponse = responseParts[0];
				Logger::LogEvent(LOG_FILE, lev::string() << "In Redeem(): " << strResponse, true);
				*res_ = RES_OK;
			}
			else
			{
				strResponse = "BAD RESPONSE FORM [0]";
				Logger::LogEvent(LOG_FILE, lev::string() << "In Redeem(): " << strResponse, true);
				*res_ = EXECUTION_ERROR;
			}
		}
		else
		{
			//devuelvo codigo de error		
			*res_ = (char)reqMsg.MessageType;
		}
	}
	catch (const std::exception ex)
	{
		lev::string strErr = "";
		strErr << "In Redeem(), exception caught: " << ex.what();
		strResponse = strErr;

		Logger::LogEvent(LOG_FILE, strErr, true);
		*res_ = 0;
	}
	catch (...)
	{
		lev::string strErr = "";
		strErr << "In Redeem(), unhandled exception caught";
		strResponse = strErr;

		Logger::LogEvent(LOG_FILE, strErr, true);
		*res_ = 0;
	}

	Logger::LogEvent(LOG_FILE, "Exit from Redeem()", true);
}


TWSRESQRPROMOTIONCLIENT_API_EXPORTS
void Void(IN char* ip_,       IN int port_,    IN  char* qrCode_,      IN char* store_, 
	      IN char* terminal_, IN int msgSize_, OUT char* responseMsg_, OUT char* res_)
{
	ReqMessage	reqMsg;
	ByteStream	msgChain;

	lev::string	strBody;
	lev::string strResponse;

	try
	{
		Logger::LogEvent(LOG_FILE, "Enter to Redeem()", true);

		strBody << qrCode_ << "|" << store_ << "|" << terminal_;

		msgChain.Assign((Byte*)(const char*)strBody, strBody.Longitud());

		ReqClient reqClient(ip_, port_, ReqMessage(QRPROMOTION_MSG::VOIDQR, msgChain.Length(), (Byte*)msgChain));

		if (reqClient.RequestCommand(reqMsg))
		{
			lev::string str((char*)reqMsg.Body, reqMsg.BodySize);
			lev::SafeList<lev::string> responseParts = str.Split('|');

			if (responseParts.Size() == 1)
			{
				lev::SafeList<lev::string> keyVal;
				*res_ = RES_OK;
			}
			else if (responseParts.Size() > 1)
			{
				memcpy(responseMsg_, responseParts[1], min(responseParts[1].Longitud(), msgSize_ - 1));
			}
			else
			{
				strResponse = "BAD RESPONSE FORM [0]";
				Logger::LogEvent(LOG_FILE, lev::string() << "In Redeem(): " << strResponse, true);
				*res_ = EXECUTION_ERROR;
			}
		}
		else
		{
			//devuelvo codigo de error		
			*res_ = (char)reqMsg.MessageType;
		}
	}
	catch (const std::exception ex)
	{
		lev::string strErr = "";
		strErr << "In Redeem(), exception caught: " << ex.what();
		strResponse = strErr;

		Logger::LogEvent(LOG_FILE, strErr, true);
		*res_ = 0;
	}
	catch (...)
	{
		lev::string strErr = "";
		strErr << "In Redeem(), unhandled exception caught";
		strResponse = strErr;

		Logger::LogEvent(LOG_FILE, strErr, true);
		*res_ = 0;
	}

	Logger::LogEvent(LOG_FILE, "Exit from Redeem()", true);
}
