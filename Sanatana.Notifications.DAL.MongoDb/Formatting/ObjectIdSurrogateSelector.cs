using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sanatana.Notifications.DAL.MongoDb.Formatting
{
    public class ObjectIdSurrogateSelector : SurrogateSelector
    {
        public override ISerializationSurrogate GetSurrogate(
          Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (type == typeof(MongoDB.Bson.ObjectId))
            {
                selector = this;
                return new ObjectIdSurrogate();
            }

            return base.GetSurrogate(type, context, out selector);
        }

    }
}
