using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Network;
using AngleSharp.Network.Default;
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


            var ms = new MemoryStream(Encoding.UTF8.GetBytes(body));

            var request = DocumentRequest.Post(url, ms, type, null, referer);

            if (context != null && context.Active != null)
            {
                request.Referer = context.Active.DocumentUri;
            }

            return context.OpenAsync(request, CancellationToken.None);
        }

        public static Task<IDocument> PostAsync(this IBrowsingContext context, Request request ,String type)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var docRequest = DocumentRequest.Post(request.Address, request.Content, type, null, request.Address.Href);

            foreach (var head in request.Headers)
            {
                if (docRequest.Headers.ContainsKey(head.Key))
                {
                    docRequest.Headers[head.Key] = head.Value;
                }
                else
                {
                    docRequest.Headers.Add(head.Key, head.Value);
                }
            }

            if (context != null && context.Active != null)
            {
                docRequest.Referer = context.Active.DocumentUri;
            }

            return context.OpenAsync(docRequest, CancellationToken.None);
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

        public static Task<IDocument> OpenAsync(this IBrowsingContext context, Request request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));


            var docRequest = DocumentRequest.Get(request.Address, referer: request.Address.Href);

            foreach (var head in request.Headers)
            {
                if (docRequest.Headers.ContainsKey(head.Key))
                {
                    docRequest.Headers[head.Key] = head.Value;
                }
                else
                {
                    docRequest.Headers.Add(head.Key, head.Value);
                }
            }

            if (context != null && context.Active != null)
            {
                docRequest.Referer = context.Active.DocumentUri;
            }

            return context.OpenAsync(docRequest, CancellationToken.None);
        }
    }
}
