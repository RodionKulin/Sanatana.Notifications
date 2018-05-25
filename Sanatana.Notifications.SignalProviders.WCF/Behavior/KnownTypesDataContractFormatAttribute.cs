using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.Notifications.SignalProviders.WCF
{
    public class KnownTypesDataContractFormatAttribute : Attribute, IOperationBehavior
    {
        public Type BaseType { get; private set; }


        //init
        public KnownTypesDataContractFormatAttribute(Type baseType)
        {
            BaseType = baseType;
        }


        //methods
        public void AddBindingParameters(OperationDescription description, BindingParameterCollection parameters)
        { }

        public void ApplyClientBehavior(OperationDescription description, System.ServiceModel.Dispatcher.ClientOperation proxy)
        {
            IOperationBehavior innerBehavior = new KnownTypesDataContractSerializerOperationBehavior(description, BaseType);
            innerBehavior.ApplyClientBehavior(description, proxy);
        }


        public void ApplyDispatchBehavior(OperationDescription description, System.ServiceModel.Dispatcher.DispatchOperation dispatch)
        {
            IOperationBehavior innerBehavior = new KnownTypesDataContractSerializerOperationBehavior(description, BaseType);
            innerBehavior.ApplyDispatchBehavior(description, dispatch);
        }

        public void Validate(OperationDescription description)
        { }
    }
}
