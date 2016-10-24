using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCatch.Core
{
    public partial class WebQuery
    {
        private string url;
        private Dictionary<string, string> querySelectors;
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

        public WebQuery From(string url)
        {
            this.url = url;
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

        public void Exceute<TResult>(Func<Dictionary<string,object>, TResult> action)
        {

        }

        public void ExceuteJson<TResult>(Func<Dictionary<string, object>, TResult> action)
        {

        }


    }
}
