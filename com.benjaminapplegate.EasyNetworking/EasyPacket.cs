using System;
using System.Collections.Generic;
using System.Text;

namespace com.benjaminapplegate.EasyNetworking
{
    public class EasyPacket
    {
        private static int readPos = 0;

        public virtual void handleDataFromServer(byte[] data)
        {
            readPos = 0;
        }

        public virtual void handleDataFromClient(byte[] data, int userID)
        {
            readPos = 0;
        }

        protected static void writeInt(int value, ref List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(value));
        }

        protected static int readInt(byte[] data)
        {
            int value = BitConverter.ToInt32(data, readPos);
            readPos += sizeof(int);
            return value;
        }

        protected static void writeFloat(float value, ref List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(value));
        }

        protected static float readFloat(byte[] data)
        {
            float value = BitConverter.ToSingle(data, readPos);
            readPos += sizeof(float);
            return value;
        }

        protected static void writeDouble(double value, ref List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(value));
        }

        protected static double readDouble(byte[] data)
        {
            double value = BitConverter.ToDouble(data, readPos);
            readPos += sizeof(double);
            return value;
        }

        protected static void writeBool(bool value, List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(value));
        }

        protected static bool readBool(byte[] data)
        {
            bool value = BitConverter.ToBoolean(data, readPos);
            readPos += sizeof(bool);
            return value;
        }

        protected static void writeChar(char value, List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(value));
        }

        protected static char readChar(byte[] data)
        {
            char value = BitConverter.ToChar(data, readPos);
            readPos += sizeof(char);
            return value;
        }
        
        protected static void writeShort(short value, ref List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(value));
        }

        protected static short readShort(byte[] data)
        {
            short value = BitConverter.ToInt16(data, readPos);
            readPos += sizeof(short);
            return value;
        }
        
        protected static void writeLong(long value, ref List<byte> data)
        {
            data.AddRange(BitConverter.GetBytes(value));
        }

        protected static long readLong(byte[] data)
        {
            long value = BitConverter.ToInt32(data, readPos);
            readPos += sizeof(long);
            return value;
        }

        protected static void writeString(string value, ref List<byte> data)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            writeInt(valueBytes.Length, ref data);
            data.AddRange(valueBytes);
        }

        protected static string readString(byte[] data)
        {
            int stringLength = readInt(data);
            string value = Encoding.UTF8.GetString(data, readPos, stringLength);
            readPos += stringLength;
            return value;
        }
    }
}