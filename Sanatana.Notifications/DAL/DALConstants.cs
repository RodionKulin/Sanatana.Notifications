using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL
{
    internal static class DALConstants
    {
        public const string TEMP_SIGNAL_FOLDER = "TemporarySignalsStorage";
        public const string TEMP_FILE_EXTENSION = ".bin";
        public const string TEMP_FILE_SEARCH_PATTERN = "*.bin";
        public const string TEMP_FILE_Id_REGEX_PATTERN = @"\w+-\d+(?:\.\d+){3}-([A-Za-z0-9-_]{22})\.bin";


    }
}
