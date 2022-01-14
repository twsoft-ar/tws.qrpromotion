using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.Remoting;

namespace TWS.Networking
{
    public class ReqInstance : IReqInstance 
    {
        private static readonly NLog.Logger LOG = NLog.LogManager.GetCurrentClassLogger();

        private Socket mSocket;
       
        public ReqInstance(Socket socket_)
        {
            mSocket = socket_;
        }

        public void StartProcess(Socket socket_)
        {
            mSocket = socket_;
            StartProcess();
        }

        public void StartProcess()
        {
            ReqMessage requestMsg = WaitForRequest();
            ReqMessage responseMsg;

            try
            {
                if (requestMsg.MessageType != MSG_DEF.INVALID_MSG)
                {
                    responseMsg = ProcessIncomingRequest(requestMsg);

                    if (!SendResponse(responseMsg))
                    {
                        LOG.Info("{Message}", $"Error sending response message");
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }
            finally
            {
                mSocket.Close();
                LOG.Trace("EXIT");
            }
        }

        public Socket Socket
        {
            get { return mSocket; }
            set { mSocket = value; }
        }

        protected ReqInstance()
        { }

        ReqMessage WaitForRequest()
        {
            ReqMessage retVal = new ReqMessage(null);

            const Int32 bufferSize = 8192;
            Int32 totalBytesRecv;
            Int32 bytesRecv;
            Byte[] buffer = new Byte[bufferSize];
            bool stopReceiving;
            Int32 elapsedTime;

            totalBytesRecv  = 0;
            elapsedTime     = 0;
            stopReceiving   = false;

            totalBytesRecv = 0;

            while (!stopReceiving)
            {
                try
                {
                    bytesRecv = 0;

                    while (!stopReceiving && bytesRecv >= 0)
                    {
                        bytesRecv = mSocket.Receive(buffer, totalBytesRecv, bufferSize - totalBytesRecv, SocketFlags.None);

                        if (bytesRecv > -1)
                            totalBytesRecv += bytesRecv;
                    }

                }
                catch (SocketException se)
                {
                    LOG.Fatal(se, "{Message}", "SocketException caught.");
                }
                catch (ObjectDisposedException ode)
                {
                    LOG.Fatal(ode, "{Message}", "ObjectDisposedException caught.");
                    break;
                }

                if (totalBytesRecv > 0)
                {
                    retVal = ReqMessage.ReqMessageFromByteStream(buffer, ref totalBytesRecv);
                    stopReceiving = (retVal.MessageType != MSG_DEF.INVALID_MSG);
                }

                stopReceiving = stopReceiving || (elapsedTime > 5000);

                Thread.Sleep(10);
                elapsedTime += 10;
                        
            }//end while

            return retVal;
        }

        protected bool SendResponse(ReqMessage msg_)
        {
            bool retVal = false;
            
            Int32 bytesToSend = msg_.HeaderSize + msg_.BodySize;
            Int32 sentBytes = 0;
            Int32 totalSentBytes = 0;

            Byte[] buffer_ = msg_.ToPByte();

            try
            {
                while (totalSentBytes < bytesToSend && sentBytes >=0)
                {
                    sentBytes = mSocket.Send(buffer_, totalSentBytes, bytesToSend - totalSentBytes, SocketFlags.None);
                    totalSentBytes += sentBytes;
                }

                retVal = (totalSentBytes == bytesToSend);
            }
            catch (SocketException se)
            {
                LOG.Fatal(se, "{Message}", "SocketException caught.");
            }
            catch (ObjectDisposedException ode)
            {
                LOG.Fatal(ode, "{Message}", "ObjectDisposedException caught.");
            }
            catch (Exception ex)
            {
                LOG.Fatal(ex, "{Message}", "Exception caught.");
            }

            return retVal;
        }
    
        protected virtual ReqMessage ProcessIncomingRequest(ReqMessage msg_)
        {
            ReqMessage retVal = new ReqMessage(null);

	        //this method is only executed if no ReqInstance derived class method is executed
	        //due to no existance of such derived class

            LOG.Info("{Message}", $"Message type->{retVal.MessageType} received");
            
            retVal.MessageType = 0;
            retVal.Body = Marshaling.ByteStream.ToPByte("Derived ReqInstance not implemented\0");
            retVal.BodySize = retVal.Body.Length;

            return retVal;
        }
    }
}
