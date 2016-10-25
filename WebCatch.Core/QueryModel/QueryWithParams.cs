using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCatch.Core
{
    public class QueryWithParams : IQueryWithParams
    {
        protected QueryWithParams parent;

        public TQuery CreateSubQuery<TQuery>()
           where TQuery : QueryWithParams, new()
        {
            var subQuery = new TQuery
            {
                parent = this
            };
            return subQuery;
        }
    }
}
