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
        protected abstract bool OnConfirmed(QueryResultItem result);
        protected abstract void OnScan();
        
        public int MaxResultsShown          { get; set; }
        public MainWindow MainWindow        { get; private set; }
        public bool IsScanning              { get; private set; }
        public QueryResults ShownResults    { get; private set; }

        protected Dictionary<IntPtr, ImageSource> IconCache { get; private set; }

        public event Action OnScanComplete              = delegate { };
        public event Action OnQueryBegin                = delegate { };
        public event Action<QueryResults> OnQueryEnd    = delegate { };
        
        private object _queryDictLock = new object();
        
        public QueryHandler(MainWindow mainWindow)
        {
            ShownResults = new QueryResults();
            IconCache = new Dictionary<IntPtr, ImageSource>();
            MainWindow = mainWindow;
            MaxResultsShown = 32;
        }

        protected virtual IOrderedEnumerable<QueryResultItem> OrderResults(QueryResults currentOrder, string query)
        {
            return null;
        }

        protected virtual void OnResultItemsAddedForShow(IEnumerable<QueryResultItem> results, ListBox listBoxResults) { }
        protected virtual void OnResultsShown(QueryResults shownResults) { }

        public void ShowResults(QueryResults results, string query)
        {
            IOrderedEnumerable<QueryResultItem> ordered = results.SortByNameRelevance(query);
            ordered = OrderResults(results, query);

            if (ordered != null)
            {
                QueryResults sortedResults = new QueryResults();
                int resultCount = 0;
                foreach (QueryResultItem result in ordered)
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

        protected void QueryEnd(QueryResults results)
        {
            OnQueryEnd(results);
        }

        public bool Confirm()
        {
            if (MainWindow.ListBoxResults.SelectedItem != null)
            {
                QueryResultItem item = (QueryResultItem)MainWindow.ListBoxResults.SelectedItem;
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
