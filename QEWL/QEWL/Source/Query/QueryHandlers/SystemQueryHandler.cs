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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using Native;

namespace QEWL
{
    public class SystemQueryHandler : QueryHandler
    {
        //public const string FOLDER_ICON_IMAGE_PATH = @"Images\\FolderIcon.png";
        public const int ICON_FETCH_INTERVAL_MS = 100;

        public QueryNode RootQueryDictionary  { get; private set; }
        public List<string> RootIgnorePaths         { get; private set; }

        private Queue<Action> _iconResultInvokers = new Queue<Action>();
        private DispatcherTimer _iconTimer = new DispatcherTimer();
        
        public SystemQueryHandler(MainWindow mainWindow)
            : base(mainWindow)
        {
            RootQueryDictionary = new QueryNode(' ');
            RootIgnorePaths = new List<string>();
            //
            string winPath = Path.GetPathRoot(Environment.SystemDirectory);
            Log.Message(string.Format("Windows detected on drive '{0}'", winPath));

            RootIgnorePaths.Add(winPath + "Recovery");
            RootIgnorePaths.Add(winPath + "Windows");
            RootIgnorePaths.Add(winPath + "System Volume Information");
        }
        
        protected override void OnScan()
        {
            RootQueryDictionary.nodes.Clear();
            RootQueryDictionary.results.Clear();
            
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
                    QueryResultItem result = new QueryResultItem(true, drive.Name, drive.Name);
                    DistributeResultInDictionaryTree(result, RootQueryDictionary);
                    ScanSubDirsAndFiles(drive.RootDirectory, true);
                });   
            };

            worker.RunWorkerCompleted += (sender, args) => 
            {
                ScanComplete();
                Log.Success(string.Format("{0} scan completed in {1} seconds", GetType().Name, timer.Elapsed.TotalSeconds.ToString("F3")));
                Log.ReportMemoryUsage();
                Console.Beep();
            };

            worker.RunWorkerAsync();
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
                        QueryResultItem result = new QueryResultItem(true, dir.Name, dir.FullName);
                        DistributeResultInDictionaryTree(result, RootQueryDictionary);
                    });
                }
                else
                {
                    foreach (DirectoryInfo dir in dirs)
                    {
                        ScanSubDirsAndFiles(dir);
                        QueryResultItem result = new QueryResultItem(true, dir.Name, dir.FullName);
                        DistributeResultInDictionaryTree(result, RootQueryDictionary);
                    }
                }

                // Scan files.
                foreach (FileInfo file in files)
                {
                    QueryResultItem result = new QueryResultItem(true, file.Name, file.FullName);
                    DistributeResultInDictionaryTree(result, RootQueryDictionary);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                //Log.Warning(e.Message);
            }
        }

        protected override void OnQuery(string query)
        {
            if (IsScanning)
                return;
            
            string lowerCaseQuery = query.ToLower();
            QueryNode deepestDict = FindDeepestDictionaryForQuery(lowerCaseQuery, RootQueryDictionary);
            if (deepestDict != null)
            {
                ShowResults(deepestDict.results, lowerCaseQuery);
                QueryEnd(deepestDict.results);
            }
            else
            {
                QueryEnd(null);
            }
        }

        protected override IOrderedEnumerable<QueryResultItem> OrderResults(IOrderedEnumerable<QueryResultItem> currentOrder, string query)
        {
            IOrderedEnumerable<QueryResultItem> sortedByRelevanceAndLength = currentOrder.OrderBy(x => x.ResultDesc.Length);
            IOrderedEnumerable<QueryResultItem> sortedByExtensionPriority = sortedByRelevanceAndLength.OrderByDescending(
                x => Path.GetExtension(x.ResultDesc) == ".exe");
            return sortedByExtensionPriority;
        }
        
        protected override void OnResultItemsAddedForShow(IEnumerable<QueryResultItem> results, ListBox listBoxResults)
        {
            _iconResultInvokers.Clear();
            foreach(QueryResultItem result in results)
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

        private void AddIconToResult(QueryResultItem result)
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

        protected override bool OnConfirmed(QueryResultItem result)
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
