using Sanatana.Notifications.Resources;
using Sanatana.Notifications.DAL.Interfaces;
using Sanatana.Notifications.DAL.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.Queries
{
    public class TemporaryStorage<TS> : ITemporaryStorage<TS>
    {
        //fields
        protected FileRepository _repository;
        protected string _tempFileFolder;
        protected Regex _fileIdRegex;
        

        //init
        public TemporaryStorage()
        {
            _repository = new FileRepository();
            _fileIdRegex = new Regex(DALConstants.TEMP_FILE_Id_REGEX_PATTERN);

            Assembly currentAssmebly = typeof(TemporaryStorage<TS>).Assembly;
            Uri codeBase = new Uri(currentAssmebly.CodeBase);
            FileInfo assemblyFileInfo = new FileInfo(codeBase.LocalPath);

            _tempFileFolder = Path.Combine(assemblyFileInfo.DirectoryName, DALConstants.TEMP_SIGNAL_FOLDER);
        }


        //file actions
        public virtual void Insert(TemporaryStorageParameters queueParams, Guid Id, TS item)
        {
            string filePath = GetFilePath(queueParams, Id);

            _repository.CreateBinary(filePath, item);
        }

        public virtual Dictionary<Guid, TS> Select(TemporaryStorageParameters queueParams)
        {
            var items = new Dictionary<Guid, TS>();
            
            string searchPattern = GetSearchPattern(queueParams);
            FileInfo[] files = _repository.GetAllFiles(_tempFileFolder, searchPattern);
            
            foreach (FileInfo file in files)
            {
                TS item = _repository.ReadBinary<TS>(file.FullName);
                Guid? Id = ParseIdFromFileName(queueParams, file.Name);

                if (item == null)
                {
                    string message = string.Format(SenderInternalMessages.Filerepository_CorruptedFile, file.Name);
                    throw new FileNotFoundException(message);
                }
                else if (Id.HasValue == false)
                {
                    string message = string.Format(SenderInternalMessages.Filerepository_InvalidGuidName, file.Name);
                    throw new FileNotFoundException(message);
                }

                items.Add(Id.Value, item);
            }

            return items;
        }

        public virtual void Update(TemporaryStorageParameters queueParams, Guid Id, TS item)
        {
            Insert(queueParams, Id, item);
        }

        public virtual void Delete(TemporaryStorageParameters queueParams, Guid Id)
        {
            string filePath = GetFilePath(queueParams, Id);
            _repository.Delete(filePath);            
        }

        public virtual void Delete(TemporaryStorageParameters queueParams, List<Guid> Ids)
        {
            foreach (Guid Id in Ids)
            {
                Delete(queueParams, Id);
            }
        }
        


        //file names 
        protected virtual string GetSearchPattern(TemporaryStorageParameters queueParams)
        {
            string version = queueParams.EntityVersion.ToString();
            string pattern = $"{queueParams.QueueType}-{version}-{DALConstants.TEMP_FILE_SEARCH_PATTERN}";
            return pattern;
        }

        protected virtual Guid? ParseIdFromFileName(TemporaryStorageParameters queueParams, string fileName)
        {
            Match match = _fileIdRegex.Match(fileName);
            if (match.Success == false
                || match.Groups.Count < 2
                || match.Groups[1].Success == false)
            {
                return null;
            }

            string shortGuidId = match.Groups[1].Value;
            try
            {
                Guid Id = ShortGuid.Decode(shortGuidId);
                return Id;
            }
            catch
            {

            }
            return null;
        }

        protected virtual string GetFilePath(TemporaryStorageParameters queueParams, Guid Id)
        {
            string version = queueParams.EntityVersion.ToString();
            string shortId = ShortGuid.Encode(Id);
            string fileName = $"{queueParams.QueueType}-{version}-{shortId}{DALConstants.TEMP_FILE_EXTENSION}";
            return Path.Combine(_tempFileFolder, fileName);
        }

        public virtual string GetStorageFolder()
        {
            return _tempFileFolder;
        }
    }
}
