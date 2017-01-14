using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Utils;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;
using Native;
using System.Runtime.InteropServices;

namespace QEWL
{
    public class SystemQueryHandler : QueryHandler
    {
        //public const string FOLDER_ICON_IMAGE_PATH = @"Images\\FolderIcon.png";
        public const int ICON_FETCH_INTERVAL_MS = 100;

        //public QueryNode RootQueryDictionary        { get; private set; }
        public List<string> RootIgnorePaths         { get; private set; }
        public List<SystemQueryResultItem> BigResultList  { get; private set; }

        private Queue<Action> _iconResultInvokers = new Queue<Action>();
        private DispatcherTimer _iconTimer = new DispatcherTimer();
        
        public SystemQueryHandler(MainWindow mainWindow)
            : base(mainWindow)
        {
            BigResultList = new List<SystemQueryResultItem>();
            RootIgnorePaths = new List<string>();
            
            string winPath = Path.GetPathRoot(Environment.SystemDirectory);
            Log.Message(string.Format("Windows detected on drive '{0}'", winPath));

            RootIgnorePaths.Add(winPath + "Recovery");
            RootIgnorePaths.Add(winPath + "Windows");
            RootIgnorePaths.Add(winPath + "System Volume Information");
        }
        
        protected override void OnScan()
        {
            recDepth = 0;
            BackgroundWorker worker = new BackgroundWorker();
            Stopwatch timer = Stopwatch.StartNew();
            worker.DoWork += (sender, args) =>
            {
                DriveInfo[] drives = DriveInfo.GetDrives();
                IEnumerable<DriveInfo> readyDrives = drives.Where(x => x.IsReady);
                Log.Message(string.Format("{0} ready drives detected", readyDrives.Count()));

                Parallel.ForEach(readyDrives, (DriveInfo drive) =>
                {
                    Log.Message(string.Format("Scanning drive {0}: [{1}] ASync...", drive.VolumeLabel, drive.Name));
                    SystemQueryResultItem result = new SystemQueryResultItem(drive.Name);
                    AddItem(result);
                    ScanSubDirsAndFiles(drive.RootDirectory, true);
                });
            };

            worker.RunWorkerCompleted += (sender, args) => 
            {
                SortBigList();
                ScanComplete();
                Log.Success(string.Format("{0} scan completed in {1} seconds", GetType().Name, timer.Elapsed.TotalSeconds.ToString("F3")));
                GC.Collect();
                Log.ReportMemoryUsage();
                Console.Beep();
            };

            worker.RunWorkerAsync();
        }

        public void SortBigList()
        {
            BigResultList = BigResultList.Where(x => x != null).OrderBy(y => Path.GetFileName(y.GetPathString())).ToList();
        }

        private void AddItem(SystemQueryResultItem item)
        {
            if(item != null)
                BigResultList.Add(item);
        }

        int recDepth = 0;
        long prevMem = 0;
        private void ScanSubDirsAndFiles(DirectoryInfo parentDir, bool isRoot = false)
        {
            // temp
            if(recDepth == 0)
                Log.Warning(string.Format("Temporary using recursion depth limit ({0}) for debugging purposes", DebugOptions.SystemQueryDepthLimit));
            recDepth++;
            if (recDepth >= DebugOptions.SystemQueryDepthLimit)
            {
                return;
            }

            try
            {
                IEnumerable<DirectoryInfo> dirs = parentDir.EnumerateDirectories();
                IEnumerable<FileInfo> files = parentDir.EnumerateFiles();

                // Root folders are filtered and scanned in parallel.
                if (isRoot)
                {
                    dirs = dirs.Where(x => !RootIgnorePaths.Contains(x.FullName));
                    Parallel.ForEach(dirs, (DirectoryInfo dir) =>
                    {
                        Stopwatch timer = Stopwatch.StartNew();
                        ScanSubDirsAndFiles(dir);
                        SystemQueryResultItem result = new SystemQueryResultItem(dir.FullName);
                        AddItem(result);
                    });
                }
                else
                {
                    foreach (DirectoryInfo dir in dirs)
                    {
                        ScanSubDirsAndFiles(dir);
                        SystemQueryResultItem result = new SystemQueryResultItem(dir.FullName);
                        AddItem(result);
                    }
                }

                // Scan files.
                foreach (FileInfo file in files)
                {
                    SystemQueryResultItem result = new SystemQueryResultItem(file.FullName);
                    AddItem(result);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Warning(e.Message);
            }
        }

        private int FindClosestResultIndex(string query, int startIndex = 0, int substringLength = 1, int closest = -1)
        {
            if (substringLength > query.Length)
                return closest;

            string querySubString = query.Substring(0, substringLength);
            for (int i = startIndex; i < BigResultList.Count; i++)
            {
                string name = Path.GetFileName(BigResultList[i].GetPathString()).ToLower();
                if (substringLength < name.Length)
                {
                    if (name.Substring(0, substringLength) == querySubString)
                    {
                        closest = i;
                        startIndex = i;
                        substringLength++;
                        return FindClosestResultIndex(query, startIndex, substringLength, closest);
                    }
                }
            }
            substringLength++;
            return FindClosestResultIndex(query, startIndex, substringLength, closest);
        }

        protected override void OnQuery(string query)
        {
            if (IsScanning)
                return;
            
            string lowerCaseQuery = query.ToLower();
            int closestIndex = FindClosestResultIndex(lowerCaseQuery);
            if (closestIndex != -1)
            {
                Log.Message(BigResultList[closestIndex]);
                UIResults results = new UIResults();
                for (int i = 0; i < MaxResultsShown; i++)
                {
                    int index = (closestIndex + i);
                    if (index < BigResultList.Count && index > 0)
                    {
                        string path = BigResultList[index].GetPathString();
                        UIResultItem uiItem = new UIResultItem(true, Path.GetFileName(path), path);
                        results.Add(uiItem);
                    }
                }

                ShowResults(results, lowerCaseQuery);
                QueryEnd(results);
            }
            else
            {
                QueryEnd(null);
            }
        }

        protected override IOrderedEnumerable<UIResultItem> OrderResults(UIResults currentOrder, string query)
        {
            //IOrderedEnumerable<QueryResultItem> sortedByDiscLength = currentOrder.OrderBy(x => x.ResultDesc.Length);
            return null;
        }
        
        protected override void OnResultItemsAddedForShow(IEnumerable<UIResultItem> results, ListBox listBoxResults)
        {
            _iconResultInvokers.Clear();
            foreach(UIResultItem result in results)
            {
                _iconResultInvokers.Enqueue(() => 
                {
                    AddIconToResult(result);
                });
            }

            if (_iconTimer.IsEnabled)
                _iconTimer.Stop();

            _iconTimer.Interval = new TimeSpan(0, 0, 0, 0, ICON_FETCH_INTERVAL_MS);
            _iconTimer.Tick += (sender, args) =>
            {
                if(_iconResultInvokers.Count != 0)
                {
                    _iconResultInvokers.Dequeue().Invoke();
                    listBoxResults.ItemsSource = ShownResults;
                    listBoxResults.Items.Refresh();
                }
                else
                {
                    _iconTimer.Stop();
                }
            };
            
            _iconTimer.Start();
        }

        private void AddIconToResult(UIResultItem result)
        {
            if (result.iconFromFile)
            {
                IntPtr iIcon = Win32.GetIconIndex(result.ResultDesc);
                IntPtr hIcon = Win32.GetExtraLargeIcon(iIcon);
                
                if (!IconCache.ContainsKey(iIcon))
                {
                    if (hIcon != IntPtr.Zero)
                    {
                        Icon icon = Icon.FromHandle(hIcon);
                        if (icon != null)
                        {
                            ImageSource imgSource = icon.ToImageSource();
                            IconCache.Add(iIcon, imgSource);
                            result.ResultIconBitmap = imgSource;
                        }
                    }
                }
                else
                {
                    result.ResultIconBitmap = IconCache[iIcon];
                }
            }
        }

        protected override bool OnConfirmed(UIResultItem result)
        {
            string path = result.ResultDesc;
            if (!string.IsNullOrWhiteSpace(path))
            {
                if (Directory.Exists(path) || File.Exists(path))
                {
                    if (path.Contains("steamapps\\common") && Path.GetExtension(path) == ".exe")
                    {
                        Log.Warning("Steam game detected, might not launch properly if exe is run directly.");
                    }

                    string file = Path.GetFileName(path);
                    Log.Message(string.Format("Starting process for: '{0}'...", file));
                    try
                    {
                        Process.Start(path);
                        Log.Success(string.Format("Process for: '{0}' started succesfully", file));
                    }
                    catch(Exception e)
                    {
                        Log.Error(string.Format("Failed to start process for '{0}'", result.ResultName));
                        Log.Exception(e);
                    }
                }
                else
                {
                    Log.Error(string.Format("Failed to open file or directory: file or directory '{0}' does not exist", path));
                    return false;
                }
            }
            return true;
        }
    }
}
