using Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace QEWL
{
    public abstract class QueryHandler
    {
        protected abstract void OnQuery(string text);
        protected abstract bool OnConfirmed(UIResultItem result);
        protected abstract void OnScan();
        
        public int MaxResultsShown          { get; set; }
        public MainWindow MainWindow        { get; private set; }
        public bool IsScanning              { get; private set; }
        public UIResults ShownResults    { get; private set; }

        protected Dictionary<IntPtr, ImageSource> IconCache { get; private set; }

        public event Action OnScanComplete              = delegate { };
        public event Action OnQueryBegin                = delegate { };
        public event Action<UIResults> OnQueryEnd    = delegate { };
        
        private object _queryDictLock = new object();
        
        public QueryHandler(MainWindow mainWindow)
        {
            ShownResults = new UIResults();
            IconCache = new Dictionary<IntPtr, ImageSource>();
            MainWindow = mainWindow;
            MaxResultsShown = 32;
        }

        protected virtual IOrderedEnumerable<UIResultItem> OrderResults(UIResults currentOrder, string query)
        {
            return null;
        }

        protected virtual void OnResultItemsAddedForShow(IEnumerable<UIResultItem> results, ListBox listBoxResults) { }
        protected virtual void OnResultsShown(UIResults shownResults) { }

        public void ShowResults(UIResults results, string query)
        {
            IOrderedEnumerable<UIResultItem> ordered = results.SortByNameRelevance(query);
            ordered = OrderResults(results, query);

            if (ordered != null)
            {
                UIResults sortedResults = new UIResults();
                int resultCount = 0;
                foreach (UIResultItem result in ordered)
                {
                    if (resultCount >= MaxResultsShown)
                        break;
                    sortedResults.Add(result);
                    resultCount++;
                }
                
                ShownResults = sortedResults;
                OnResultItemsAddedForShow(sortedResults, MainWindow.ListBoxResults);

                MainWindow.ListBoxResults.ItemsSource = sortedResults;
            }
            else
            {
                ShownResults = results;
                OnResultItemsAddedForShow(results, MainWindow.ListBoxResults);
                MainWindow.ListBoxResults.ItemsSource = results;
            }

            if (MainWindow.ListBoxResults.HasItems)
            {
                MainWindow.ListBoxResults.Visibility = Visibility.Visible;
            }

            OnResultsShown(results);
        }

        public void Scan()
        {
            if(IsScanning)
            {
                Log.Warning("Attempting to begin new scan while already scanning");
                return;
            }

            Log.Message(string.Format("Scanning {0}...", GetType().Name));
            IsScanning = true;
            OnScan();
        }

        protected void ScanComplete()
        {
            IsScanning = false;
            OnScanComplete();
        }

        protected void QueryEnd(UIResults results)
        {
            OnQueryEnd(results);
        }

        public bool Confirm()
        {
            if (MainWindow.ListBoxResults.SelectedItem != null)
            {
                UIResultItem item = (UIResultItem)MainWindow.ListBoxResults.SelectedItem;
                Log.Message(string.Format("Confirmed item: {0}", item.ResultName));
                return OnConfirmed(item);
            }
            return false;
        }

        public void Query(string text)
        {
            OnQueryBegin();
            if (MainWindow.ListBoxResults.HasItems)
            {
                MainWindow.ListBoxResults.ItemsSource = null;
            }
            OnQuery(text);
        }
    }
}
