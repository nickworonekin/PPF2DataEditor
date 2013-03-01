using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Linq;

namespace PPF2DataEditor
{
    public class ISO
    {
        private FileStream gameISO;
        public FileSystem FileSystem;

        private List<FileSystem.FileEntry> RawFileSystem;

        private const long ExeOffset     = 0x8F800;
        private const long DataBinOffset = 0x271800;
        private const long FileOffsets   = 0x1185D0;
        private const long FnameOffsets  = 0x140AF0;

        private const long DataBinLength = 1228800000;

        public ISO(string fname)
        {
            gameISO = new FileStream(fname, FileMode.Open, FileAccess.ReadWrite);

            FileSystem = new FileSystem();
            RawFileSystem = new List<FileSystem.FileEntry>();
            ReadFileSystem();
        }

        /// <summary>
        /// Reads the file system
        /// </summary>
        /// <param name="offset">Offset of the executable within the ISO</param>
        public void ReadFileSystem()
        {
            // Seek to the proper location
            gameISO.Seek(ExeOffset + FileOffsets, SeekOrigin.Begin);

            // Set up the current directory and directory list
            int CurrentDirIndex   = 0;
            int NumSubDirectories = 0;
            List<FileSystem.DirectoryEntry> DirList = new List<FileSystem.DirectoryEntry>();
            DirList.Add(FileSystem.Root); // Set the root directory as the first directory

            // Get directory and file location information
            int entryNumber = 0;
            do
            {
                uint EntryIdentifier = Extensions.ReadUInt32(gameISO);
                uint DirIdentifier   = Extensions.ReadUInt32(gameISO);
                uint FileOffset      = Extensions.ReadUInt32(gameISO) << 11; // Same as * 0x800
                uint FileLength      = Extensions.ReadUInt32(gameISO);

                bool IsDirectory = (EntryIdentifier != 0 && DirIdentifier != 0);
                bool IsFile      = (EntryIdentifier != 0 && DirIdentifier == 0);
                bool ChangeDir   = (EntryIdentifier == 0);

                // Read the filename data
                long streamPos = gameISO.Position;
                uint FilenameOffset = (uint)(FnameOffsets + (EntryIdentifier & 0xFFFF) - 2672);

                gameISO.Seek(ExeOffset + FilenameOffset, SeekOrigin.Begin);
                string entryFname = Extensions.ReadString(gameISO, Encoding.GetEncoding("Shift_JIS"));

                gameISO.Seek(streamPos, SeekOrigin.Begin);

                if (IsDirectory) // Add the directory
                {
                    DirList[CurrentDirIndex].AddDirectory(entryFname);
                    DirList.Insert(CurrentDirIndex + NumSubDirectories + 1, DirList[CurrentDirIndex].Directories[DirList[CurrentDirIndex].Directories.Count - 1]);
                    NumSubDirectories++;
                }
                else if (IsFile)
                {
                    FileSystem.FileEntry entry = new FileSystem.FileEntry(entryNumber, DirList[CurrentDirIndex], entryFname, FileOffset, FileLength);
                    DirList[CurrentDirIndex].AddFile(entry);
                    RawFileSystem.Add(entry);
                }
                else if (ChangeDir)
                {
                    CurrentDirIndex++;
                    NumSubDirectories = 0;
                }

                entryNumber++;
            }
            while (CurrentDirIndex < DirList.Count);
        }

        public void WriteFileSystem()
        {
            // We can either do it the proper way or do it the very easy and lazy way.
            // Let's do it the very easy and lazy way (by using RawFileSystem).
            foreach (FileSystem.FileEntry file in RawFileSystem)
            {
                gameISO.Seek(ExeOffset + FileOffsets + (file.EntryId * 16) + 8, SeekOrigin.Begin);
                Extensions.WriteUInt32(gameISO, file.Offset >> 11); // Same as / 0x800
                Extensions.WriteUInt32(gameISO, file.Length);
            }
        }

        public void ExportDirectory(FileSystem.DirectoryEntry entry, string path, BackgroundWorker bw)
        {
            if (bw != null)
                bw.ReportProgress(0, String.Empty);

            FileSystem.Traversal traversal = new FileSystem.Traversal(entry);
            int currentFileIndex = 0;
            int totalFiles = traversal.TotalFiles();

            FileSystem.FileEntry file;
            while ((file = traversal.Next()) != null)
            {
                string outputPath = Path.Combine(path, file.Directory.LocalPath);

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                if (bw != null)
                    bw.ReportProgress((int)((float)currentFileIndex / totalFiles * 100), Path.Combine(file.Directory.LocalPath, file.Name));

                ExportFile(file, outputPath);

                currentFileIndex++;
            }
        }

        public void ExportFile(FileSystem.FileEntry file, string path)
        {
            gameISO.Seek(DataBinOffset + file.Offset, SeekOrigin.Begin);

            string fname = Path.Combine(path, file.Name);
            using (FileStream output = File.Create(fname))
                Extensions.CopyStreamPart(gameISO, output, (int)file.Length);
        }

        public void ExportExecutable(string fname)
        {
            gameISO.Seek(ExeOffset, SeekOrigin.Begin);
            using (FileStream output = File.Create(fname))
                Extensions.CopyStreamPart(gameISO, output, 1365424);
        }

        public bool ImportExecutable(string fname)
        {
            gameISO.Seek(ExeOffset, SeekOrigin.Begin);
            using (FileStream input = File.OpenRead(fname))
            {
                if (input.Length != 1365424)
                    return false;

                Extensions.CopyStream(input, gameISO);
            }

            // Copy the file offsets from the original executable to the new one
            WriteFileSystem();

            return true;
        }

        public void ExportAndExtractFile(FileSystem.FileEntry file, string path)
        {
            ExportFile(file, path);

            string fname = Path.Combine(path, file.Name);
            if (File.Exists(fname))
            {
                bool isMRG = file.Name.ToLower().EndsWith(".mrg") || file.Name.ToLower().EndsWith(".mrz"),
                     isTEX = file.Name.ToLower().EndsWith(".tex") || file.Name.ToLower().EndsWith(".tez"),
                     isSPK = file.Name.ToLower().EndsWith(".spk"),
                     isCompressed = file.Name.ToLower().EndsWith(".mrz") || file.Name.ToLower().EndsWith(".tez");

                Stream input = File.OpenRead(fname);

                // Check to see if the data needs to be decompressed first
                if (isCompressed)
                {
                    MemoryStream decompressedInput = new MemoryStream();
                    if (LZ00.Decompress(input, decompressedInput))
                    {
                        input.Close();
                        input = decompressedInput;
                        input.Position = 0;
                    }
                }

                string archivePath = Path.Combine(path, Path.GetFileNameWithoutExtension(file.Name));

                // Extract the archive
                if (isMRG)
                {
                    MRG.Extract(input, archivePath);

                    // Extract any embedded TEX archives in the MRG archive as well
                    string texArchive = Path.Combine(Path.Combine(path, Path.GetFileNameWithoutExtension(file.Name)), Path.GetFileNameWithoutExtension(file.Name) + ".TEX");
                    if (File.Exists(texArchive))
                    {
                        TEX.Extract(File.OpenRead(texArchive), Path.Combine(archivePath, "TEX"));
                    }
                }
                else if (isTEX)
                    TEX.Extract(input, archivePath);
                else if (isSPK)
                    SPK.Extract(input, archivePath);

                input.Close();
            }
        }

        public bool ImportFile(FileSystem.FileEntry file, string fname)
        {
            using (FileStream input = File.OpenRead(fname))
            {
                // Ok, let's first see if we can just replace the file where it currently is (optimal space only)
                if (Extensions.RoundUpTo(file.Length, 2048) == Extensions.RoundUpTo(input.Length, 2048))
                {
                    // Ok, just use the current entry space
                    file.Length = (uint)input.Length;

                    gameISO.Seek(ExeOffset + FileOffsets + (file.EntryId * 16) + 12, SeekOrigin.Begin);
                    Extensions.WriteUInt32(gameISO, file.Length);

                    // Now copy the data
                    gameISO.Seek(DataBinOffset + file.Offset, SeekOrigin.Begin);
                    Extensions.CopyStream(input, gameISO);
                }

                // Ok, let's try looking for the most optimal space
                else
                {
                    RawFileSystem.Remove(file);
                    long offset = FindEmptySpace((int)input.Length);

                    if (offset == -1 || offset + Extensions.RoundUpTo(input.Length, 2048) > DataBinLength)
                    {
                        RawFileSystem.Add(file);
                        return false;
                    }
                    else
                    {
                        // Update the entries and re-add back to the raw file system
                        file.Offset = (uint)offset;
                        file.Length = (uint)input.Length;

                        gameISO.Seek(ExeOffset + FileOffsets + (file.EntryId * 16) + 8, SeekOrigin.Begin);
                        Extensions.WriteUInt32(gameISO, (uint)(file.Offset) >> 11);
                        Extensions.WriteUInt32(gameISO, file.Length);

                        // Now copy the data
                        gameISO.Seek(DataBinOffset + file.Offset, SeekOrigin.Begin);
                        Extensions.CopyStream(input, gameISO);

                        // Re-add the entry
                        RawFileSystem.Add(file);
                    }
                }
            }

            return true;
        }

        private long FindEmptySpace(int length)
        {
            // It's ok to sort the original raw file system since this is the only use we have for it
            RawFileSystem = RawFileSystem.OrderBy(x => x.Offset).ToList();
            /*RawFileSystem.Sort(delegate(FileSystem.FileEntry entry1, FileSystem.FileEntry entry2)
            {
                return entry1.Offset.CompareTo(entry2.Offset);
            });*/

            int paddedLength = Extensions.RoundUpTo(length, 2048);
            long offset = -1;
            for (int i = 1; i < RawFileSystem.Count; i++)
            {
                long testOffset = RawFileSystem[i - 1].Offset + Extensions.RoundUpTo(RawFileSystem[i - 1].Length, 2048);
                if (testOffset + paddedLength == RawFileSystem[i].Offset) // Best match we can get. Just use this offset
                {
                    offset = testOffset;
                    break;
                }
                if (testOffset + paddedLength < RawFileSystem[i].Offset) // A possible offset but not the best. Just store it for now
                {
                    offset = testOffset;
                }
            }

            // If we have failed to find an offset, just place it after the last stored file
            if (offset == -1)
            {
                long testOffset = RawFileSystem[RawFileSystem.Count - 1].Offset + Extensions.RoundUpTo(RawFileSystem[RawFileSystem.Count - 1].Length, 2048);

                // Make sure we don't overflow
                if (testOffset + paddedLength <= DataBinLength)
                    offset = testOffset;
            }

            return offset;
        }

        public bool Rebuild(string path, bool quick, BackgroundWorker bw)
        {
            if (bw != null)
                bw.ReportProgress(0, String.Empty);

            // Create a buffer when we are doing a full rebuild so it can go quicker
            byte[] nullBuffer = null;
            if (!quick)
                nullBuffer = new byte[4096];

            // The first thing we want to do is make sure all the files exist.
            // So, let's loop through the file system.
            FileSystem.Traversal traversal;
            FileSystem.FileEntry file;

            traversal = new FileSystem.Traversal(FileSystem.Root);
            while ((file = traversal.Next()) != null)
            {
                if (!File.Exists(Path.Combine(path, Path.Combine(file.Directory.LocalPath, file.Name))))
                    return false;
            }

            // All the files exist, let's gather all the new offsets and lengths for the files and add them to the ISO
            uint offset = 0;

            traversal = new FileSystem.Traversal(FileSystem.Root);
            int currentFileIndex = 0;
            int totalFiles = traversal.TotalFiles();

            while ((file = traversal.Next()) != null)
            {
                if (bw != null)
                    bw.ReportProgress((int)((float)currentFileIndex / totalFiles * 100), Path.Combine(file.Directory.LocalPath, file.Name));

                using (FileStream input = File.OpenRead(Path.Combine(path, Path.Combine(file.Directory.LocalPath, file.Name))))
                {
                    // Update the entries and re-add back to the raw file system
                    file.Offset = (uint)offset;
                    file.Length = (uint)input.Length;

                    gameISO.Seek(ExeOffset + FileOffsets + (file.EntryId * 16) + 8, SeekOrigin.Begin);
                    Extensions.WriteUInt32(gameISO, (uint)(file.Offset) >> 11);
                    Extensions.WriteUInt32(gameISO, file.Length);

                    // Now copy the data
                    gameISO.Seek(DataBinOffset + file.Offset, SeekOrigin.Begin);
                    Extensions.CopyStream(input, gameISO);
                }

                // If we're doing a full rebuild, null out any old data
                if (!quick)
                    gameISO.Write(nullBuffer, 0, 2048 - ((int)file.Length % 2048));

                offset += Extensions.RoundUpTo(file.Length, 2048);

                currentFileIndex++;
            }

            // If we're doing a full rebuild, null out any old data
            if (!quick)
            {
                if (bw != null)
                    bw.ReportProgress(0, String.Empty);

                Thread t = new Thread(new ThreadStart(delegate()
                    {
                        while (true)
                        {
                            if (bw != null)
                                bw.ReportProgress((int)((float)(gameISO.Position - DataBinOffset - offset) / (DataBinLength - offset) * 100), (gameISO.Position - DataBinOffset).ToString("N0") + " of " + DataBinLength.ToString("N0") + " bytes written");

                            Thread.Sleep(500);
                        }
                    }
                ));
                t.Start();

                while ((gameISO.Position - DataBinOffset) < DataBinLength)
                {
                    if ((gameISO.Position - DataBinOffset) + nullBuffer.Length > DataBinLength)
                        gameISO.Write(nullBuffer, 0, (int)(DataBinLength - (gameISO.Position - DataBinOffset)));
                    else
                        gameISO.Write(nullBuffer, 0, nullBuffer.Length);
                }

                t.Abort();
            }

            return true;
        }

        public bool ImportFileFromDirectory(FileSystem.FileEntry file, string path)
        {
            string fext = Path.GetExtension(file.Name).ToUpper();

            // Is this a MRG or MRZ archive
            if (fext == ".MRG" || fext == ".MRZ")
            {
                if (Directory.Exists(Path.Combine(path, "TEX")))
                {
                    // This archive has an embedded TEX archive in it. Let's build it and add it.
                    IEnumerable<string> texFiles = Directory.GetFiles(Path.Combine(path, "TEX"))
                        .Where(s =>
                            s.ToUpper().EndsWith(".SVR") ||
                            s.ToUpper().EndsWith(".SVP") ||
                            s.ToUpper().EndsWith(".MOT") ||
                            s.ToUpper().EndsWith(".VAG")
                        );

                    using (FileStream texArchive = File.Create(Path.Combine(path, Path.GetFileNameWithoutExtension(file.Name) + ".TEX")))
                        TEX.Create(texFiles.ToArray(), texArchive);
                }

                IEnumerable<string> mrgFiles = Directory.GetFiles(path)
                    .Where(s =>
                        s.ToUpper().EndsWith(".SVR") ||
                        s.ToUpper().EndsWith(".SVP") ||
                        s.ToUpper().EndsWith(".MOT") ||
                        s.ToUpper().EndsWith(".VAG") ||
                        s.ToUpper().EndsWith(".TEX")
                    );

                if (fext == ".MRG")
                {
                    using (FileStream mrgArchive = File.Create(Path.Combine(Directory.GetParent(path).ToString(), file.Name)))
                        MRG.Create(mrgFiles.ToArray(), mrgArchive);
                }
                else if (fext == ".MRZ")
                {
                    using (FileStream mrgArchive = File.Create(Path.Combine(Directory.GetParent(path).ToString(), file.Name)))
                    {
                        using (MemoryStream tempOutput = new MemoryStream())
                        {
                            MRG.Create(mrgFiles.ToArray(), tempOutput);
                            LZ00.Compress(tempOutput, mrgArchive, file.Name);
                        }
                    }
                }
            }

            // Is this a TEX or TEZ archive
            else if (fext == ".TEX" || fext == ".TEZ")
            {
                IEnumerable<string> texFiles = Directory.GetFiles(path)
                    .Where(s =>
                        s.ToUpper().EndsWith(".SVR") ||
                        s.ToUpper().EndsWith(".SVP") ||
                        s.ToUpper().EndsWith(".MOT") ||
                        s.ToUpper().EndsWith(".VAG")
                    );

                if (fext == ".TEX")
                {
                    using (FileStream texArchive = File.Create(Path.Combine(Directory.GetParent(path).ToString(), file.Name)))
                        TEX.Create(texFiles.ToArray(), texArchive);
                }
                else if (fext == ".TEZ")
                {
                    using (FileStream texArchive = File.Create(Path.Combine(Directory.GetParent(path).ToString(), file.Name)))
                    {
                        using (MemoryStream tempOutput = new MemoryStream())
                        {
                            TEX.Create(texFiles.ToArray(), tempOutput);
                            LZ00.Compress(tempOutput, texArchive, file.Name);
                        }
                    }
                }
            }

            // Is this a SPK archive
            else if (fext == ".SPK")
            {
                IEnumerable<string> spkFiles = Directory.GetFiles(path)
                    .Where(s =>
                        s.ToUpper().EndsWith(".LST") ||
                        s.ToUpper().EndsWith(".HD") ||
                        s.ToUpper().EndsWith(".BD")
                    );

                using (FileStream spkArchive = File.Create(Path.Combine(Directory.GetParent(path).ToString(), file.Name)))
                    TEX.Create(spkFiles.ToArray(), spkArchive);
            }

            // Should never reach this condition, but you never know
            else
                return false;

            // Now we can import this file
            return ImportFile(file, Path.Combine(Directory.GetParent(path).ToString(), file.Name));
        }
    }
}