using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignaloBot.NDR.Host
{
    public class UnsupportedMediaTypeConnegHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;
            MediaTypeFormatterCollection formatters = request.GetConfiguration().Formatters;
            bool hasFormetterForContentType = formatters //
                .Any(formatter => formatter.SupportedMediaTypes.Contains(contentType));

            if (!hasFormetterForContentType)
            {
                return Task<HttpResponseMessage>.Factory //
                    .StartNew(() => new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
