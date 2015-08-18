using Common.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Senders.LimitManager.JournalStorage
{
    public class FileJournalStorage : MemoryJournalStorage
    {
        //поля
        BasicFile _basicFile;
        ICommonLogger _logger;

                
        //инициализация
        public FileJournalStorage(ICommonLogger logger, string fullFileName = null)
        {
            _logger = logger;

            FileInfo journalFileInfo = new FileInfo(fullFileName);
            _basicFile = new BasicFile(journalFileInfo);
            _journal = ReadAllValuesFromFile();
        }



        //доступ к файлу
        protected virtual void AppendNewValuesToFile(List<DateTime> journalEntries)
        {
            Exception exception;
            _basicFile.AppendLineList<DateTime>(journalEntries, out exception);

            if (exception != null && _logger != null)
            {
                _logger.Exception(exception);
            }
        }

        protected virtual List<DateTime> ReadAllValuesFromFile()
        {
            Exception exception;
            List<string> lineList = _basicFile.ReadLineList(out exception);

            if (exception != null && _logger != null)
            {
                _logger.Exception(exception);
            }


            //преобразовать в даты
            List<DateTime> journalEntries = new List<DateTime>();
            
            foreach (string dateString in lineList)
            {
                DateTime dateTime;
                if (DateTime.TryParse(dateString, out dateTime))
                {
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    journalEntries.Add(dateTime);
                }
            }

            return journalEntries;
        }

        protected virtual void WritedAllValuesToFile(List<DateTime> journalEntries)
        {
            Exception exception;
            _basicFile.CreateLineList(journalEntries, out exception);
                            
            if (exception != null && _logger != null)
            {
                _logger.Exception(exception);
            }
        }
        



        //IJournalStorage
        public override void CleanJournal(TimeSpan deleteBeforePeriod)
        {
            base.CleanJournal(deleteBeforePeriod);
            WritedAllValuesToFile(_journal);
        }

        public override void InsertTime()
        {
            base.InsertTime();

            AppendNewValuesToFile(new List<DateTime>
            {
                DateTime.UtcNow
            });
        }
    }
}
