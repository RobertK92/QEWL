using System.Collections.Generic;

namespace QEWL
{
    public class QueryDictionary : SortedDictionary<char, QueryDictionary>
    {
        public QueryResults results;
            
        public QueryDictionary()
        {
            results = new QueryResults();
        }
    }
}
