#ifndef __WSREQCLIENT_H
#define __WSREQCLIENT_H


#include "Socket5.h"
#include "ReqStruct.h"

class ReqClient
{
private:
	TcpSocket	mSocket;
	ReqMessage	mRequestCommand;
	ReqMessage	mResponseCommand;
	bool		mSocketConnected;

public:
	ReqClient(char* ip_, int port_, ReqMessage reqMsg_);
	~ReqClient();

	bool RequestCommand(ReqMessage& msg_);

private:
	bool ConnectToServer();
	bool Disconnect();
	bool SendRequest();
	ReqMessage& WaitForResponse();
};

#endif