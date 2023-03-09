using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TWS.Marshaling;


namespace TWS.Networking
{

    public struct MSG_DEF
    {
        public const Int16 INVALID_MSG = -1;
        public const Int16 EXECUTION_ERROR = (-100);
        public const Int16 CONNECTION_ERROR	=(-101);
        public const Int16 PARAM_ERROR = (-102);
        public const Int16 TIMEOUT_ERROR = (-103);
        public const Int16 CHECKSUM_ERROR = (-104);
    };

#if WindowsCE 
    [StructLayout(LayoutKind.Sequential)]
#else
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
#endif
    public struct ReqMessage : IMarshable
    {
        private Int32 mMessageType;
        private Int32 mBodySize;
        private Byte mChecksum;
        [MarshalAs(UnmanagedType.ByValArray)]
        private Byte[] mBody;

        public ReqMessage(ReqMessage? msg)
        {
            if (msg == null)
            {
                mMessageType = MSG_DEF.INVALID_MSG;
                mBodySize = 0;
                mBody = null;
                mChecksum = 0;
            }
            else
            {
                mMessageType = ((ReqMessage)msg).mMessageType;
                mBodySize = ((ReqMessage)msg).mBodySize;
                mBody = new Byte[mBodySize];
                for (int index = 0; index < mBodySize; index++)
                    mBody[index] = ((ReqMessage)msg).mBody[index];
                mChecksum = ((ReqMessage)msg).mChecksum;
            }
        }

        public static bool operator ==(ReqMessage msg1, ReqMessage msg2)
        {
            bool retVal = true;

            retVal = retVal && (msg1.mMessageType == msg2.mMessageType);
            retVal = retVal && (msg1.mBodySize == msg2.mBodySize);
            retVal = retVal && (msg1.mChecksum == msg2.mChecksum);

            for (int index = 0; retVal && index < msg1.mBodySize; index++)
                retVal = retVal && (msg1.mBody[index] == msg2.mBody[index]);

            return retVal;
        }

        public static bool operator !=(ReqMessage msg1, ReqMessage msg2)
        {
            return !(msg1 == msg2);
        }

        public override bool Equals(object obj)
        {
            if (obj is ReqMessage)
                return (this == (ReqMessage)obj);
            else 
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void Clear()
	    {
            mMessageType = MSG_DEF.INVALID_MSG;
            mBodySize = 0;
            mBody = null;
            mChecksum = 0;
        }

	    public Byte GenerateChecksum()
	    {
		    Int32 i;
            Byte[] bytesAux = null;

            mChecksum = 0;

            bytesAux = ByteStream.ToPByte(mMessageType);
            for(i = 0; i < bytesAux.Length; i++)
    		    mChecksum += bytesAux[i];

            bytesAux = ByteStream.ToPByte(mBodySize);
            for(i = 0; i < bytesAux.Length; i++)
    		    mChecksum += bytesAux[i];

            for(i = 0; i < mBodySize; i++)
    		    mChecksum += mBody[i];

            return mChecksum;
	    }

        public Int32 MessageType
        {
            get { return mMessageType; }
            set { mMessageType = value; }
        }
        
        public Int32 BodySize
        {
            get { return mBodySize; }
            set { mBodySize = value; }
        }
        
        public Byte Checksum
        {
            get { return mChecksum; }
            set { mChecksum = value; }
        }

        /*public Int32 ChecksumSize
        {
            get { return Marshal.SizeOf(Checksum); }
        }*/

        public Byte[] Body
        {
            get { return mBody; }
            set { mBody = value; }
        }

        public Int32 HeaderSize
        {
            get { return Marshal.SizeOf(mMessageType) + Marshal.SizeOf(mBodySize) + Marshal.SizeOf(mChecksum); }
        }
        
        #region IMarshable Members

        public byte[] ToPByte()
        {
            int offset = 0;

            Byte[] retVal = new Byte[HeaderSize + BodySize];
            
            //Message type
            Array.Copy(ByteStream.ToPByte(mMessageType), 0, retVal, offset, Marshal.SizeOf(mMessageType));
            offset += Marshal.SizeOf(mMessageType);

            //Body size
            Array.Copy(ByteStream.ToPByte(mBodySize), 0, retVal, offset, Marshal.SizeOf(mBodySize));
            offset += Marshal.SizeOf(mBodySize);

            //Checksum
            Array.Copy(ByteStream.ToPByte(mChecksum), 0, retVal, offset, Marshal.SizeOf(mChecksum));
            offset += Marshal.SizeOf(mChecksum);

            //Body
            Array.Copy(ByteStream.ToPByte(mBody), 0, retVal, offset, mBodySize);
            offset += mBodySize;

            return retVal;
        }

        #endregion

        public static ReqMessage ReqMessageFromByteStream(Byte[] buffer_, ref Int32 bytesRead_)
        {
            ReqMessage retVal = new ReqMessage(null);

            int offset = 0;

            if (bytesRead_ >= retVal.HeaderSize)
            {
                //Message Type
                retVal.MessageType = BitConverter.ToInt32(buffer_, offset);
                offset += Marshal.SizeOf(retVal.MessageType);

                //Body Size
                retVal.BodySize = BitConverter.ToInt32(buffer_, offset);
                offset += Marshal.SizeOf(retVal.BodySize);

                //Checksum
                retVal.Checksum = buffer_[offset];
                offset += Marshal.SizeOf(retVal.Checksum);

                //buffer didn't reach body
                if (retVal.BodySize <= bytesRead_ - retVal.HeaderSize)
                {
                    retVal.Body = new Byte[retVal.BodySize];

                    //get message body
                    Array.Copy(buffer_, retVal.HeaderSize, retVal.Body, 0, retVal.BodySize);
                    offset += retVal.BodySize;

                    //retVal.Checksum = buffer_[offset];

                    //test if trash was received
                    Byte recvChksum = retVal.Checksum;
                    if (recvChksum != retVal.GenerateChecksum())
                    {
                        retVal.Clear();
                        bytesRead_ = 0;
                    }
                    else
                    {
                        //adjust remaining bytes
                        Array.Copy(buffer_, retVal.HeaderSize + retVal.BodySize, buffer_, 0, bytesRead_ - retVal.HeaderSize - retVal.BodySize);
                        bytesRead_ -= (retVal.HeaderSize + retVal.BodySize);
                    }
                }
                else
                    retVal.Clear();
            }
            return retVal;
        }

        public static ReqMessage ReqMessageFromByteStream(ref List<Byte> byteStream_, ref Int32 bytesRead_)
        {
            Byte[] msgBytes = new byte[byteStream_.Count];

            byteStream_.CopyTo(msgBytes);
            ReqMessage retVal = ReqMessageFromByteStream(msgBytes, ref bytesRead_);

            byteStream_.Clear();

            for (int index = 0; index < bytesRead_; index++)
                byteStream_.Add(msgBytes[index]);

            return retVal;
        }
    }                                                                                                                                               
}
