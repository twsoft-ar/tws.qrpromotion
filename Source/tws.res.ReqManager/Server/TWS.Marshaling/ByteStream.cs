using System;
using System.Collections.Generic;
//using System.Text;
using System.Runtime.InteropServices;

namespace TWS.Marshaling
{
    public class ByteStream
    {
        #region IMarshable Members

        public static Byte[] ToPByte(object source_)
        {
            Byte[] retVal = null;

            if (source_ == null)
                return retVal;

            if (source_ is string)
            {
                int size = ((string)source_).Length;
                retVal = new Byte[size];
                Array.Clear(retVal, 0, size);

                for (int index = 0; index < size; index++)
                    retVal[index] = Convert.ToByte(((string)source_)[index]);

                return retVal;
            }

            if (source_ is Byte[])
            {
                retVal = new Byte[((Byte[])source_).Length];
                Array.Copy((Byte[])source_, retVal, retVal.Length);

                return retVal;
            }
    
            retVal = new Byte[Marshal.SizeOf(source_)];

            IntPtr pStruct = Marshal.AllocHGlobal(Marshal.SizeOf(source_));

            try
            {
                Marshal.StructureToPtr(source_, pStruct, false);
                Marshal.Copy(pStruct, retVal, 0, Marshal.SizeOf(source_));
            }
            finally
            {
                Marshal.FreeHGlobal(pStruct);
            }

            return retVal;
        }

        public static object PByteToPrimitive(Byte[] source_, int offset_, Type type_)
        {
            return PByteToPrimitive(source_, offset_, source_.Length, type_);
        }

        public static object PByteToPrimitive(Byte[] source_, int offset_, int long_, Type type_)
        {
            object retVal = null;
            IntPtr pStruct = IntPtr.Zero;

            try
            {
                if (type_ == typeof(string))
                {
                    string auxStr = "";
                    int index = offset_;

                    while (index < (long_ + offset_) && source_[index] != 0)
                    {
                        auxStr += (char)source_[index];
                        index++;
                    }

                    retVal = auxStr;
                    return retVal;
                }

                if (source_.Length < Marshal.SizeOf(type_))
                    throw (new Exception("Source too small to convert to target type"));

                pStruct = Marshal.AllocHGlobal(Marshal.SizeOf(type_));

                Marshal.Copy(source_, offset_, pStruct, Marshal.SizeOf(type_));
                retVal = Marshal.PtrToStructure(pStruct, type_);
            }
            finally
            {
                if (pStruct != IntPtr.Zero)
                    Marshal.FreeHGlobal(pStruct);
            }

            return retVal;
        }

        public static Byte[] PrimitiveToPByte(object source_)
        {
            Byte[] retVal = null;

            if (!(source_ is byte || source_ is sbyte ||
                    source_ is short || source_ is ushort ||
                    source_ is int || source_ is uint ||
                    source_ is long || source_ is ulong ||
                    source_ is float || source_ is double ||
                    source_ is decimal || source_ is char ||
                    source_ is string || source_ is bool))
            {
                return retVal;
            }

            if (source_ is string)
            {
                int size = ((string)source_).Length;
                retVal = new Byte[size];
                Array.Clear(retVal, 0, size);

                for (int index = 0; index < size; index++)
                    retVal[index] = Convert.ToByte(((string)source_)[index]);

                return retVal;
            }

            retVal = new Byte[Marshal.SizeOf(source_)];

            IntPtr pStruct = Marshal.AllocHGlobal(Marshal.SizeOf(source_));

            try
            {
                Marshal.StructureToPtr(source_, pStruct, false);
                Marshal.Copy(pStruct, retVal, 0, Marshal.SizeOf(source_));
            }
            finally
            {
                Marshal.FreeHGlobal(pStruct);
            }

            return retVal;
        }

        #endregion
    }
}
