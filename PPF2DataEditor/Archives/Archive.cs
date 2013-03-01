using System;
using System.IO;
using System.Text;

namespace PPF2DataEditor
{
    public static class Archive
    {
        public static bool Create(ArchiveType format, string[] inFiles, Stream output, bool compress)
        {
            // Write the header
            byte[] header;

            if (format == ArchiveType.MRG)
                header = new byte[] { (byte)'M', (byte)'R', (byte)'G', (byte)'0' };
            else if (format == ArchiveType.SPK)
                header = new byte[] { (byte)'S', (byte)'N', (byte)'D', (byte)'0' };
            else if (format == ArchiveType.TEX)
                header = new byte[] { (byte)'T', (byte)'E', (byte)'X', (byte)'0' };
            else
                return false;

            output.Write(header, 0, 4);

            // Write the number of files
            Extensions.WriteInt32(output, inFiles.Length);

            // Write out the 8 null bytes
            output.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 8);

            // Write out the header for the archive
            uint offset = 0x10 + (uint)(inFiles.Length * 0x30);
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

                if (format == ArchiveType.MRG)
                {
                    output.Write(new byte[] { 0, 0, 0, 0 }, 0, 4);
                    Extensions.WriteString(output, fname, Encoding.GetEncoding("Shift_JIS"), 32);
                }
                else
                {
                    Extensions.WriteString(output, fname, Encoding.GetEncoding("Shift_JIS"), 20);
                }

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

            // Get the type of archive this is
            ArchiveType format;

            byte[] header = new byte[4];
            input.Read(header, 0, 4);

            if (header[0] == 'M' && header[1] == 'R' && header[2] == 'G' && header[3] == '0')
                format = ArchiveType.MRG;
            else if (header[0] == 'S' && header[1] == 'N' && header[2] == 'D' && header[3] == '0')
                format = ArchiveType.SPK;
            else if (header[0] == 'T' && header[1] == 'E' && header[2] == 'X' && header[3] == '0')
                format = ArchiveType.TEX;
            else
                return false;

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

                string fname;
                if (format == ArchiveType.MRG)
                {
                    input.Position += 4;
                    fname = Extensions.ReadString(input, Encoding.GetEncoding("Shift_JIS"), 32);
                }
                else
                {
                    fname = Extensions.ReadString(input, Encoding.GetEncoding("Shift_JIS"), 20);
                }

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

    public enum ArchiveType
    {
        Unknown,
        MRG,
        SPK,
        TEX,
    }
}