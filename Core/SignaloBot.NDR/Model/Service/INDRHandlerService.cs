using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.NDR.Model.Service
{
    [ServiceContract]
    public interface INDRHandlerService
    {
        [OperationContract]
        [WebInvoke(
            Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "")]
        void HandleNDR(Stream stream);
    }
}
