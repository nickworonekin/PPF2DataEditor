using System;
using System.IO;
using System.Collections.Generic;

namespace PPF2DataEditor
{
    public sealed class FileSystem
    {
        public DirectoryEntry Root;

        public FileSystem()
        {
            Root = new DirectoryEntry(String.Empty, null);
        }

        public sealed class DirectoryEntry
        {
            public DirectoryEntry Parent;
            public List<DirectoryEntry> Directories;
            public List<FileEntry> Files;

            public string Name;
            public string LocalPath;

            public DirectoryEntry(string name, DirectoryEntry parent)
            {
                Parent = parent;
                Directories = new List<DirectoryEntry>();
                Files = new List<FileEntry>();

                Name = name;

                if (parent == null)
                    LocalPath = Name;
                else
                    LocalPath = Path.Combine(parent.LocalPath, Name);
            }

            public void AddDirectory(string name)
            {
                Directories.Add(new DirectoryEntry(name, this));
            }

            public void AddFile(FileEntry entry)
            {
                Files.Add(entry);
            }
            public void AddFile(int id, string name, uint offset, uint length)
            {
                Files.Add(new FileEntry(id, this, name, offset, length));
            }
        }

        public sealed class FileEntry
        {
            public int EntryId;
            public DirectoryEntry Directory;
            public string Name;

            public uint Offset;
            public uint Length;

            public FileEntry(int id, DirectoryEntry directory, string name, uint offset, uint length)
            {
                EntryId   = id;
                Directory = directory;
                Name      = name;

                Offset = offset;
                Length = length;
            }
        }

        public sealed class Traversal
        {
            int currentFileIndex;
            DirectoryEntry rootDirectory;
            DirectoryEntry currentDirectory;
            Queue<DirectoryEntry> directories;

            public Traversal(DirectoryEntry root)
            {
                rootDirectory = root;
                currentDirectory = root;

                directories = new Queue<DirectoryEntry>();
                currentFileIndex = 0;
            }

            public FileEntry Next()
            {
                if (currentFileIndex >= currentDirectory.Files.Count)
                    NextDirectory();

                if (currentFileIndex == -1 || currentDirectory == null)
                    return null;

                FileEntry file = currentDirectory.Files[currentFileIndex];
                currentFileIndex++;

                return file;
            }

            private void NextDirectory()
            {
                foreach (DirectoryEntry directory in currentDirectory.Directories)
                    directories.Enqueue(directory);

                if (directories.Count == 0)
                {
                    currentDirectory = null;
                    currentFileIndex = -1;
                }
                else
                {
                    currentDirectory = directories.Dequeue();
                    currentFileIndex = 0;
                }
            }

            public int TotalFiles()
            {
                int total = 0;

                Queue<DirectoryEntry> localDirList = new Queue<DirectoryEntry>();
                localDirList.Enqueue(rootDirectory);

                while (localDirList.Count > 0)
                {
                    DirectoryEntry localCurrentDirectory = localDirList.Dequeue();
                    total += localCurrentDirectory.Files.Count;

                    foreach (DirectoryEntry directory in localCurrentDirectory.Directories)
                        localDirList.Enqueue(directory);
                }

                return total;
            }
        }
    }
}