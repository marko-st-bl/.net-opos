using DokanNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Zadatak4.BTreeDictionary;
using Zadatak4.BPath;
using DokanNet.Logging;

namespace Zadatak4
{
    public class MSFS : IDokanOperations
    {
        const string ROOT_DIRECTORY = "\\";
        //Root folder
        static DokanDirectory _root = new DokanDirectory(null, "");

        private readonly static int MAX_FILE_SIZE = 32 * 1024 * 1024;
        private readonly static int CAPACITY = 512 * 1024 * 1024;
        private readonly static int DEGREE = 3;

        //Locker object
        private readonly static object _locker = new object();

        private ConsoleLogger logger = new ConsoleLogger("[MSFS] ");

        private readonly BTreeDictionary<DokanPath, DokanFile> files = new BTreeDictionary<DokanPath, DokanFile>(DEGREE);
        private readonly BTreeDictionary<DokanPath, DokanDirectory> dirs = new BTreeDictionary<DokanPath, DokanDirectory>(DEGREE);

        public MSFS(string path)
        {
            dirs.Add(new BPath.DokanPath() { FilePath = "" }, new DokanDirectory());
            dirs.Add(new BPath.DokanPath() { FilePath = "\\Serialized" }, new DokanDirectory());
        }
        // Free space (in bytes).
        private int freeBytesAvailable = CAPACITY;

        private string GetPath(string fileName)
        {
            return ROOT_DIRECTORY + fileName;
        }

        private string GetFileNamePart(string fileName)
        {
            return fileName.Substring(fileName.LastIndexOf(@"\") + 1);
        }

        //Implementation of IDokanOperations
        public void Cleanup(string fileName, IDokanFileInfo info)
        {
            if(info.DeleteOnClose == true)
            {
                DokanPath filePath = new DokanPath() { FilePath = fileName };
                files.Remove(filePath);
                string parent = fileName.Substring(0, fileName.LastIndexOf(@"\"));
                DokanDirectory dir;
                if(dirs.TryGetValue(new DokanPath() { FilePath = parent}, out dir))
                {
                    dir.Content.Remove(fileName);
                }
            }
        }

        public void CloseFile(string fileName, IDokanFileInfo info)
        {
            
        }

        public NtStatus CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, IDokanFileInfo info)
        {
            if (fileName == ROOT_DIRECTORY)
                return NtStatus.Success;

            if (access == DokanNet.FileAccess.ReadAttributes && mode == FileMode.Open)
                return NtStatus.Success;

            if (fileName == "\\Serialized")
                return NtStatus.Success;

            string path = GetPath(fileName);

            if(mode == FileMode.CreateNew)
            {
                DokanPath filePath = new DokanPath() { FilePath = fileName };
                if (attributes == FileAttributes.Directory || info.IsDirectory)
                {
                    DokanPath dirPath = new DokanPath() { FilePath = fileName };
                    if(!dirs.ContainsKey(dirPath))
                    {
                        string parentPath = fileName.Substring(0, fileName.LastIndexOf(@"\"));

                        BPath.DokanDirectory dir;
                        if (dirs.TryGetValue(new BPath.DokanPath() { FilePath = parentPath }, out dir))
                        {
                            dirs.Add(dirPath, new DokanDirectory(dir, GetFileNamePart(fileName)));
                            dir.Content.Add(filePath.FilePath);
                        }
                    }
                }
                else if(!files.ContainsKey(filePath))
                {
                    BPath.DokanFile file = new DokanFile() { CreationTime = DateTime.Now, LastWriteTime = DateTime.Now };
                    files.Add(filePath, file);
                    string parentPath = fileName.Substring(0, fileName.LastIndexOf("\\"));

                    BPath.DokanDirectory dir;
                    if(dirs.TryGetValue(new BPath.DokanPath() { FilePath = parentPath}, out dir))
                    {
                        dir.Content.Add(filePath.FilePath);
                    }
                }
            }
            else if(mode == FileMode.Append)
            {
                return NtStatus.AccessDenied;
            }
            return NtStatus.Success;
        }

        public NtStatus DeleteDirectory(string fileName, IDokanFileInfo info)
        {
            if (!info.IsDirectory)
                return NtStatus.Error;
            // DeleteOnClose gets or sets a value indicating whether the file has to be deleted during the IDokanOperations.Cleanup event. 
            info.DeleteOnClose = true;
            return NtStatus.Success;
        }

        public NtStatus DeleteFile(string fileName, IDokanFileInfo info)
        {
            if (info.IsDirectory)
                return NtStatus.Error;
            // DeleteOnClose gets or sets a value indicating whether the file has to be deleted during the IDokanOperations.Cleanup event. 
            info.DeleteOnClose = true;
            return NtStatus.Success;
        }

        public NtStatus FindFiles(string dirPathName, out IList<FileInformation> foundFiles, IDokanFileInfo info)
        {
            foundFiles = new List<FileInformation>();
            Console.WriteLine("PATH_NAME: {0}", dirPathName);
            if (dirPathName == @"\")
                dirPathName = "";
            /*if (!dirs.ContainsKey(new BPath.DokanPath() { FilePath = dirPathName }))
                return NtStatus.ObjectNameNotFound;*/
            if(dirPathName == "\\Serialized")
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var memoryStream = new MemoryStream())
                {
                    binaryFormatter.Serialize(memoryStream, files);
                    FileInformation fileInfo = new FileInformation();
                    fileInfo.FileName = "ser.bin";
                    fileInfo.Length = memoryStream.ToArray().Length;
                    fileInfo.CreationTime = DateTime.Now;
                    fileInfo.LastWriteTime = DateTime.Now;
                    foundFiles.Add(fileInfo);
                    return NtStatus.Success;
                }
            }
            DokanDirectory dir;
            if(dirs.TryGetValue(new BPath.DokanPath() { FilePath = dirPathName }, out dir))
            {
                foreach(var path in dir.Content)
                {
                    BPath.DokanFile file;
                    BPath.DokanDirectory directory;
                    if(files.TryGetValue(new BPath.DokanPath() { FilePath = path}, out file))
                    {
                        FileInformation fileInfo = new FileInformation();
                        fileInfo.FileName = System.IO.Path.GetFileName(path);
                        fileInfo.Length = fileInfo.FileName.Length;
                        fileInfo.CreationTime = file.CreationTime;
                        fileInfo.LastWriteTime = file.LastWriteTime;
                        fileInfo.Attributes = FileAttributes.ReadOnly;
                        foundFiles.Add(fileInfo);
                    }
                    else if(dirs.TryGetValue(new DokanPath() { FilePath = path }, out directory))
                    {
                        FileInformation fileInfo = new FileInformation();
                        fileInfo.FileName = directory.Name;
                        fileInfo.Attributes = FileAttributes.Directory;
                        fileInfo.Length = path.Length;
                        fileInfo.CreationTime = directory.CreationTime;
                        fileInfo.LastWriteTime = directory.LastWriteTime;
                        fileInfo.Attributes = directory.Attributes;
                        foundFiles.Add(fileInfo);
                    }
                }
            }
            return NtStatus.Success;
        }

        public NtStatus FindFilesWithPattern(string fileName, string searchPattern, out IList<FileInformation> files, IDokanFileInfo info)
        {
            files = new FileInformation[0];
            return NtStatus.NotImplemented;
        }

        public NtStatus FindStreams(string fileName, out IList<FileInformation> streams, IDokanFileInfo info)
        {
            streams = new FileInformation[0];
            return NtStatus.NotImplemented;
        }

        public NtStatus FlushFileBuffers(string fileName, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus GetDiskFreeSpace(out long freeBytesAvailable, out long totalNumberOfBytes, out long totalNumberOfFreeBytes, IDokanFileInfo info)
        {
            freeBytesAvailable = this.freeBytesAvailable;
            totalNumberOfFreeBytes = this.freeBytesAvailable;
            totalNumberOfBytes = CAPACITY;
            return NtStatus.Success;
        }

        public NtStatus GetFileInformation(string fileName, out FileInformation fileInfo, IDokanFileInfo info)
        {
            if(fileName == "\\Serialized\\ser.bin")
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var memoryStream = new MemoryStream())
                {
                    binaryFormatter.Serialize(memoryStream, files);
                    fileInfo = new FileInformation()
                    {
                        FileName = "ser.bin",
                        Length = memoryStream.ToArray().Length * 8,
                        Attributes = FileAttributes.ReadOnly,
                        CreationTime = DateTime.Now,
                        LastWriteTime = DateTime.Now
                    };

                }
                return NtStatus.Success;
            }

            BPath.DokanFile file;
            DokanDirectory dir;
            DokanPath path = new DokanPath() { FilePath = fileName };
            if(files.TryGetValue(path, out file))
            {
                fileInfo = new FileInformation()
                {
                    FileName = System.IO.Path.GetFileName(fileName),
                    Length = file.Bytes.Length * 8,
                    Attributes = FileAttributes.ReadOnly,
                    CreationTime = file.CreationTime,
                    LastWriteTime = file.LastWriteTime
                    
                };
            }
            else if(dirs.TryGetValue(path, out dir))
            {
                fileInfo = new FileInformation()
                {
                    FileName = System.IO.Path.GetDirectoryName(fileName),
                    Length = dir.Content.Count(),
                    Attributes = FileAttributes.Directory,
                    CreationTime = dir.CreationTime,
                    LastWriteTime = dir.LastWriteTime
                };
            }
            else
            {
                fileInfo = default(FileInformation);
                return NtStatus.Error;
            }
            return NtStatus.Success;
        }

        public NtStatus GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            security = null;
            return NtStatus.Success;
        }

        public NtStatus GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, out uint maximumComponentLength, IDokanFileInfo info)
        {
            volumeLabel = "MSFS";
            features = FileSystemFeatures.None;
            fileSystemName = "MarkoStojanovicFileSystem";
            maximumComponentLength = 255;
            return NtStatus.Success;
        }

        public NtStatus LockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus Mounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus MoveFile(string oldName, string newName, bool replace, IDokanFileInfo info)
        {
            //Console.WriteLine("OLD NAME: {0}; NEW NAME: {1}; REPLACE: {2}", oldName, newName, replace);
            if (replace)
                return NtStatus.NotImplemented;

            if (oldName == newName)
                return NtStatus.Success;

            DokanPath filePath = new DokanPath() { FilePath = oldName };
            DokanPath newPath = new DokanPath() { FilePath = newName };

            BPath.DokanFile file;
            BPath.DokanDirectory dir;
            string oldParentPath = oldName.Substring(0, oldName.LastIndexOf(@"\"));
            string newParentPath = newName.Substring(0, newName.LastIndexOf(@"\"));

            // Moving a file
            if (files.TryGetValue(filePath, out file))
            {
                if (dirs.TryGetValue(new BPath.DokanPath() { FilePath = oldParentPath }, out dir))
                {
                    dir.Content.Remove(filePath.FilePath);
                    if(oldParentPath == newParentPath)
                    {
                        dir.Content.Add(newPath.FilePath);
                    } else
                    {
                        if (dirs.TryGetValue(new BPath.DokanPath() { FilePath = newParentPath }, out dir))
                        {
                            dir.Content.Add(newPath.FilePath);
                        }
                    }

                }

                files.Remove(filePath);
                files.Add(newPath, file);
                return NtStatus.Success;
            }

            // Moving directory
            if(dirs.TryGetValue(filePath, out dir))
            {
                BPath.DokanPath newDir = new DokanPath() { FilePath = newName, IsDirectory = true };
                if (dirs.TryGetValue(new BPath.DokanPath() { FilePath = oldParentPath }, out dir))
                {
                    dir.Content.Remove(filePath.FilePath);
                    if (oldParentPath == newParentPath)
                    {
                        dir.Content.Add(newPath.FilePath);
                    }
                    else
                    {
                        if (dirs.TryGetValue(new BPath.DokanPath() { FilePath = newParentPath }, out dir))
                        {
                            dir.Content.Add(newPath.FilePath);
                        }
                    }
                    dirs.Remove(filePath);
                    dirs.Add(newDir, new DokanDirectory(dir, GetFileNamePart(newName)));
                    Console.WriteLine("CONTENT: {0}", dir.Content[0]);
                }
                return NtStatus.Success;
            }

                return NtStatus.Success;
        }

        public NtStatus ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, IDokanFileInfo info)
        {
            lock(_locker)
            {
                int offsetInt = (int)offset;
                if(fileName == "\\Serialized\\ser.bin")
                {
                    var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    using (var memoryStream = new MemoryStream())
                    {
                        binaryFormatter.Serialize(memoryStream, files);
                        memoryStream.ToArray().CopyTo(buffer, 0);
                        bytesRead = memoryStream.ToArray().Length;
                    }                    
                    return NtStatus.Success;   
                }

                DokanFile existingFile;
                if(files.TryGetValue(new DokanPath() { FilePath = fileName }, out existingFile))
                {
                    /* existingFile.Bytes.Skip(offsetInt).Take(buffer.Length).ToArray().CopyTo(buffer, 0);
                     int diff = existingFile.Bytes.Length - offsetInt;
                     bytesRead = buffer.Length > diff ? diff : buffer.Length;
                     return NtStatus.Success;*/


                    byte[] fileBytes = existingFile.Bytes;
                    int i = 0;
                    for (; i < fileBytes.Length && i < buffer.Length; i++)
                    {
                        buffer[i] = fileBytes[i];
                    }
                    bytesRead = i;
                    return NtStatus.Success;
                }
                bytesRead = 0;
                return NtStatus.Error;
            }
        }

        public NtStatus SetAllocationSize(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus SetEndOfFile(string fileName, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus SetFileAttributes(string fileName, FileAttributes attributes, IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus UnlockFile(string fileName, long offset, long length, IDokanFileInfo info)
        {
            return NtStatus.Error;
        }

        public NtStatus Unmounted(IDokanFileInfo info)
        {
            return NtStatus.Success;
        }

        public NtStatus WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offsetLong, IDokanFileInfo info)
        {
            bytesWritten = 0;

            if (buffer.Length + offsetLong > MAX_FILE_SIZE)
            {
                bytesWritten = 0;
                return NtStatus.FileTooLarge;
            }

            int offset = unchecked((int)offsetLong);

            if(fileName == "\\Serialized\\ser.bin")
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                using (var memoryStream = new MemoryStream())
                {
                    binaryFormatter.Serialize(memoryStream, files);
                    buffer = memoryStream.ToArray();
                    bytesWritten = buffer.Length;
                }
                return NtStatus.Success;
            }

            DokanFile file;
            if(files.TryGetValue(new DokanPath() { FilePath = fileName }, out file))
            {
                //TODO: Disable file editing
                if(file.Bytes.Length > 0)
                {
                    Console.WriteLine("CANNOT EDIT FILE!!!");
                    return NtStatus.Error;
                }

                lock(_locker)
                {
                    /* if (offset > file.Bytes.Length)
                     {
                         bytesWritten = 0;
                         return NtStatus.ArrayBoundsExceeded;
                     }*/

                    /*if (info.WriteToEndOfFile)
                    {
                        return NtStatus.AccessDenied;
                    }*/

                    /*file.Bytes = file.Bytes.Take(offset).Concat(buffer).ToArray();
                    bytesWritten = buffer.Length;
                    int difference = file.Bytes.Length - offset;
                    freeBytesAvailable += difference;*/


                    file.Bytes = new byte[buffer.Length];
                    int i = 0;
                    for (; i < buffer.Length; i++)
                    {
                        file.Bytes[i] = buffer[i];
                    }
                    bytesWritten = i;
                    freeBytesAvailable -= bytesWritten;
                }
            }
            if (file != null)
                file.LastWriteTime = DateTime.Now;
            return NtStatus.Success;
        }
    }
}
