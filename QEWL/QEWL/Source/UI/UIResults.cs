using System.Collections.Generic;
using System.Linq;

namespace QEWL
{
    public class UIResults : List<UIResultItem>
    {
        public IOrderedEnumerable<UIResultItem> SortByNameRelevance(string name)
        {
            return this.OrderByDescending(x => QueryOrderNameRelevance(name.ToLower(), x.ResultName.ToLower()));
        }

        public IOrderedEnumerable<UIResultItem> SortByUriLength()
        {
            return this.OrderByDescending(x => x.ResultDesc.Length);
        }
        
        private int QueryOrderNameRelevance(string query, string name)
        {
            if (name == query)
                return 2;
            if (query.StartsWith(name))
                return 1;
            if (name.Contains(query))
                return 0;
            return -1;
        }
    }
}
