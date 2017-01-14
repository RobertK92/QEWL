using System.Text;

namespace QEWL
{
    public sealed class SystemQueryResultItem 
    {
        public readonly byte[] path;

        public SystemQueryResultItem(string path)
        {
            this.path = Encoding.UTF8.GetBytes(path);
        }
        
        public string GetPathString()
        {
            return Encoding.UTF8.GetString(path);
        }
    }
}
