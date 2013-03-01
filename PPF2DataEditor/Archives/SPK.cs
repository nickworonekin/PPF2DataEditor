using System;
using System.IO;
using System.Text;

namespace PPF2DataEditor
{
    public static class SPK
    {
        public static bool Create(string[] inFiles, Stream output)
        {
            // Write the header
            output.WriteByte((byte)'S');
            output.WriteByte((byte)'N');
            output.WriteByte((byte)'D');
            output.WriteByte((byte)'0');

            // Write the number of files
            Extensions.WriteInt32(output, inFiles.Length);

            // Write out the 8 null bytes
            output.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 8);

            // Write out the header for the archive
            uint offset = 0x10 + (uint)(inFiles.Length * 0x20);
            for (int i = 0; i < inFiles.Length; i++)
            {
                int length = (int)(new FileInfo(inFiles[i]).Length);
                string fname = Path.GetFileNameWithoutExtension(inFiles[i]);
                string fext = Path.GetExtension(inFiles[i]);
                if (fext.StartsWith("."))
                    fext = fext.Substring(1);

                Extensions.WriteString(output, fext, Encoding.GetEncoding("Shift_JIS"), 4);
                Extensions.WriteUInt32(output, offset); // Offset
                Extensions.WriteInt32(output, length); // Length
                Extensions.WriteString(output, fname, Encoding.GetEncoding("Shift_JIS"), 20);

                offset += (uint)Extensions.RoundUpTo(length, 16);
            }

            // Write out the files
            for (int i = 0; i < inFiles.Length; i++)
            {
                using (FileStream input = File.OpenRead(inFiles[i]))
                {
                    Extensions.CopyStream(input, output);

                    while (input.Length % 16 != 0)
                        output.WriteByte(0);
                }
            }

            return true;
        }

        public static bool Extract(Stream input, string outPath)
        {
            long startOffset = input.Position;

            // Check to see if this is a SPK
            if (!(input.ReadByte() == 'S' && input.ReadByte() == 'N' && input.ReadByte() == 'D' && input.ReadByte() == '0'))
            {
                return false;
            }

            // Get the number of files in the archive
            int numFiles = Extensions.ReadInt32(input);

            // Create the directory to extract the files to
            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            // Read each file entry & extract the file
            input.Position += 8;
            for (int i = 0; i < numFiles; i++)
            {
                string fext = Extensions.ReadString(input, Encoding.GetEncoding("Shift_JIS"), 4);

                uint offset = Extensions.ReadUInt32(input);
                int length = Extensions.ReadInt32(input);

                string fname = Extensions.ReadString(input, Encoding.GetEncoding("Shift_JIS"), 20);

                // Now extract the file
                long currentOffset = input.Position;

                input.Position = startOffset + offset;
                using (FileStream output = File.Create(Path.Combine(outPath, fname + '.' + fext)))
                    Extensions.CopyStreamPart(input, output, length);

                input.Position = currentOffset;
            }

            return true;
        }
    }
}