using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Sanatana.Notifications.DAL.Queries
{
    public class FileJournalQueries
    {
        //fields
        protected FileInfo _fileInfo;
        protected ReaderWriterLockSlim _fileLocker;
        protected bool _directoryCreated;


        //init
        public FileJournalQueries(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
            _fileLocker = new ReaderWriterLockSlim();
        }


        //methods
        protected virtual FileInfo CreateDirectoryAndGetFile(bool createDirectory)
        {
            _fileInfo.Directory.Refresh();
            bool directoryCreated = _fileInfo.Directory.Exists;

            if (!directoryCreated && createDirectory)
            {
                _fileInfo.Directory.Create();
            }

            _fileInfo.Refresh();
            return _fileInfo;
        }

        public virtual void CreateLineList<T>(List<T> lines)
        {
            try
            {
                _fileLocker.EnterWriteLock();

                FileInfo fileInfo = CreateDirectoryAndGetFile(true);
                if (fileInfo.Exists)
                    fileInfo.Delete();

                using (FileStream fileStream = new FileStream(fileInfo.FullName
                    , FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    foreach (T line in lines)
                    {
                        streamWriter.WriteLine(line.ToString());
                    }
                }
            }
            finally
            {
                _fileLocker.ExitWriteLock();
            }
        }

        public virtual void AppendLineList<T>(List<T> lines)
        {
            try
            {
                _fileLocker.EnterWriteLock();

                FileInfo fileInfo = CreateDirectoryAndGetFile(true);

                using (FileStream fileStream = new FileStream(fileInfo.FullName
                    , FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    foreach (T line in lines)
                    {
                        streamWriter.WriteLine(line.ToString());
                    }
                }

            }
            finally
            {
                _fileLocker.ExitWriteLock();
            }
        }

        public virtual List<string> ReadLineList()
        {
            List<string> lines = new List<string>();
            
            try
            {
                _fileLocker.EnterReadLock();

                FileInfo fileInfo = CreateDirectoryAndGetFile(false);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                    return lines;

                using (FileStream fileStream = new FileStream(fileInfo.FullName
                    , FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (!streamReader.EndOfStream)
                    {
                        string line = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            continue;

                        lines.Add(line);
                    }
                }
            }
            finally
            {
                _fileLocker.ExitReadLock();
            }

            return lines;
        }
    }
}
