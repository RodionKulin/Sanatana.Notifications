using Sanatana.Notifications.DAL.Queries;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb.Formatting
{
    public class ObjectIdFileRepository : FileRepository
    {
        protected override IFormatter GetFormatter()
        {
            return new BinaryFormatter(new ObjectIdSurrogateSelector(), new StreamingContext());
        }
    }
}
