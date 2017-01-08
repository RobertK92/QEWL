using Utils;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Native;

namespace QEWL
{    
    using Image = System.Windows.Controls.Image;

    public class Previewer
    {
        public readonly Grid PreviewGrid;
        public readonly Image PreviewImage;
        public readonly TextBlock PreviewName;
        public readonly TextBlock PreviewDesc;

        private Dictionary<string, BitmapImage> _resourceIconCache = new Dictionary<string, BitmapImage>();

        public Previewer(Grid previewGrid, Image previewImage, TextBlock previewName, TextBlock previewDesc)
        {
            PreviewGrid = previewGrid;
            PreviewImage = previewImage;
            PreviewName = previewName;
            PreviewDesc = previewDesc;
        }

        public virtual void PreviewFromQueryResultItem(QueryResultItem item)
        {
            PreviewDefault(item);
            /*string extension = Path.GetExtension(item.ResultDesc);
            if(extension.EndsWith(".jpg") || extension.EndsWith(".png"))
            {
                Uri uri = new Uri(item.ResultDesc, UriKind.Absolute);
                BitmapImage img = new BitmapImage(uri);
                
                PreviewImage.Width = img.Width;
                PreviewImage.Height = img.Height;
                PreviewImage.Source = img;

            }
            else
            {
                PreviewDefault(item);
            }*/
        }

        private void PreviewDefault(QueryResultItem item)
        {
            PreviewName.Text = item.ResultName;
            PreviewDesc.Text = item.ResultDesc;

            if (item.ResultIconBitmap != null)
            {
                IntPtr iIcon = Win32.GetIconIndex(item.ResultDesc);
                IntPtr hIcon = Win32.GetJumboIcon(iIcon);

                if (hIcon != IntPtr.Zero)
                {
                    Icon icon = Icon.FromHandle(hIcon);
                    if (icon != null)
                    {
                        ImageSource imgSource = icon.ToImageSource();
                        PreviewImage.Source = imgSource;
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(item.ResultIcon))
                {
                    if (!_resourceIconCache.ContainsKey(item.ResultIcon))
                    {
                        BitmapImage img = new BitmapImage(new Uri(item.ResultIcon, UriKind.Absolute));
                        _resourceIconCache.Add(item.ResultIcon, img);
                    }
                    PreviewImage.Source = _resourceIconCache[item.ResultIcon];
                }
            }
        }
    }
}
