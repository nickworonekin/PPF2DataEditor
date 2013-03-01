using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PPF2DataEditor
{
    public static class LZ00
    {
        public static bool Decompress(Stream input, Stream output)
        {
            long startOffset = input.Position;

            // Check to see if this is an LZ00 compressed file
            if (!(input.ReadByte() == 'L' && input.ReadByte() == 'Z' && input.ReadByte() == '0' && input.ReadByte() == '0'))
            {
                return false;
            }

            // Get filesizes and encryption key
            int inputLength = Extensions.ReadInt32(input);

            input.Position += 40;

            int outputLength = Extensions.ReadInt32(input);
            uint key = Extensions.ReadUInt32(input);

            // Set up our pointers
            long inputPointer  = 0x30;
            long outputPointer = 0x0;
            int bufferPointer  = 0xFEE;

            // Set up our buffer
            byte[] buffer = new byte[0x1000];

            // Start decompression
            input.Position += 8;
            while (inputPointer < inputLength)
            {
                byte flag = Get((byte)input.ReadByte(), ref key);
                inputPointer++;

                for (int i = 0; i < 8; i++)
                {
                    if ((flag & 0x1) != 0) // Not compressed
                    {
                        byte value = Get((byte)input.ReadByte(), ref key);
                        output.WriteByte(value);
                        buffer[bufferPointer] = value;

                        inputPointer++;
                        outputPointer++;
                        bufferPointer = (bufferPointer + 1) & 0xFFF;
                    }
                    else // Compressed
                    {
                        byte b1 = Get((byte)input.ReadByte(), ref key), b2 = Get((byte)input.ReadByte(), ref key);
                        int offset = (((b2 >> 4) & 0xF) << 8) | b1;
                        int length = (b2 & 0xF) + 3;
                        inputPointer += 2;

                        for (int j = 0; j < length; j++)
                        {
                            output.WriteByte(buffer[(offset + j) & 0xFFF]);
                            buffer[bufferPointer] = buffer[(offset + j) & 0xFFF];

                            outputPointer++;
                            bufferPointer = (bufferPointer + 1) & 0xFFF;
                        }
                    }

                    // Check to see if we reached the end of the file
                    if (inputPointer >= inputLength)
                        break;

                    flag >>= 1;
                }
            }

            return true;
        }

        public static void Compress(Stream input, Stream output, string fname)
        {
            // Set up our compression dictionary
            LZ00Dictionary dictionary = new LZ00Dictionary();

            // Now read all the bytes
            byte[] inBuffer = new byte[input.Length];
            input.Position = 0;
            input.Read(inBuffer, 0, inBuffer.Length);
            input.Position = 0;

            long inputPointer = 0;
            uint inputLength = (uint)input.Length;

            // Set up buffer
            byte[] buffer = new byte[0x1000];
            int bufferPointer = 0xFEE;

            // Set up the encryption key
            uint key = (uint)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;

            // Set up destination stream and write the header
            output.WriteByte((byte)'L');
            output.WriteByte((byte)'Z');
            output.WriteByte((byte)'0');
            output.WriteByte((byte)'0');

            // Compressed Size (will fill in later)
            // 8 null bytes
            // We'll just write 12 bytes here
            output.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12);

            // Get the original filename based on the extension of the file in the ISO
            string fext = String.Empty;
            if (fname.ToLower().EndsWith(".mrz"))
                fext = ".mrg";
            else if (fname.ToLower().EndsWith("tez"))
                fext = ".tex";

            Extensions.WriteString(output, Path.GetFileNameWithoutExtension(fname) + fext, Encoding.GetEncoding("Shift_JIS"), 32);

            // Uncompressed filesize
            Extensions.WriteUInt32(output, inputLength);

            // Encryption key
            Extensions.WriteUInt32(output, key);

            // 8 null bytes
            output.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 8);

            int outputPointer = (int)output.Position;

            // Start compressing
            while (inputPointer < inputLength)
            {
                byte flag = 0;
                long flagPointer = outputPointer;
                uint flagKey = key;
                output.WriteByte(Get(flag, ref key)); // This will be filled in later
                outputPointer++;

                for (int i = 0; i < 8; i++)
                {
                    int matchOffset, matchLength;
                    if (dictionary.Search(inBuffer, buffer, (int)inputLength, (int)inputPointer, out matchOffset, out matchLength))
                    {
                        // Good match!
                        output.WriteByte(Get((byte)(matchOffset & 0xFF), ref key));
                        output.WriteByte(Get((byte)(((matchOffset & 0xF00) >> 4) | ((matchLength - 3) & 0xF)), ref key));

                        for (int j = 0; j < matchLength; j++)
                        {
                            dictionary.AddEntry(buffer, inBuffer[inputPointer], (int)inputPointer);
                            buffer[bufferPointer] = inBuffer[inputPointer];
                            inputPointer++;
                            bufferPointer = (bufferPointer + 1) & 0xFFF;
                        }

                        outputPointer += 2;
                    }
                    else // No match
                    {
                        flag |= (byte)(1 << i);

                        output.WriteByte(Get(inBuffer[inputPointer], ref key));
                        dictionary.AddEntry(buffer, inBuffer[inputPointer], (int)inputPointer);
                        buffer[bufferPointer] = inBuffer[inputPointer];

                        inputPointer++;
                        outputPointer++;
                        bufferPointer = (bufferPointer + 1) & 0xFFF;
                    }

                    // Check for out of bounds
                    if (inputPointer >= inputLength)
                        break;
                }

                // Now we can write the flag
                output.Position = flagPointer;
                output.WriteByte(Get(flag, ref flagKey));
                output.Position = outputPointer;
            }

            // Write the compressed length
            output.Position = 0x4;
            Extensions.WriteUInt32(output, (uint)outputPointer);
            output.Position = outputPointer;
        }

        private static byte Get(byte val, ref uint key)
        {
            // Generate a new key
            uint x = (((((((key << 1) + key) << 5) - key) << 5) + key) << 7) - key;
            x = (x << 6) - x;
            x = (x << 4) - x;

            key = ((x << 2) - x) + 12345;

            // Now return the value since we have the key
            uint t = (key >> 16) & 0x7FFF;
            return (byte)(val ^ ((((t << 8) - t) >> 15)));
        }

        public class LZ00Dictionary
        {
            public int BufferSize = 0x1000;
            public int BufferStart = 0xFEE;
            public int MinMatchLength = 3;
            public int MaxMatchLength = 18;
            List<int>[] OffsetList;

            public LZ00Dictionary()
            {
                // Create the offset list
                OffsetList = new List<int>[0x100];
                for (int i = 0; i < OffsetList.Length; i++)
                    OffsetList[i] = new List<int>();

                // "Fill" buffer with null bytes
                for (int i = 0; i < BufferSize; i++)
                    OffsetList[0].Add(i - BufferSize);
            }

            public bool Search(byte[] inBuffer, byte[] buffer, int srcLength, int srcPointer, out int outOffset, out int outLength)
            {
                outOffset = 0;
                outLength = 0;

                // If the window isn't large enough, don't bother searching
                if (srcLength - srcPointer < MinMatchLength)
                    return false;

                // Start finding matches
                for (int i = OffsetList[inBuffer[srcPointer]].Count - 1; i >= 0; i--)
                {
                    int matchOffset = OffsetList[inBuffer[srcPointer]][i];
                    int matchLength = 1;

                    while (matchLength < MaxMatchLength &&
                        srcPointer + matchLength < srcLength &&
                        matchOffset + matchLength < srcPointer &&
                        inBuffer[srcPointer + matchLength] == buffer[(matchOffset + BufferStart + matchLength) & (BufferSize - 1)])
                    {
                        matchLength++;
                    }

                    // Is it good & better than our last match?
                    if (matchLength >= MinMatchLength && matchLength > outLength)
                    {
                        outOffset = (matchOffset + BufferStart) & (BufferSize - 1);
                        outLength = matchLength;

                        if (matchLength == MaxMatchLength) // If we can't do any better, stop looking for matches
                            break;
                    }
                }

                if (outLength >= MinMatchLength)
                    return true;
                else
                    return false;
            }

            // Add an entry
            public void AddEntry(byte[] buffer, byte index, int offset)
            {
                // First, let's remove the old entry at this position
                OffsetList[buffer[(offset + BufferStart) & (BufferSize - 1)]].RemoveAt(0);
                OffsetList[index].Add(offset);
            }
        }
    }
}