using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebCatch.Core
{
    public static class MethodChainingExtensions
    {
        public static TChain With<TChain>(this TChain chain, Func<TChain, TChain> action)
            where TChain : IChainable
        {
            if (action == null)
                throw new ArgumentNullException("action");

            action(chain);

            return chain;
        }

        public static Task<IDocument> PostAsync(this IBrowsingContext context, Url url, string body, String type, String referer = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var ms = new MemoryStream();
            var tw = new StreamWriter(ms, TextEncoding.Utf8);
            tw.Write(body);
            tw.Flush();
            ms.Position = 0;

            var request = DocumentRequest.Post(url, ms, type, null, referer);

            if (context != null && context.Active != null)
            {
                request.Referer = context.Active.DocumentUri;
            }

            return context.OpenAsync(request, CancellationToken.None);
        }

        public static Task<IDocument> OpenAsync(this IBrowsingContext context, String address, String referer = null)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            var url = Url.Create(address);

            var request = DocumentRequest.Get(url, referer: referer);

            if (context != null && context.Active != null)
            {
                request.Referer = context.Active.DocumentUri;
            }

            return context.OpenAsync(request, CancellationToken.None);
        }
    }
}
