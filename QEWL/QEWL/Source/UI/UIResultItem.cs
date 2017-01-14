using System;
using System.Windows.Media;

namespace QEWL
{
    public class UIResultItem
    {
        public static string TransparantIconPath = @"Images\\TransparantIcon.png";
        public static string TransparantIconPathAbsolute = null;

        public string ResultIcon { get; set; }
        public string ResultName { get; set; }
        public string ResultDesc { get; set; }
        public ImageSource ResultIconBitmap { get; set; }

        public bool iconFromFile;
        public string resultNameLowerCase;

        public UIResultItem(string resultIcon, string resultName, string resultDesc)
        {
            ResultIcon = string.Format("{0}/{1}", Environment.CurrentDirectory, resultIcon);
            ResultName = resultName;
            resultNameLowerCase = ResultName.ToLower();
            ResultDesc = resultDesc;
        }

        public UIResultItem(bool iconFromFile, string resultName, string resultDesc)
        {
            if (string.IsNullOrEmpty(TransparantIconPathAbsolute))
            {
                TransparantIconPathAbsolute = string.Format("{0}/{1}", Environment.CurrentDirectory, TransparantIconPath);
            }

            ResultIcon = TransparantIconPathAbsolute;
            ResultName = resultName;
            resultNameLowerCase = ResultName.ToLower();
            ResultDesc = resultDesc;
            this.iconFromFile = iconFromFile;
        }

        public override string ToString()
        {
            return ResultName;
        }
    }
}
