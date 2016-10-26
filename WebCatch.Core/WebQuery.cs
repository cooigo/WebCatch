using AngleSharp;
using AngleSharp.Extensions;
using AngleSharp.Dom.Html;
using AngleSharp.Io.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Newtonsoft.Json.Linq;
using AngleSharp.Network;
using HttpMethod = AngleSharp.Network.HttpMethod;
using AngleSharp.Network.Default;
using System.IO;

namespace WebCatch.Core
{
    public partial class WebQuery : IChainable
    {
        protected WebQuery subQuery;
        private Dictionary<string, IEnumerable<ParseObject>> querySelectors;
        private Request request;
        public WebQuery()
        {
            this.request = new Request
            {
                Method = HttpMethod.Get,
                Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36"},
                },
                Content = MemoryStream.Null,
            };
        }
        private IDictionary<string, IEnumerable<ParseObject>> QuerySelectors
        {
            get
            {
                if (querySelectors == null)
                {
                    querySelectors = new Dictionary<string, IEnumerable<ParseObject>>(StringComparer.OrdinalIgnoreCase);
                }
                return querySelectors;
            }
        }
        public WebQuery From(string url, string body = null, Dictionary<string, string> headers = null)
        {
            //this.method = string.IsNullOrEmpty(body) ? HttpMethod.Get : HttpMethod.Post;
            //this.url = url;
            //this.body = body;

            this.request.Address = Url.Create(url);
            if (!string.IsNullOrEmpty(body))
            {
                this.request.Method = HttpMethod.Post;
                this.request.Content = new MemoryStream(Encoding.UTF8.GetBytes(body));
            }
            if (headers != null)
            {
                foreach (var head in headers)
                {
                    if (this.request.Headers.ContainsKey(head.Key))
                    {
                        this.request.Headers[head.Key] = head.Value;
                    }
                    else
                    {
                        this.request.Headers.Add(head.Key, head.Value);
                    }
                }
            }

            return this;
        }
        public WebQuery From(WebQuery subQuery)
        {
            this.subQuery = subQuery;
            return this;
        }
        public WebQuery Select(IEnumerable<ParseObject> selectors, string alias)
        {
            if (querySelectors != null && querySelectors.ContainsKey(alias))
            {
                throw new ArgumentOutOfRangeException("{0} alias is used");
            }

            QuerySelectors.Add(alias, selectors);

            return this;
        }
        public WebQuery SubQuery()
        {
            return new WebQuery();
        }
        public async Task Exceute<TResult>(Func<Dictionary<string, ParseObject>, TResult> action)
        {
            var httpClient = new HttpClient();
            var requester = new HttpClientRequester(httpClient);
            var config = Configuration.Default.WithDefaultLoader(requesters: new[] { requester });
            var context = BrowsingContext.New(config);

            Dictionary<string, ParseObject> dicResult;


            if (subQuery != null)
            {
                dicResult = await Exceute(context, subQuery);
                if (dicResult != null)
                {
                    if (dicResult.First().Value.TargetType == Enums.CatchParseType.anchor)
                    {
                        var document = await context.OpenAsync(mx => mx.Address(subQuery.request.Address).Content(string.Join("", dicResult.First().Value.ParseResult.Select(m => string.Format("<a href='{0}'></a>", m.ToString())))));

                        var anchors = document.QuerySelectorAll<IHtmlAnchorElement>("a");
                        var i = anchors.Count();
                        foreach (var anchor in anchors)
                        {
                            this.request.Address = Url.Create(anchor.Href);
                            Console.WriteLine(i--);
                            dicResult = await Exceute(context, this);
                            action(dicResult);
                        }
                    }
                }
            }
            else
            {
                dicResult = await Exceute(context, this);
                action(dicResult);
            }
        }
        private async Task<Dictionary<string, ParseObject>> Exceute(IBrowsingContext context, WebQuery webQuery)
        {

            var document = webQuery.request.Method == HttpMethod.Post ? await context.PostAsync(webQuery.request, MimeTypeNames.UrlencodedForm)
                   : await context.OpenAsync(webQuery.request);

            var elements = new Dictionary<string, ParseObject>();

            var sourceType = webQuery.QuerySelectors.First().Value.First().SourceType;

            JObject job = null;

            if (sourceType == Enums.CatchParseType.json)
            {
                job = JObject.Parse(document.Source.Text);
            }

            foreach (var item in webQuery.QuerySelectors)
            {
                var parseTemp = new ParseObject();

                foreach (var parseItem in item.Value)
                {
                    if (parseTemp.ParseResult != null && parseTemp.ParseResult.Count() > 0)
                    {
                        var rList = new List<string>();
                        foreach (var subResult in parseTemp.ParseResult)
                        {
                            if (parseItem.SourceType == Enums.CatchParseType.html)
                            {
                                var doc = await context.OpenAsync(hh => hh.Address(webQuery.request.Address).Content(subResult));
                                if (parseItem.TargetType == Enums.CatchParseType.anchor)
                                {
                                    rList.AddRange(doc.QuerySelectorAll<IHtmlAnchorElement>(parseItem.ParseString).Select(m => m.Href));
                                }
                                else
                                {
                                    rList.AddRange(doc.QuerySelectorAll(parseItem.ParseString).Select(m => parseItem.TargetType == Enums.CatchParseType.text ? m.TextContent : m.OuterHtml));
                                }
                            }
                        }
                        parseTemp = parseItem;
                        parseTemp.ParseResult = rList;
                    }
                    else
                    {
                        if (job != null)
                        {
                            parseTemp = parseItem;
                            var jts = job.SelectTokens(parseItem.ParseString);
                            parseTemp.ParseResult = jts.Select(m => m.ToString());
                        }
                        else
                        {
                            parseTemp = parseItem;
                            parseTemp.ParseResult = document.QuerySelectorAll(parseItem.ParseString).Select(m => parseItem.TargetType == Enums.CatchParseType.text ? m.TextContent : m.OuterHtml);
                        }

                    }
                }
                elements.Add(item.Key, parseTemp);
            }
            return elements;
        }
    }
}
