#ifndef __SOCKETCLASS_H
#define __SOCKETCLASS_H

#if defined(SOCKET_DLL_EXPORT)
#define SOCKET_API __declspec(dllexport)
#elif defined(SOCKET_DLL_IMPORT)
#define SOCKET_API __declspec(dllimport)
#else
#define SOCKET_API
#endif

#if !defined(UNDER_CE) && !defined(WINCE)
#pragma comment (lib, "ws2_32.lib")
#else
#pragma comment (lib, "ws2.lib")
#endif

#include "winsock2.h" 
#include "ByteStream.h" 

/*==========================================================================*/
/*                                CLASE SOCKET                              */
/*==========================================================================*/

class SOCKET_API Socket
	{
	protected:
		SOCKET mSocketDescriptor;
		sockaddr_in mLocalSocketAddress;
		sockaddr_in mRemoteSocketAddress;
		
		static void LogEvento(ByteStream mensaje_, in_addr idDestino_, int nivelLog_ = 0);

	public:
		Socket();
		Socket(const Socket& socket_);
		virtual ~Socket();

		SOCKET& SocketDescriptor(void);
		SOCKET SocketDescriptor(void) const;

		sockaddr_in& LocalSocketAddress(void);
		sockaddr_in LocalSocketAddress(void) const;

		sockaddr_in& RemoteSocketAddress(void);
		sockaddr_in RemoteSocketAddress(void) const;

		bool CloseSocket(void);
		bool Shutdown(int type_ = SD_BOTH);

		bool operator ==(const Socket& socket_);
		Socket& operator  =(const Socket& socket_);
	};



/*==========================================================================*/
/*                             CLASE TCPSOCKET                              */
/*==========================================================================*/

class SOCKET_API TcpSocket : public Socket
	{
	private:
		static bool mWSAInitialized;

	public:
		TcpSocket();
		TcpSocket(const TcpSocket& tcpSocket_);
		virtual ~TcpSocket();

		void		SetBlockMode(bool block_ = true);

		bool		Bind(void);
		bool		Listen(int maxConn_);
		TcpSocket	Accept(void);
		bool		Connect(void);
		
		#ifdef _WIN_VISTA
		bool		Poll(short events_, short& response_, int timeOut_);
		#endif

		void Reset(void);

		int Send(Byte* data_, int size_);
		int Receive(Byte*, int size_); 

		bool operator ==(const TcpSocket& socket_);
		TcpSocket& operator =(const TcpSocket& socket_);

		static bool InitWSA(void);
		static bool StopWSA(void);
	};

#endif