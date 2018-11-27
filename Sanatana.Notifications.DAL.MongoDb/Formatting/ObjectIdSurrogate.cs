using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb.Formatting
{
    public class ObjectIdSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(
            object obj, SerializationInfo info, StreamingContext context)
        {
            MongoDB.Bson.ObjectId oid = (MongoDB.Bson.ObjectId)obj;
            var val = oid.ToString();
            info.AddValue("oid", val);

        }

        public object SetObjectData( object obj, SerializationInfo info, 
            StreamingContext context, ISurrogateSelector selector)
        {
            string val = info.GetString("oid");
            var result = new MongoDB.Bson.ObjectId(val);
            return result;
        }
    }
}
