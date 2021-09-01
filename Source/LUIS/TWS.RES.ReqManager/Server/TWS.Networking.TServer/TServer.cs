using System;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Reflection;
using TWS.Networking.Properties;

namespace TWS.Networking
{
    public class TServer
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        private Socket mListenerSocket;
        private int mServerTcpPort;
        private Thread mThreadDoServe = null;
        private volatile bool mStopProcess;
        enum LockingState { UNLOCKED = 0, LOCKED = 1 };
        private int mLockingStatus = (int)LockingState.UNLOCKED;

        #region Constructors
        public TServer(int? tcpPort_)
        {
            mServerTcpPort = 0;

            try
            {
                //if (Registry.GetValue(Registry.LocalMachine.Name + @"\Software\Micros\FIP\IVU Loto Server","Server Port", null) == null)
                //    throw (new Exception("TServer Exception thrown: TServer port not defined in registry"));

                if (tcpPort_ == null || tcpPort_ == 0)
                {
                    //mServerTcpPort = (int)Registry.GetValue(Registry.LocalMachine.Name + @"\Software\Micros\FIP\IVU Loto Server", "Server Port", null);

                    if (mServerTcpPort == 0)
                        mServerTcpPort = Settings.Default.TCP_SERVER_PORT;

                    //if (mServerTcpPort == 0) { }
                      //  mServerTcpPort = Convert.ToInt32(Resources.SERVER_TCP_PORT);
                }
                else
                    mServerTcpPort = (int)tcpPort_;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Private Methods
        private void DoServe()
        {
            LOG.Trace("ENTER");

            mListenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, mServerTcpPort);

            try
            {
                mListenerSocket.Bind(localEndPoint);
                mListenerSocket.Listen(256);

                mStopProcess = false;
                while (!mStopProcess)
                {
                    Socket newSocket;

                    newSocket = mListenerSocket.Accept();
                    newSocket.Blocking = false;

                    //RaiseNewConnectionEvent(newSocket);
                    Socket handleSocket = newSocket;
                    new Thread(RaiseNewConnectionEvent).Start(handleSocket);
                }
            }
            catch (SocketException se)
            {
                if (se.ErrorCode != 10004) //when socket is closed 
                {   
                    LOG.Fatal(se, "{Message}", "SocketException caught.");
                    throw (se); 
                }
            }
            catch (ObjectDisposedException ode)
            {
                LOG.Fatal(ode, "{Message}", "ObjectDisposedException caught.");
                throw (ode);
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
                throw (ex);
            }
            finally
            {
                LOG.Trace("EXIT");
                CloseListenerSocket();
            }
        }

        private void CloseListenerSocket()
        {
            //Because of race condition between StopSever() and DoServe()
            while (Interlocked.CompareExchange(ref mLockingStatus, (int)LockingState.LOCKED, (int)LockingState.UNLOCKED) != (int)LockingState.UNLOCKED)
                Thread.Sleep(100);

            if (mListenerSocket != null)
                mListenerSocket.Close();

            mListenerSocket = null;
            mLockingStatus = (int)LockingState.UNLOCKED;
        }
        #endregion

        #region Public Methods
        public void StartServer()
        {
            if (mThreadDoServe != null && mThreadDoServe.IsAlive)
                return;

            mThreadDoServe = new Thread(DoServe);
            mThreadDoServe.Start(); 
        }

        public void StopServer()
        {
            mStopProcess = true;

            CloseListenerSocket();

            if (mThreadDoServe != null)
                mThreadDoServe.Join();
        }
        #endregion

        #region Get/Set
        public int ServerTcpPort
        {
            get { return mServerTcpPort; }
        }

        public Socket ListenerSocket
        {
            get { return mListenerSocket; }
        }
        #endregion

        #region Events
        public event NewConnectionEventHandler EventNewConnection;
        #endregion

        #region Event Raisers
        private void RaiseNewConnectionEvent(object socket_)
        {
            Socket socket = (Socket)socket_;

            //use a temporary copy to avoid the case a unsubscription after a null was checked
            NewConnectionEventHandler tempHandler = EventNewConnection;

            if (tempHandler != null)
                tempHandler(this, new NewConnectionEventArgs(socket));
        }
        #endregion
    }

    #region Delegates
    public delegate void NewConnectionEventHandler(object sender, NewConnectionEventArgs e);
    #endregion

    public class NewConnectionEventArgs : EventArgs
    {
        Socket mSocket;

        public NewConnectionEventArgs(Socket socket_)
        {
            mSocket = socket_;
        }

        public Socket Socket
        {
            get { return mSocket; }
        }
    }
}
