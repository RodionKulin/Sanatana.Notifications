using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.DAL.EntityFrameworkCore.AutoMapper
{
    public class ToJsonValueResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, string>
    {
        //fields
        private static Dictionary<string, List<string>> _typeProperties = new Dictionary<string, List<string>>();


        //methods
        public string Resolve(TSource source, TDestination destination, string destMember, ResolutionContext context)
        {
            Type baseType = typeof(TSource);
            List<string> excludePropertyNames = GetEntityProperties(baseType);

            string json = JsonConvert.SerializeObject(source, new JsonSerializerSettings
            {
                ContractResolver = new ShouldSerializeContractResolver(excludePropertyNames),
                TypeNameHandling = TypeNameHandling.Objects
            });
            return json;
        }

        private List<string> GetEntityProperties(Type baseType)
        {
            if (_typeProperties.ContainsKey(baseType.AssemblyQualifiedName) == false)
            {
                PropertyInfo[] baseTypeProperties = baseType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                List<string> propertyNames = baseTypeProperties.Select(x => x.Name).ToList();
                _typeProperties.Add(baseType.AssemblyQualifiedName, propertyNames);
            }

            return _typeProperties[baseType.AssemblyQualifiedName];
        }
    }

}
