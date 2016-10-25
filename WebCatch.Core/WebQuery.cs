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

namespace WebCatch.Core
{
    public partial class WebQuery : QueryWithParams, IChainable
    {
        private string url;
        protected WebQuery subQuery;
        private Dictionary<string, string> querySelectors;
        private HttpMethod method = HttpMethod.Get;
        private string contextType;
        private string body;
        private IDictionary<string, string> QuerySelectors
        {
            get
            {
                if (querySelectors == null)
                {
                    querySelectors = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                return querySelectors;
            }
        }


        public WebQuery From(string url, string body = null, string type = null)
        {
            this.method = string.IsNullOrEmpty(body) ? HttpMethod.Get : HttpMethod.Post;
            this.url = url;
            this.body = body;
            this.contextType = type;
            return this;
        }


        public WebQuery From(WebQuery subQuery, string alias)
        {
            this.subQuery = subQuery;
            return this;
        }

        public WebQuery Select(string selectors, string alias)
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
            return this.CreateSubQuery<WebQuery>();
        }
       

        public async Task Exceute<TResult>(Func<Dictionary<string, IHtmlCollection<IElement>>, TResult> action)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.143 Safari/537.36");
            var requester = new HttpClientRequester(httpClient);
            var config = Configuration.Default.WithDefaultLoader(requesters: new[] { requester });
            var context = BrowsingContext.New(config);


            if (subQuery != null)
            {
                var rlist = new List<IHtmlAnchorElement>();

                var document = subQuery.method == HttpMethod.Post ? await context.PostAsync(Url.Create(subQuery.url), subQuery.body, MimeTypeNames.UrlencodedForm, subQuery.url)
                    : await context.OpenAsync(subQuery.url, subQuery.url);
                foreach (var item in subQuery.QuerySelectors)
                {
                    IEnumerable<IHtmlAnchorElement> anchors;

                    if (subQuery.contextType == "json")
                    {
                        var jts = JObject.Parse(document.Body.TextContent).SelectTokens(item.Value);
                        document = await context.OpenAsync(m => m.Address(url)
                                                                    .Content(string.Join("",
                                                                    jts.Select(j => string.Format("<a href='{0}'></a>", j.ToString())))));
                        anchors = document.QuerySelectorAll<IHtmlAnchorElement>("a");
                    }
                    else
                    {
                         anchors = document.QuerySelectorAll<IHtmlAnchorElement>(item.Value);
                    }

                    if (anchors != null && anchors.Count() > 0)
                    {
                        foreach (var anchor in anchors)
                        {
                            var elements = new Dictionary<string, IHtmlCollection<IElement>>();

                            var subDocument = await anchor.NavigateAsync();

                            foreach (var curSel in this.QuerySelectors)
                            {
                                elements.Add(curSel.Key, subDocument.QuerySelectorAll(curSel.Value));
                            }

                            action(elements);
                        }
                    }
                }
            }
        }

        private async Task Exceute<T1, TResult>(IBrowsingContext context, WebQuery webQuery, Func<T1, TResult> action)
        {

            var document = webQuery.method == HttpMethod.Post ? await context.PostAsync(Url.Create(webQuery.url), webQuery.body, MimeTypeNames.UrlencodedForm, webQuery.url)
                   : await context.OpenAsync(webQuery.url, webQuery.url);

            var elements = new Dictionary<string, IHtmlCollection<IElement>>();


            if (webQuery.contextType == "json")
            {
                var job = JObject.Parse(document.Source.Text);
                foreach (var item in webQuery.QuerySelectors)
                {
                   // elements.Add(item.Key, job.SelectTokens(item.Value));
                }
            }

            foreach (var item in webQuery.QuerySelectors)
            {
                if (webQuery.contextType == "json")
                {
                    var jts = JObject.Parse(document.Body.TextContent).SelectTokens(item.Value);

                }
                else
                {
                    elements.Add(item.Key, document.QuerySelectorAll(item.Value));
                }
            }

           // action((T1)elements);

        }
    }
}
