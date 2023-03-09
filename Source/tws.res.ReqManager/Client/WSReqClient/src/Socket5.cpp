#include "Socket5.h"
#include "stdio.h"

#ifndef _WIN32_WCE
#include "fstream"
#endif

/*==========================================================================*/
/*                                CLASE SOCKET                              */
/*==========================================================================*/

Socket::Socket()
{
	memset(&mLocalSocketAddress, 0, sizeof(mLocalSocketAddress));
	memset(&mRemoteSocketAddress, 0, sizeof(mRemoteSocketAddress));
	mSocketDescriptor = INVALID_SOCKET;
}

Socket::Socket(const Socket& socket_)
{
	mSocketDescriptor = socket_.mSocketDescriptor;
	mLocalSocketAddress = socket_.mLocalSocketAddress;
	mRemoteSocketAddress = socket_.mRemoteSocketAddress;
}

Socket::~Socket()
{
}

SOCKET& Socket::SocketDescriptor(void)
{
	return mSocketDescriptor;
}

SOCKET Socket::SocketDescriptor(void) const
{
	return mSocketDescriptor;
}

sockaddr_in& Socket::LocalSocketAddress(void)
	{
	return mLocalSocketAddress;
	}

sockaddr_in Socket::LocalSocketAddress(void) const
{
	return mLocalSocketAddress;
}

sockaddr_in& Socket::RemoteSocketAddress(void)
{
	return mRemoteSocketAddress;
}

sockaddr_in Socket::RemoteSocketAddress(void) const
{
	return mRemoteSocketAddress;
}

bool Socket::CloseSocket(void)
{
	bool res = true;
	int retVal;
	char strAux[255];
	
	if(mSocketDescriptor != INVALID_SOCKET)
	{
		retVal = closesocket(mSocketDescriptor);
	
		sprintf(strAux, "Valor devuelto por CloseSocket: %d", retVal);
		//LogEvento(ByteStream((const char*)strAux), mRemoteSocketAddress.sin_addr);

		if(res = (retVal != SOCKET_ERROR))
			mSocketDescriptor = INVALID_SOCKET;
	}

	return res;
}

bool Socket::Shutdown(int type_)
{
	bool res = true;
	int retVal;

	if(mSocketDescriptor != INVALID_SOCKET)
	{
		retVal = shutdown(mSocketDescriptor, type_);
		res = (retVal != SOCKET_ERROR);

		char strAux[255];
		sprintf(strAux, "Valor devuelto por Shutdown: %d", retVal);
		//LogEvento(ByteStream((const char*)strAux), mRemoteSocketAddress.sin_addr);
	}
	return res ;
}

bool Socket::operator ==(const Socket& socket_)
{
	bool res = true;
	int i;

	for(i=0; res && i<sizeof(sockaddr_in); i++)
		res = res && (&mLocalSocketAddress) + i == (&(socket_.mLocalSocketAddress))+i;

	for(i=0; res && i<sizeof(sockaddr_in); i++)
		res = res && (&mRemoteSocketAddress) + i == (&(socket_.mRemoteSocketAddress))+i;

	return res;
}

Socket& Socket::operator  =(const Socket& socket_)
{
	mSocketDescriptor = socket_.mSocketDescriptor;
	mLocalSocketAddress = socket_.mLocalSocketAddress;
	mRemoteSocketAddress = socket_.mRemoteSocketAddress;
	
	return *this;
}

#ifndef _WIN32_WCE
void Socket::LogEvento(ByteStream mensaje_, in_addr idDestino_, int nivelLog_)
{
	std::ofstream archivoLog;
	char nombreArchivo[255];
	SYSTEMTIME fechaHora;

	strcpy(nombreArchivo, "LogSOCKET_");
	strcat(nombreArchivo, inet_ntoa(idDestino_));
	strcat(nombreArchivo, ".txt");

	archivoLog.open(nombreArchivo, std::ios::binary | std::ios::app);
	
	GetLocalTime(&fechaHora);
	
	archivoLog << fechaHora.wDay << "/" << fechaHora.wMonth << "/" << fechaHora.wYear << " ";
	archivoLog << fechaHora.wHour << ":" << fechaHora.wMinute << ":" << fechaHora.wSecond << ":" << fechaHora.wMilliseconds << "\t-->\t";
	archivoLog.write((char*)mensaje_.Bytes(), mensaje_.Length());
	archivoLog << std::endl ;

	archivoLog.close();
	archivoLog.flush();
}
#endif

/*==========================================================================*/
/*                             CLASE TCPSOCKET                              */
/*==========================================================================*/
TcpSocket::TcpSocket()
: Socket()
{
	mLocalSocketAddress.sin_family = AF_INET;
	mRemoteSocketAddress.sin_family = AF_INET;
}

TcpSocket::TcpSocket(const TcpSocket& tcpSocket_)
: Socket(tcpSocket_)
{
}

TcpSocket::~TcpSocket()
{
}

void TcpSocket::SetBlockMode(bool block_)
{
	unsigned long blockMode = 1;
	
	if(mSocketDescriptor == INVALID_SOCKET)
		mSocketDescriptor = socket(AF_INET, SOCK_STREAM, 0);

	if(block_)
		blockMode = 0;

	ioctlsocket(mSocketDescriptor, FIONBIO, &blockMode);
}

bool TcpSocket::Bind(void)
{
	bool res = false;
	int retVal = 0;
	
	if(mSocketDescriptor == INVALID_SOCKET)
		mSocketDescriptor = socket(AF_INET, SOCK_STREAM, 0);

	if(mSocketDescriptor != INVALID_SOCKET)
	{
		retVal = bind(mSocketDescriptor, (const sockaddr*)&mLocalSocketAddress, sizeof(mLocalSocketAddress));
		res = (retVal != SOCKET_ERROR);
	}

	return res;
}

#ifdef _WIN_VISTA
bool TcpSocket::Poll(short events_, short& response_, int timeOut_)
{
	bool res = false;
	int retVal = 0;

	if(mSocketDescriptor != INVALID_SOCKET)
	{
		pollfd fd = {mSocketDescriptor, events_, 0};

		retVal = WSAPoll(&fd, 1, timeOut_);

		res = (retVal == 1 && (fd.revents & events_));

		response_ = fd.revents;
	}

	return res;
}
#endif

bool TcpSocket::Listen(int maxConn_)
{
	bool res = false;
	int retVal = 0;

	if(mSocketDescriptor == INVALID_SOCKET)
		mSocketDescriptor = socket(AF_INET, SOCK_STREAM, 0);

	if(mSocketDescriptor != INVALID_SOCKET)
	{
		retVal = listen(mSocketDescriptor, maxConn_);
		res = (retVal != SOCKET_ERROR);
	}

	return res;
}

TcpSocket TcpSocket::Accept(void)
{
	TcpSocket res;
	int tmp = sizeof(mRemoteSocketAddress);

	if(mSocketDescriptor == INVALID_SOCKET)
		mSocketDescriptor = socket(AF_INET, SOCK_STREAM, 0);


	if(mSocketDescriptor != INVALID_SOCKET)
	{
		res.SocketDescriptor() = accept(mSocketDescriptor, (sockaddr*)&mRemoteSocketAddress, &tmp);
		res.LocalSocketAddress() = mLocalSocketAddress;
		res.RemoteSocketAddress() = mRemoteSocketAddress;
	}

	return res;
}

bool TcpSocket::Connect(void)
{
	bool res = false;
	int retVal = 0;
	int addrSize = sizeof(mLocalSocketAddress);

	if(mSocketDescriptor == INVALID_SOCKET)
		mSocketDescriptor = socket(AF_INET, SOCK_STREAM, 0);

	if(mSocketDescriptor != INVALID_SOCKET)
	{
		retVal = connect(mSocketDescriptor, (const sockaddr*)&mRemoteSocketAddress, sizeof(mRemoteSocketAddress)); 
		getsockname(mSocketDescriptor, (sockaddr*) &mLocalSocketAddress, &addrSize);
	
		char strAux[255];
		sprintf(strAux, "Valor devuelto por Connect: %d", retVal);
		//LogEvento(ByteStream((const char*)strAux), mRemoteSocketAddress.sin_addr);
		
		res = (retVal != SOCKET_ERROR);
	}

	return res;
}

void TcpSocket::Reset(void)
{
	Shutdown();
	if(CloseSocket())
	{
		mSocketDescriptor = socket(AF_INET, SOCK_STREAM, 0);
		mLocalSocketAddress.sin_family = AF_INET;
		mRemoteSocketAddress.sin_family = AF_INET;
	}
}

int TcpSocket::Send(Byte* data_, int size_)
{
	return send(mSocketDescriptor, (const char*) data_, size_, 0);
}

int TcpSocket::Receive(Byte* data_, int size_)
{
	int ret = 0;
	ret = recv(mSocketDescriptor, (char*)data_, size_, 0);
	return ret;
}


bool TcpSocket::operator ==(const TcpSocket& socket_)
{
	return Socket::operator ==(socket_);
}

TcpSocket& TcpSocket::operator  =(const TcpSocket& socket_)
{
	Socket::operator = (socket_);
	return *this;
}

bool TcpSocket::InitWSA(void)
{
	WORD	wVersionRequested;
	WSADATA wsaData;
	int		err;

	if(TcpSocket::mWSAInitialized)
		return true;

	wVersionRequested = MAKEWORD(2, 2);
 
	err = WSAStartup(wVersionRequested, &wsaData);
	
	mWSAInitialized = (err == 0); 
	
	return (mWSAInitialized);
}

bool TcpSocket::StopWSA(void)
{
	int	err;
	
	if(!mWSAInitialized)
		return true;
	
	err = WSACleanup();

	mWSAInitialized = (err != 0); 

	return (!mWSAInitialized);
}

bool TcpSocket::mWSAInitialized = false;
