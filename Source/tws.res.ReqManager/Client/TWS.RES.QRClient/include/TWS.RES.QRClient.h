#ifndef __TWS_RES_QRPROMOTIONCLIENT_H
#define __TWS_RES_QRPROMOTIONCLIENT_H


#if defined(TWSRESQRPROMOTIONCLIENT_EXPORTS)
#define TWSRESQRPROMOTIONCLIENT_API_EXPORTS __declspec(dllexport)
#elif defined(TWSRESQRPROMOTIONCLIENT_IMPORT)
#define TWSRESQRPROMOTIONCLIENT_API_EXPORTS __declspec(dllimport)
#else
#define TWSRESQRPROMOTIONCLIENT_API_EXPORTS
#endif

#define IN
#define OUT
#define INOUT


// Define here all exportable functions to be called by micros SIM script

TWSRESQRPROMOTIONCLIENT_API_EXPORTS void Test(int* intArray_);

//TWSRESQRPROMOTIONCLIENT_API_EXPORTS

TWSRESQRPROMOTIONCLIENT_API_EXPORTS
void Validate(IN  char* ip_,            IN  int  port_,         IN  char* qrCode_,       INOUT int* arraySize_, OUT int* status_,
              OUT int*  itemTypeArray_, OUT int* itemPluArray_, OUT int*  itemQtyArray_, IN    int  msgSize_,   OUT char* responseMsg_, OUT char* res_); 

typedef void(*VALIDATE)(IN  char* /*ip_*/,            IN  int  /*port_*/,         IN  char* /*qrCode_,*/,      INOUT int* /*arraySize_*/, OUT int*  /*status_*/,
	                    OUT int*  /*itemTypeArray_*/, OUT int* /*itemPluArray_*/, OUT int*  /*itemQtyArray_*/, IN    int  /*msgSize_*/,   OUT char* /*responseMsg_*/, OUT char* /*res_*/);

TWSRESQRPROMOTIONCLIENT_API_EXPORTS
void Redeem(IN  char* ip_,			IN  int   port_,	IN  char* qrCode_,		IN  char* reference_,	IN  int   msgSize_,
			IN  int   amount_,		IN  char* store_,	IN  char* terminal_,   
			OUT char* responseMsg_, OUT int*  status_,	OUT int*  voucherId_,	OUT int*  transId_,		OUT char* res_);

typedef void(*REDEEM)(IN  char* /*ip_*/,          IN  int   /*port_*/,   IN  char* /*qrCode_*/,    IN  char* /*reference_*/, IN  int   /*msgSize_*/,
                      IN  long  /*amount_*/,      IN  char* /*store_*/,  IN  char* /*terminal_*/,
			          OUT char* /*responseMsg_*/, OUT int*  /*status_*/, OUT int*  /*voucherId_*/, OUT int*  /*transId_*/,   OUT char* /*res_*/);

TWSRESQRPROMOTIONCLIENT_API_EXPORTS
void Void(IN char* ip_,       IN int port_,    IN  char* qrCode_,      IN char* store_, 
	      IN char* terminal_, IN int msgSize_, OUT char* responseMsg_, OUT char* res_);

typedef void(*VOIDQR)(IN char* /*ip_*/,       IN int /*port_*/,    IN  char* /*qrCode_*/,      IN char* /*store_*/, 
	                  IN char* /*terminal_*/, IN int /*msgSize_*/, OUT char* /*responseMsg_*/, OUT char* /*res_*/);

#endif