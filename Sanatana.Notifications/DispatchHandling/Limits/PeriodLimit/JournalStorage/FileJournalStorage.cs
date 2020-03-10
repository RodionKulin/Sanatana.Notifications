using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DispatchHandling.Limits.JournalStorage
{
    public class FileJournalStorage : MemoryJournalStorage
    {
        //fields
        protected FileJournalQueries _fileQueries;

                
        //init
        public FileJournalStorage(string fullFileName = null)
        {
            FileInfo journalFileInfo = new FileInfo(fullFileName);
            _fileQueries = new FileJournalQueries(journalFileInfo);
            _journal = ReadAllValuesFromFile();
        }



        //access to file
        protected virtual void AppendNewValuesToFile(List<DateTime> journalEntries)
        {
            _fileQueries.AppendLineList<DateTime>(journalEntries);            
        }

        protected virtual List<DateTime> ReadAllValuesFromFile()
        {
            List<string> lineList = _fileQueries.ReadLineList();
            
            //convert to date
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
            _fileQueries.CreateLineList(journalEntries);
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
