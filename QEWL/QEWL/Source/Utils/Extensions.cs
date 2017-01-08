
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Utils
{
    public static class Extensions
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                new Int32Rect(0, 0, icon.Width, icon.Height),
                BitmapSizeOptions.FromEmptyOptions());
        }

        public static bool IsArrow(this Key key)
        {
            return (key == Key.Right || key == Key.Left || key == Key.Up || key == Key.Down);
        }

        public static Key ToKey(this KeyModifier modKey)
        {
            switch (modKey)
            {
                case KeyModifier.None:
                    return Key.None;
                case KeyModifier.LeftCtrl:
                    return Key.LeftCtrl;
                case KeyModifier.RightCtrl:
                    return Key.RightCtrl;
                case KeyModifier.LeftShift:
                    return Key.LeftShift;
                case KeyModifier.RightShift:
                    return Key.RightShift;
                case KeyModifier.LeftWin:
                    return Key.LWin;
                case KeyModifier.RightWin:
                    return Key.RWin;
                default:
                    return Key.None;
            }
        }
    }
}
