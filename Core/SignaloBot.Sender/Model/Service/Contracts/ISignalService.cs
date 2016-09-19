using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.Sender.Service
{
    [ServiceContract]
    public interface ISignalService<TKey>
        where TKey : struct
    {
        [OperationContract]
        Task RaiseKeyValueEvent(TKey? groupID, TKey? userID, int categoryID, string topicID, Dictionary<string, string> values);
        
    }
    
}
