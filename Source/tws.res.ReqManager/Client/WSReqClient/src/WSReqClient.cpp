#include "WSReqClient.h"
#include "comutil.h"

#define RES_TIEME_OUT 3*60*1000

ReqClient::ReqClient(char* ip_, int port_, ReqMessage reqMsg_)
{
	mSocket.RemoteSocketAddress().sin_family = AF_INET;
	mSocket.RemoteSocketAddress().sin_port = htons(port_);
	mSocket.RemoteSocketAddress().sin_addr.s_addr = inet_addr(ip_);

	mRequestCommand = reqMsg_;
}

ReqClient::~ReqClient()
{}

bool ReqClient::RequestCommand(ReqMessage& msg_)
{
	bool res = false;
	
	try
	{
		//connect to server
		if (ConnectToServer())
		{
			//send request
			if (SendRequest())
			{
				//wait response
				msg_ = WaitForResponse();
				res = msg_.IsValid();
			}
			else
			{
				msg_.MessageType = EXECUTION_ERROR;
			}

		}
		else
			msg_.MessageType = CONNECTION_ERROR;
	}
	catch (...)
	{
		msg_.MessageType = CONNECTION_ERROR;
	}

	Disconnect();

	return res;
}

bool ReqClient::ConnectToServer()
{
	bool res = false;
	
	if(!TcpSocket::InitWSA())
	    return res;	

	if(res = mSocket.Connect())
		mSocket.SetBlockMode(false);

	mSocketConnected = true;

	return res;
}

bool ReqClient::Disconnect()
{
	bool res = true;

	if(mSocketConnected)
		res = mSocket.CloseSocket();
	
	//res = res && mSocket.StopWSA();

	return res;
}

bool ReqClient::SendRequest()
{
	Byte		*rawMwssage;
	ByteStream bytesMessage;
	int			bytesToSend;
	int			totalSentBytes;
	int			sentBytes;
	bool		keepSending;

	bytesToSend = REQ_HEADER_SIZE + mRequestCommand.BodySize + sizeof(mRequestCommand.Checksum);

	mRequestCommand.GenerateChecksum();

	bytesMessage	= mRequestCommand.Bytes();
	rawMwssage		= (Byte*)bytesMessage;

	sentBytes		= 0;
	totalSentBytes	= 0;
	keepSending		= true;

	while(keepSending)
	{
		sentBytes = mSocket.Send((Byte*)(rawMwssage + totalSentBytes), bytesToSend - totalSentBytes);
		totalSentBytes += sentBytes;
		
		keepSending = totalSentBytes < bytesToSend && sentBytes >= 0;
	}

	return (totalSentBytes == bytesToSend);
}

ReqMessage& ReqClient::WaitForResponse()
{
	ReqMessage	res;
	Byte		buffer[8192];
	int			bufferSize;
	int			totalBytesRecv;
	int			bytesRecv;
	bool		stopReceiving;
	bool		receiving;
	ByteStream msgBytes;
	long		elapsedTime;
	Byte		checkSum;

	bufferSize = sizeof(buffer);

	checkSum		= 0;
	totalBytesRecv	= 0;
	elapsedTime		= 0;
	stopReceiving	= false;

	while(!stopReceiving)
	{
		bytesRecv = 0;
		receiving = true;
		while(receiving && totalBytesRecv < bufferSize)
		{
			bytesRecv = mSocket.Receive(buffer + totalBytesRecv, bufferSize - totalBytesRecv);
			
			if(receiving = (bytesRecv > 0))
			{
				totalBytesRecv += bytesRecv;
				elapsedTime = 0;
			}
		}
			
		if(bytesRecv > 0)
		{
			totalBytesRecv += bytesRecv;
			elapsedTime = 0;
		}

		if(totalBytesRecv > 0)
		{
			msgBytes += ByteStream(buffer, totalBytesRecv);
			totalBytesRecv = 0;
			
			int offset = 0;
			
			if(res.MessageType == (__int32)INVALID_MSG && (msgBytes.Length() >= REQ_HEADER_SIZE))
			{
				//res.MessageType = msgBytes[(size_t)0];
				//Get message type
				memcpy((char*)&res.MessageType, ((char*)msgBytes), sizeof(res.MessageType));
				offset += sizeof(res.MessageType);

				//Get body size
				memcpy((char*)&res.BodySize, ((char*)msgBytes) + offset, sizeof(res.BodySize));
				offset += sizeof(res.BodySize);

				//Get checksum
				memcpy((char*)&res.Checksum, ((char*)msgBytes) + offset, sizeof(res.Checksum));
				offset += sizeof(res.Checksum);

				res.Body = new Byte[res.BodySize];

				if(!res.Body)
					throw("(ReqInstance::WaitForRequest) -> Error allocating buffer");
			}

			if(res.MessageType != (__int32)INVALID_MSG && (msgBytes.Length() >= REQ_HEADER_SIZE + res.BodySize))
			{
				memcpy((void*)res.Body, ((char*)msgBytes) + REQ_HEADER_SIZE, res.BodySize);
				//res.Checksum = msgBytes[(size_t)(REQ_HEADER_SIZE + res.BodySize)];

				stopReceiving = true;
			}
		}
		
		stopReceiving = stopReceiving || (elapsedTime > RES_TIEME_OUT);

		Sleep(10);
		elapsedTime += 10;
	}//end while

	if(elapsedTime <= RES_TIEME_OUT)
	{
		//test checksum;
		Byte ckSum = res.Checksum;
		//for(long i = 0; i < (long)msgBytes.Length() - 1; i++)
			//checkSum += msgBytes[(size_t)i];
		res.GenerateChecksum();
		if(ckSum != res.Checksum)
		{
			res.Clear();
			res.MessageType = CHECKSUM_ERROR;
		}
	}
	else
	{
		//MessageBoxW(NULL, L"Request Client TIMEOUT", L"", MB_OK);
		res.Clear();
		res.MessageType = TIMEOUT_ERROR;
	}

	mResponseCommand = res;

 	return  mResponseCommand;
}
