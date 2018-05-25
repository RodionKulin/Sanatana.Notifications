using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    public class KnownTypesDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        public Type BaseType { get; private set; }
        public KnownTypesDataContractSerializerOperationBehavior(OperationDescription operationDescription, Type baseType) : base(operationDescription)
        {
            BaseType = baseType;
        }


        //methods
        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes);
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes);
        }

        private IEnumerable<Type> GetKnownTypes()
        {
            // Try to find all types that derive from BaseType in the 
            // executing assembly and add them to the knownTypes collection
            return
                from type in Assembly.GetExecutingAssembly().GetTypes()
                where type != BaseType && BaseType.IsAssignableFrom(type)
                select type;
        }
    }
}
