using System.Collections.Generic;
using System.Linq;

namespace QEWL
{
    public class QueryNode 
    {
        public QueryResults results;
        public List<QueryNode> nodes;
        public char letter;

        public QueryNode(char letter)
        {
            this.letter = letter;
            results = new QueryResults();
            nodes = new List<QueryNode>();
        }

        public QueryNode FindInnerNode(char letter)
        {
            return nodes.FirstOrDefault(x => x.letter == letter);
        }

        public bool HasInnerNode(char letter)
        {
            return (FindInnerNode(letter) != null);
        }
    }
}
