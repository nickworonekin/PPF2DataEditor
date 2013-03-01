using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PPF2DataEditor
{
    public static class Extensions
    {
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[4096];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }

        public static void CopyStreamPart(Stream input, Stream output, int inputLength)
        {
            byte[] buffer = new byte[4096];
            int read;
            int totalRead = 0;
            while ((read = input.Read(buffer, 0, inputLength - totalRead > buffer.Length ? buffer.Length : inputLength - totalRead)) > 0)
            {
                output.Write(buffer, 0, read);
                totalRead += read;
            }
        }

        public static string ReadString(Stream input, Encoding encoding)
        {
            List<byte> inputBytes = new List<byte>();
            byte inputByte;

            while ((inputByte = (byte)input.ReadByte()) != 0x00)
                inputBytes.Add(inputByte);

            return encoding.GetString(inputBytes.ToArray());
        }

        public static string ReadString(Stream input, Encoding encoding, int maxLength)
        {
            byte[] buffer = new byte[maxLength];
            input.Read(buffer, 0, maxLength);

            for (int i = 0; i < maxLength; i++)
            {
                if (buffer[i] == 0)
                {
                    maxLength = i;
                    break;
                }
            }

            return encoding.GetString(buffer, 0, maxLength);
        }

        public static void WriteString(Stream output, string s, Encoding encoding, int length)
        {
            byte[] buffer = new byte[length];
            buffer = encoding.GetBytes(s);

            if (buffer.Length < length)
            {
                for (int i = 0; i < buffer.Length; i++)
                    output.WriteByte(buffer[i]);
                for (int i = buffer.Length; i < length; i++)
                    output.WriteByte(0);
            }
            else
            {
                for (int i = 0; i < length - 1; i++)
                    output.WriteByte(buffer[i]);

                output.WriteByte(0);
            }
        }

        public static int ReadInt32(Stream input)
        {
            return (input.ReadByte() | input.ReadByte() << 8 | input.ReadByte() << 16 | input.ReadByte() << 24);
        }
        public static uint ReadUInt32(Stream input)
        {
            return (uint)(input.ReadByte() | input.ReadByte() << 8 | input.ReadByte() << 16 | input.ReadByte() << 24);
        }

        public static void WriteInt32(Stream output, int value)
        {
            output.WriteByte((byte)(value & 0xFF));
            output.WriteByte((byte)((value >> 8) & 0xFF));
            output.WriteByte((byte)((value >> 16) & 0xFF));
            output.WriteByte((byte)((value >> 24) & 0xFF));
        }
        public static void WriteUInt32(Stream output, uint value)
        {
            output.WriteByte((byte)(value & 0xFF));
            output.WriteByte((byte)((value >> 8) & 0xFF));
            output.WriteByte((byte)((value >> 16) & 0xFF));
            output.WriteByte((byte)((value >> 24) & 0xFF));
        }

        public static int RoundUpTo(int n, int roundUpTo)
        {
            if (n % roundUpTo == 0)
                return n;

            return n + (roundUpTo - (n % roundUpTo));
        }
        public static uint RoundUpTo(uint n, int roundUpTo)
        {
            if (n % roundUpTo == 0)
                return n;

            return n + (uint)(roundUpTo - (n % roundUpTo));
        }
        public static long RoundUpTo(long n, int roundUpTo)
        {
            if (n % roundUpTo == 0)
                return n;

            return n + (roundUpTo - (n % roundUpTo));
        }
    }
}