
using System;
using System.Runtime.InteropServices;

namespace Native
{

    public static class Win32
    {
        const string IID_IImageList = "46EB5926-582E-4017-9FDF-E8998DAA0950";
        const string IID_IImageList2 = "192B9D83-50FC-457B-90A0-2B82A8B5DAE1";

        public const int SHIL_LARGE = 0x0;
        public const int SHIL_SMALL = 0x1;
        public const int SHIL_EXTRALARGE = 0x2;
        public const int SHIL_SYSSMALL = 0x3;
        public const int SHIL_JUMBO = 0x4;
        public const int SHIL_LAST = 0x4;

        public const int ILD_TRANSPARENT = 0x00000001;
        public const int ILD_IMAGE = 0x00000020;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(IntPtr pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        
        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);


        public static IntPtr GetIconIndex(string pszFile)
        {
            SHFILEINFO sfi = new SHFILEINFO();
            SHGetFileInfo(pszFile
                , 0
                , ref sfi
                , (uint)System.Runtime.InteropServices.Marshal.SizeOf(sfi)
                , (uint)(SHGFI.SysIconIndex | SHGFI.LargeIcon | SHGFI.UseFileAttributes));
            return sfi.iIcon;
        }

        /// <summary>
        /// Gets the 256 x 256 icon.
        /// </summary>
        /// <param name="iImage"></param>
        /// <returns></returns>
        public static IntPtr GetJumboIcon(IntPtr iImage)
        {
            return GetIcon(iImage, SHIL_JUMBO);
        }


        /// <summary>
        /// Gets the 48 x 48 icon.
        /// </summary>
        /// <param name="iImage"></param>
        /// <returns></returns>
        public static IntPtr GetExtraLargeIcon(IntPtr iImage)
        {
            return GetIcon(iImage, SHIL_EXTRALARGE);
        }

        /// <summary>
        /// Gets the 32 x 32 icon.
        /// </summary>
        /// <param name="iImage"></param>
        /// <returns></returns>
        public static IntPtr GetLargeIcon(IntPtr iImage)
        {
            return GetIcon(iImage, SHIL_LARGE);
        }

        /// <summary>
        /// Gets the 16 x 16 icon.
        /// </summary>
        /// <param name="iImage"></param>
        /// <returns></returns>
        public static IntPtr GetSmallIcon(IntPtr iImage)
        {
            return GetIcon(iImage, SHIL_SMALL);
        }

        public static IntPtr GetIcon(IntPtr iImage, int shil)
        {
            IImageList spiml = null;
            Guid guil = new Guid(IID_IImageList);
            SHGetImageList(shil, ref guil, ref spiml);
            IntPtr hIcon = IntPtr.Zero;
            spiml.GetIcon((int)iImage, ILD_TRANSPARENT | ILD_IMAGE, ref hIcon);
            return hIcon;
        }

        public static class SHGFI
        {
            /// <summary>get icon</summary>
            public const uint Icon = 0x000000100;
            /// <summary>get display name</summary>
            public const uint DisplayName = 0x000000200;
            /// <summary>get type name</summary>
            public const uint TypeName = 0x000000400;
            /// <summary>get attributes</summary>
            public const uint Attributes = 0x000000800;
            /// <summary>get icon location</summary>
            public const uint IconLocation = 0x000001000;
            /// <summary>return exe type</summary>
            public const uint ExeType = 0x000002000;
            /// <summary>get system icon index</summary>
            public const uint SysIconIndex = 0x000004000;
            /// <summary>put a link overlay on icon</summary>
            public const uint LinkOverlay = 0x000008000;
            /// <summary>show icon in selected state</summary>
            public const uint Selected = 0x000010000;
            /// <summary>get only specified attributes</summary>
            public const uint Attr_Specified = 0x000020000;
            /// <summary>get large icon</summary>
            public const uint LargeIcon = 0x000000000;
            /// <summary>get small icon</summary>
            public const uint SmallIcon = 0x000000001;
            /// <summary>get open icon</summary>
            public const uint OpenIcon = 0x000000002;
            /// <summary>get shell size icon</summary>
            public const uint ShellIconSize = 0x000000004;
            /// <summary>pszPath is a pidl</summary>
            public const uint PIDL = 0x000000008;
            /// <summary>use passed dwFileAttribute</summary>
            public const uint UseFileAttributes = 0x000000010;
            /// <summary>apply the appropriate overlays</summary>
            public const uint AddOverlays = 0x000000020;
            /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
            public const uint OverlayIndex = 0x000000040;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left, top, right, bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            int x;
            int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGELISTDRAWPARAMS
        {
            public int cbSize;
            public IntPtr himl;
            public int i;
            public IntPtr hdcDst;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int xBitmap;    // x offest from the upperleft of bitmap
            public int yBitmap;    // y offset from the upperleft of bitmap
            public int rgbBk;
            public int rgbFg;
            public int fStyle;
            public int dwRop;
            public int fState;
            public int Frame;
            public int crEffect;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGEINFO
        {
            public IntPtr hbmImage;
            public IntPtr hbmMask;
            public int Unused1;
            public int Unused2;
            public RECT rcImage;
        }

        [ComImportAttribute()]
        [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IImageList
        {
            [PreserveSig]
            int Add(
            IntPtr hbmImage,
            IntPtr hbmMask,
            ref int pi);

            [PreserveSig]
            int ReplaceIcon(
            int i,
            IntPtr hicon,
            ref int pi);

            [PreserveSig]
            int SetOverlayImage(
            int iImage,
            int iOverlay);

            [PreserveSig]
            int Replace(
            int i,
            IntPtr hbmImage,
            IntPtr hbmMask);

            [PreserveSig]
            int AddMasked(
            IntPtr hbmImage,
            int crMask,
            ref int pi);

            [PreserveSig]
            int Draw(
            ref IMAGELISTDRAWPARAMS pimldp);

            [PreserveSig]
            int Remove(
        int i);

            [PreserveSig]
            int GetIcon(
            int i,
            int flags,
            ref IntPtr picon);

            [PreserveSig]
            int GetImageInfo(
            int i,
            ref IMAGEINFO pImageInfo);

            [PreserveSig]
            int Copy(
            int iDst,
            IImageList punkSrc,
            int iSrc,
            int uFlags);

            [PreserveSig]
            int Merge(
            int i1,
            IImageList punk2,
            int i2,
            int dx,
            int dy,
            ref Guid riid,
            ref IntPtr ppv);

            [PreserveSig]
            int Clone(
            ref Guid riid,
            ref IntPtr ppv);

            [PreserveSig]
            int GetImageRect(
            int i,
            ref RECT prc);

            [PreserveSig]
            int GetIconSize(
            ref int cx,
            ref int cy);

            [PreserveSig]
            int SetIconSize(
            int cx,
            int cy);

            [PreserveSig]
            int GetImageCount(
        ref int pi);

            [PreserveSig]
            int SetImageCount(
            int uNewCount);

            [PreserveSig]
            int SetBkColor(
            int clrBk,
            ref int pclr);

            [PreserveSig]
            int GetBkColor(
            ref int pclr);

            [PreserveSig]
            int BeginDrag(
            int iTrack,
            int dxHotspot,
            int dyHotspot);

            [PreserveSig]
            int EndDrag();

            [PreserveSig]
            int DragEnter(
            IntPtr hwndLock,
            int x,
            int y);

            [PreserveSig]
            int DragLeave(
            IntPtr hwndLock);

            [PreserveSig]
            int DragMove(
            int x,
            int y);

            [PreserveSig]
            int SetDragCursorImage(
            ref IImageList punk,
            int iDrag,
            int dxHotspot,
            int dyHotspot);

            [PreserveSig]
            int DragShowNolock(
            int fShow);

            [PreserveSig]
            int GetDragImage(
            ref POINT ppt,
            ref POINT pptHotspot,
            ref Guid riid,
            ref IntPtr ppv);

            [PreserveSig]
            int GetItemFlags(
            int i,
            ref int dwFlags);

            [PreserveSig]
            int GetOverlayImage(
            int iOverlay,
            ref int piIndex);
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

    }
}
