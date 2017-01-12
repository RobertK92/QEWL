
using System;
using System.IO;
using System.Runtime.InteropServices;
using Utils;

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

        public const int ILD_TRANSPARENT = 0x00000001;
        public const int ILD_IMAGE = 0x00000020;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(IntPtr pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("Shell32.dll", SetLastError = false)]
        public static extern Int32 SHGetStockIconInfo(SHSTOCKICONID siid, SHGSI uFlags, ref SHSTOCKICONINFO psii);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);
        
        public static IntPtr GetIconIndex(string pszFile)
        {
            uint flags = SHGFI.SysIconIndex | SHGFI.UseFileAttributes;
            uint dwFileAtrtibutes = 0;
            if (!Path.HasExtension(pszFile))
            {
                flags |= SHGFI.OpenIcon;
                dwFileAtrtibutes |= 0x00000010;
            }
            else
            {
                flags |= SHGFI.LargeIcon;
            }

            SHFILEINFO sfi = new SHFILEINFO();
            SHGetFileInfo(pszFile
                , dwFileAtrtibutes
                , ref sfi
                , (uint)Marshal.SizeOf(sfi)
                , flags);

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

        [Flags]
        public enum SHGSI : UInt32
        {

            /// <summary>
            /// The szPath and iIcon members of the SHSTOCKICONINFO structure receive the path and icon index of the requested icon, in a format suitable for passing to the ExtractIcon function. The numerical value of this flag is zero, so you always get the icon location regardless of other flags.
            /// </summary>
            SHGSI_ICONLOCATION = 0,

            /// <summary>
            /// The hIcon member of the SHSTOCKICONINFO structure receives a handle to the specified icon.
            /// </summary>
            SHGSI_ICON = 0x100,

            /// <summary>
            /// The iSysImageImage member of the SHSTOCKICONINFO structure receives the index of the specified icon in the system imagelist.
            /// </summary>
            SHGSI_SYSICONINDEX = 0x4000,

            /// <summary>
            /// Modifies the SHGSI_ICON value by causing the function to add the link overlay to the file's icon.
            /// </summary>
            SHGSI_LINKOVERLAY = 0x8000,

            /// <summary>
            /// Modifies the SHGSI_ICON value by causing the function to blend the icon with the system highlight color.
            /// </summary>
            SHGSI_SELECTED = 0x10000,

            /// <summary>
            /// Modifies the SHGSI_ICON value by causing the function to retrieve the large version of the icon, as specified by the SM_CXICON and SM_CYICON system metrics.
            /// </summary>
            SHGSI_LARGEICON = 0x0,

            /// <summary>
            /// Modifies the SHGSI_ICON value by causing the function to retrieve the small version of the icon, as specified by the SM_CXSMICON and SM_CYSMICON system metrics.
            /// </summary>
            SHGSI_SMALLICON = 0x1,

            /// <summary>
            /// Modifies the SHGSI_LARGEICON or SHGSI_SMALLICON values by causing the function to retrieve the Shell-sized icons rather than the sizes specified by the system metrics.
            /// </summary>
            SHGSI_SHELLICONSIZE = 0x4

        }

        public enum SHSTOCKICONID : UInt32
        {
            /// <summary>
            /// Document of a type with no associated application.
            /// </summary>
            SIID_DOCNOASSOC = 0,
            /// <summary>
            /// Document of a type with an associated application.
            /// </summary>
            SIID_DOCASSOC = 1,
            /// <summary>
            /// Generic application with no custom icon.
            /// </summary>
            SIID_APPLICATION = 2,
            /// <summary>
            /// Folder (generic, unspecified state).
            /// </summary>
            SIID_FOLDER = 3,
            /// <summary>
            /// Folder (open).
            /// </summary>
            SIID_FOLDEROPEN = 4,
            /// <summary>
            /// 5.25-inch disk drive.
            /// </summary>
            SIID_DRIVE525 = 5,
            /// <summary>
            /// 3.5-inch disk drive.
            /// </summary>
            SIID_DRIVE35 = 6,
            /// <summary>
            /// Removable drive.
            /// </summary>
            SIID_DRIVEREMOVE = 7,
            /// <summary>
            /// Fixed drive (hard disk).
            /// </summary>
            SIID_DRIVEFIXED = 8,
            /// <summary>
            /// Network drive (connected).
            /// </summary>
            SIID_DRIVENET = 9,
            /// <summary>
            /// Network drive (disconnected).
            /// </summary>
            SIID_DRIVENETDISABLED = 10,
            /// <summary>
            /// CD drive.
            /// </summary>
            SIID_DRIVECD = 11,
            /// <summary>
            /// RAM disk drive.
            /// </summary>
            SIID_DRIVERAM = 12,
            /// <summary>
            /// The entire network.
            /// </summary>
            SIID_WORLD = 13,
            /// <summary>
            /// A computer on the network.
            /// </summary>
            SIID_SERVER = 15,
            /// <summary>
            /// A local printer or print destination.
            /// </summary>
            SIID_PRINTER = 16,
            /// <summary>
            /// The Network virtual folder (FOLDERID_NetworkFolder/CSIDL_NETWORK).
            /// </summary>
            SIID_MYNETWORK = 17,
            /// <summary>
            /// The Search feature.
            /// </summary>
            SIID_FIND = 22,
            /// <summary>
            /// The Help and Support feature.
            /// </summary>
            SIID_HELP = 23,

            // OVERLAYS...

            /// <summary>
            /// Overlay for a shared item.
            /// </summary>
            SIID_SHARE = 28,
            /// <summary>
            /// Overlay for a shortcut.
            /// </summary>
            SIID_LINK = 29,
            /// <summary>
            /// Overlay for items that are expected to be slow to access.
            /// </summary>
            SIID_SLOWFILE = 30,

            // MORE ICONS...

            /// <summary>
            /// The Recycle Bin (empty).
            /// </summary>
            SIID_RECYCLER = 31,
            /// <summary>
            /// The Recycle Bin (not empty).
            /// </summary>
            SIID_RECYCLERFULL = 32,
            /// <summary>
            /// Audio CD media.
            /// </summary>
            SIID_MEDIACDAUDIO = 40,
            /// <summary>
            /// Security lock.
            /// </summary>
            SIID_LOCK = 47,
            /// <summary>
            /// A virtual folder that contains the results of a search.
            /// </summary>
            SIID_AUTOLIST = 49,
            /// <summary>
            /// A network printer.
            /// </summary>
            SIID_PRINTERNET = 50,
            /// <summary>
            /// A server shared on a network.
            /// </summary>
            SIID_SERVERSHARE = 51,
            /// <summary>
            /// A local fax printer.
            /// </summary>
            SIID_PRINTERFAX = 52,
            /// <summary>
            /// A network fax printer.
            /// </summary>
            SIID_PRINTERFAXNET = 53,
            /// <summary>
            /// A file that receives the output of a Print to file operation.
            /// </summary>
            SIID_PRINTERFILE = 54,
            /// <summary>
            /// A category that results from a Stack by command to organize the contents of a folder.
            /// </summary>
            SIID_STACK = 55,
            /// <summary>
            /// Super Video CD (SVCD) media.
            /// </summary>
            SIID_MEDIASVCD = 56,
            /// <summary>
            /// A folder that contains only subfolders as child items.
            /// </summary>
            SIID_STUFFEDFOLDER = 57,
            /// <summary>
            /// Unknown drive type.
            /// </summary>
            SIID_DRIVEUNKNOWN = 58,
            /// <summary>
            /// DVD drive.
            /// </summary>
            SIID_DRIVEDVD = 59,
            /// <summary>
            /// DVD media.
            /// </summary>
            SIID_MEDIADVD = 60,
            /// <summary>
            /// DVD-RAM media.
            /// </summary>
            SIID_MEDIADVDRAM = 61,
            /// <summary>
            /// DVD-RW media.
            /// </summary>
            SIID_MEDIADVDRW = 62,
            /// <summary>
            /// DVD-R media.
            /// </summary>
            SIID_MEDIADVDR = 63,
            /// <summary>
            /// DVD-ROM media.
            /// </summary>
            SIID_MEDIADVDROM = 64,
            /// <summary>
            /// CD+ (enhanced audio CD) media.
            /// </summary>
            SIID_MEDIACDAUDIOPLUS = 65,
            /// <summary>
            /// CD-RW media.
            /// </summary>
            SIID_MEDIACDRW = 66,
            /// <summary>
            /// CD-R media.
            /// </summary>
            SIID_MEDIACDR = 67,
            /// <summary>
            /// A writeable CD in the process of being burned.
            /// </summary>
            SIID_MEDIACDBURN = 68,
            /// <summary>
            /// Blank writable CD media.
            /// </summary>
            SIID_MEDIABLANKCD = 69,
            /// <summary>
            /// CD-ROM media.
            /// </summary>
            SIID_MEDIACDROM = 70,
            /// <summary>
            /// An audio file.
            /// </summary>
            SIID_AUDIOFILES = 71,
            /// <summary>
            /// An image file.
            /// </summary>
            SIID_IMAGEFILES = 72,
            /// <summary>
            /// A video file.
            /// </summary>
            SIID_VIDEOFILES = 73,
            /// <summary>
            /// A mixed (media) file.
            /// </summary>
            SIID_MIXEDFILES = 74,


            /// <summary>
            /// Folder back. Represents the background Fold of a Folder.
            /// </summary>
            SIID_FOLDERBACK = 75,
            /// <summary>
            /// Folder front. Represents the foreground Fold of a Folder.
            /// </summary>
            SIID_FOLDERFRONT = 76,
            /// <summary>
            /// Security shield.
            /// </summary>
            /// <remarks>Use for UAC prompts only. This Icon doesn't work on all purposes.</remarks>
            SIID_SHIELD = 77,
            /// <summary>
            /// Warning (Exclamation mark).
            /// </summary>
            SIID_WARNING = 78,
            /// <summary>
            /// Informational (Info).
            /// </summary>
            SIID_INFO = 79,
            /// <summary>
            /// Error (X).
            /// </summary>
            SIID_ERROR = 80,
            /// <summary>
            /// Key.
            /// </summary>
            SIID_KEY = 81,
            /// <summary>
            /// Software.
            /// </summary>
            SIID_SOFTWARE = 82,
            /// <summary>
            /// A UI item, such as a button, that issues a rename command.
            /// </summary>
            SIID_RENAME = 83,
            /// <summary>
            /// A UI item, such as a button, that issues a delete command.
            /// </summary>
            SIID_DELETE = 84,
            /// <summary>
            /// Audio DVD media.
            /// </summary>
            SIID_MEDIAAUDIODVD = 85,
            /// <summary>
            /// Movie DVD media.
            /// </summary>
            SIID_MEDIAMOVIEDVD = 86,
            /// <summary>
            /// Enhanced CD media.
            /// </summary>
            SIID_MEDIAENHANCEDCD = 87,
            /// <summary>
            /// Enhanced DVD media.
            /// </summary>
            SIID_MEDIAENHANCEDDVD = 88,
            /// <summary>
            /// Enhanced DVD media.
            /// </summary>
            SIID_MEDIAHDDVD = 89,
            /// <summary>
            /// High definition DVD media in the Blu-ray Disc™ format.
            /// </summary>
            SIID_MEDIABLURAY = 90,
            /// <summary>
            /// Video CD (VCD) media.
            /// </summary>
            SIID_MEDIAVCD = 91,
            /// <summary>
            /// DVD+R media.
            /// </summary>
            SIID_MEDIADVDPLUSR = 92,
            /// <summary>
            /// DVD+RW media.
            /// </summary>
            SIID_MEDIADVDPLUSRW = 93,
            /// <summary>
            /// A desktop computer.
            /// </summary>
            SIID_DESKTOPPC = 94,
            /// <summary>
            /// A mobile computer (laptop).
            /// </summary>
            SIID_MOBILEPC = 95,
            /// <summary>
            /// The User Accounts Control Panel item.
            /// </summary>
            SIID_USERS = 96,
            /// <summary>
            /// Smart media.
            /// </summary>
            SIID_MEDIASMARTMEDIA = 97,
            /// <summary>
            /// CompactFlash media.
            /// </summary>
            SIID_MEDIACOMPACTFLASH = 98,
            /// <summary>
            /// A cell phone.
            /// </summary>
            SIID_DEVICECELLPHONE = 99,
            /// <summary>
            /// A digital camera.
            /// </summary>
            SIID_DEVICECAMERA = 100,
            /// <summary>
            /// A digital video camera.
            /// </summary>
            SIID_DEVICEVIDEOCAMERA = 101,
            /// <summary>
            /// An audio player.
            /// </summary>
            SIID_DEVICEAUDIOPLAYER = 102,
            /// <summary>
            /// Connect to network.
            /// </summary>
            SIID_NETWORKCONNECT = 103,
            /// <summary>
            /// The Network and Internet Control Panel item.
            /// </summary>
            SIID_INTERNET = 104,
            /// <summary>
            /// A compressed file with a .zip file name extension.
            /// </summary>
            SIID_ZIPFILE = 105,
            /// <summary>
            /// The Additional Options Control Panel item.
            /// </summary>
            SIID_SETTINGS = 106,
            /// <summary>
            /// Windows Vista with Service Pack 1 (SP1) and later. High definition DVD drive (any type - HD DVD-ROM, HD DVD-R, HD-DVD-RAM) that uses the HD DVD format.
            /// </summary>
            SIID_DRIVEHDDVD = 132,
            /// <summary>
            /// Windows Vista with SP1 and later. High definition DVD drive (any type - BD-ROM, BD-R, BD-RE) that uses the Blu-ray Disc format.
            /// </summary>
            SIID_DRIVEBD = 133,
            /// <summary>
            /// Windows Vista with SP1 and later. High definition DVD-ROM media in the HD DVD-ROM format.
            /// </summary>
            SIID_MEDIAHDDVDROM = 134,
            /// <summary>
            /// Windows Vista with SP1 and later. High definition DVD-R media in the HD DVD-R format.
            /// </summary>
            SIID_MEDIAHDDVDR = 135,
            /// <summary>
            /// Windows Vista with SP1 and later. High definition DVD-RAM media in the HD DVD-RAM format.
            /// </summary>
            SIID_MEDIAHDDVDRAM = 136,
            /// <summary>
            /// Windows Vista with SP1 and later. High definition DVD-ROM media in the Blu-ray Disc BD-ROM format.
            /// </summary>
            SIID_MEDIABDROM = 137,
            /// <summary>
            /// Windows Vista with SP1 and later. High definition write-once media in the Blu-ray Disc BD-R format.
            /// </summary>
            SIID_MEDIABDR = 138,
            /// <summary>
            /// Windows Vista with SP1 and later. High definition read/write media in the Blu-ray Disc BD-RE format.
            /// </summary>
            SIID_MEDIABDRE = 139,
            /// <summary>
            /// Windows Vista with SP1 and later. A cluster disk array.
            /// </summary>
            SIID_CLUSTEREDDRIVE = 140,

            /// <summary>
            /// The highest valid value in the enumeration. Values over 160 are Windows 7-only icons.
            /// </summary>
            SIID_MAX_ICONS = 175
        }

        const Int32 MAX_PATH = 260;
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHSTOCKICONINFO
        {
            /// <summary>
            /// The size of this structure, in bytes.
            /// </summary>
            public UInt32 cbSize;
            /// <summary>
            /// When SHGetStockIconInfo is called with the SHGSI_ICON flag, this member receives a handle to the icon.
            /// </summary>
            public IntPtr hIcon;
            /// <summary>
            /// When SHGetStockIconInfo is called with the SHGSI_SYSICONINDEX flag, this member receives the index of the image in the system icon cache.
            /// </summary>
            public Int32 iSysIconIndex;
            /// <summary>
            /// When SHGetStockIconInfo is called with the SHGSI_ICONLOCATION flag, this member receives the index of the icon in the resource whose path is received in szPath.
            /// </summary>
            public Int32 iIcon;
            /// <summary>
            /// When SHGetStockIconInfo is called with the SHGSI_ICONLOCATION flag, this member receives the path of the resource that contains the icon. The index of the icon within the resource is received in iIcon.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szPath;
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
