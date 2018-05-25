using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper
{
    public class ShouldSerializeContractResolver : DefaultContractResolver
    {
        //fields
        private List<string> _excludePropertyNames;


        //init
        public ShouldSerializeContractResolver(List<string> excludePropertyNames)
        {
            _excludePropertyNames = excludePropertyNames;
        }


        //methods
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (_excludePropertyNames.Contains(property.PropertyName))
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }
}
