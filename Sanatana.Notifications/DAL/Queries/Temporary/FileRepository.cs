using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Sanatana.Notifications.DAL.Queries
{
    public class FileRepository
    {
        //init
        public FileRepository()
        {
        }


        //methods
        public virtual void CreateBinary<T>(string filePath, T item)
        {
            FileInfo fileInfo = CreateDirectoryAndGetFile(filePath);
            if (fileInfo.Exists)
                fileInfo.Delete();

            var formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Append
                , FileAccess.Write, FileShare.ReadWrite))
            {
                formatter.Serialize(fileStream, item);
            }
        }

        public virtual T ReadBinary<T>(string filePath)
        {
            T item = default(T);
            
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
                return item;

            var formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open
               , FileAccess.Read, FileShare.ReadWrite))
            {
                item = (T)formatter.Deserialize(fileStream);
            }

            return item;
        }

        public virtual FileInfo[] GetAllFiles(string folderPath, string searchPattern)
        {
            FileInfo[] files = new FileInfo[0];

            DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
            if (!dirInfo.Exists)
            {
                return files;
            }

            files = dirInfo.GetFiles(searchPattern);

            return files;
        }

        public virtual void Delete(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
        }


        //common methods
        protected virtual FileInfo CreateDirectoryAndGetFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);

            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            return fileInfo;
        }
    }
}
