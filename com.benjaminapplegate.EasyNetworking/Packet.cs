using System;
using System.Collections.Generic;
using System.Text;

namespace com.benjaminapplegate.EasyNetworking
{
    public class Packet
    {
        private int _readPos = 0;
        private List<byte> _data;

        public Packet(byte[] bytes)
        {
            _data = new List<byte>(bytes);
        }

        public Packet(int packetType)
        {
            _data = new List<byte>();
            WriteInt(packetType);
        }

        public byte[] GetBytes()
        {
            return _data.ToArray();
        }

        public void WriteInt(int value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }

        public int ReadInt()
        {
            int value = BitConverter.ToInt32(_data.ToArray(), _readPos);
            _readPos += sizeof(int);
            return value;
        }

        public void WriteFloat(float value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }

        public float ReadFloat()
        {
            float value = BitConverter.ToSingle(_data.ToArray(), _readPos);
            _readPos += sizeof(float);
            return value;
        }

        public void WriteDouble(double value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }

        public double ReadDouble()
        {
            double value = BitConverter.ToDouble(_data.ToArray(), _readPos);
            _readPos += sizeof(double);
            return value;
        }

        public void WriteBool(bool value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }

        public bool ReadBool()
        {
            bool value = BitConverter.ToBoolean(_data.ToArray(), _readPos);
            _readPos += sizeof(bool);
            return value;
        }

        public void WriteChar(char value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }

        public char ReadChar()
        {
            char value = BitConverter.ToChar(_data.ToArray(), _readPos);
            _readPos += sizeof(char);
            return value;
        }
        
        public void WriteShort(short value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }

        public short ReadShort()
        {
            short value = BitConverter.ToInt16(_data.ToArray(), _readPos);
            _readPos += sizeof(short);
            return value;
        }
        
        public void WriteLong(long value)
        {
            _data.AddRange(BitConverter.GetBytes(value));
        }

        public long ReadLong()
        {
            long value = BitConverter.ToInt32(_data.ToArray(), _readPos);
            _readPos += sizeof(long);
            return value;
        }

        public void WriteString(string value)
        {
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            WriteInt(valueBytes.Length);
            _data.AddRange(valueBytes);
        }

        public string ReadString()
        {
            int stringLength = ReadInt();
            string value = Encoding.UTF8.GetString(_data.ToArray(), _readPos, stringLength);
            _readPos += stringLength;
            return value;
        }
    }
}