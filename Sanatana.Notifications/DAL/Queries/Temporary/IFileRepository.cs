using System.IO;

namespace Sanatana.Notifications.DAL.Queries
{
    public interface IFileRepository
    {
        void CreateBinary<T>(string filePath, T item);
        void Delete(string filePath);
        FileInfo[] GetAllFiles(string folderPath, string searchPattern);
        T ReadBinary<T>(string filePath);
    }
}