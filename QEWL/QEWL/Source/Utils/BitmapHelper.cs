using System;
using System.Collections.Generic;
using Utils;

namespace QEWL
{
    public static class BitmapHelper
    {
        /// <summary>
        /// Checks whether this extenion is supported.
        /// </summary>
        /// <param name="extension"> (e.g.) ".jpg" </param>
        /// <returns></returns>
        public static bool IsSupportedFormat(string extension)
        {
            if (!extension.StartsWith("."))
                return false;
            string lowerCased = extension.ToLower();
            return (lowerCased == ".jpg" || lowerCased == ".jpeg" ||
                    lowerCased == ".png" || lowerCased == ".bmp"  ||
                    lowerCased == ".tiff");
        }
    }
}
