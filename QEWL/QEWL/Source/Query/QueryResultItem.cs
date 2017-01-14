
using System;
using System.IO;
using System.Windows.Media;

namespace QEWL
{
    public class QueryResultItem 
    {
        public string ResultIcon { get; set; }
        public string ResultName { get; set; }
        public string ResultDesc { get; set; }
        public ImageSource ResultIconBitmap { get; set; }
        
        public bool iconFromFile;
        public string resultNameLowerCase;
                
        public QueryResultItem(string resultIcon, string resultName, string resultDesc)
        {
            ResultIcon = string.Format("{0}/{1}", Environment.CurrentDirectory, resultIcon);
            ResultName = resultName;
            resultNameLowerCase = ResultName.ToLower();
            ResultDesc = resultDesc;
        }

        public QueryResultItem(bool iconFromFile, string resultName, string resultDesc)
            : this("Images/TransparantIcon.png", resultName, resultDesc)
        {
            this.iconFromFile = iconFromFile;
        }

        public override string ToString()
        {
            return ResultName;
        }

    }
}
