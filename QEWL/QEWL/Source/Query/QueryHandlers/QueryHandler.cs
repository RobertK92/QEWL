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

        protected virtual IOrderedEnumerable<QueryResultItem> OrderResults(IOrderedEnumerable<QueryResultItem> currentOrder, string query)
        {
            return currentOrder;
        }

        protected virtual void OnResultItemsAddedForShow(IEnumerable<QueryResultItem> results, ListBox listBoxResults) { }
        protected virtual void OnResultsShown(QueryResults shownResults) { }

        public void ShowResults(QueryResults results, string query)
        {
            IOrderedEnumerable<QueryResultItem> ordered = results.SortByNameRelevance(query);
            ordered = OrderResults(ordered, query);

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
            if (MainWindow.ListBoxResults.HasItems)
            {
                MainWindow.ListBoxResults.Visibility = Visibility.Visible;
            }

            OnResultsShown(results);
        }

        public void Scan()
        {
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

        protected void DistributeResultInDictionaryTree(QueryResultItem result, QueryDictionary dictionary)
        {
            #region Explaination
            // Distribute this result over it's alphabetical children.
            // Determine in what query dictionaries we put this result in.
            // e.g. Git will be added to g, g's-i, and g's-i's-t.
            // - g
            //    *git
            //    - a..
            //    - i
            //      *git
            //      - t
            //        *git
            #endregion

            string name = result.ResultName.ToLower();
            if (!string.IsNullOrWhiteSpace(name))
            {
                QueryDictionary parent = dictionary;
                foreach (char letter in name)
                {
                    lock (_queryDictLock)
                    {
                        if (!parent.ContainsKey(letter))
                        {
                            parent.Add(letter, new QueryDictionary());
                        }

                        QueryDictionary lettersDict = parent[letter];
                        lettersDict.results.Add(result);
                        parent = lettersDict;
                    }
                }
            }
        }

        protected QueryDictionary FindDeepestDictionaryForQuery(string query, QueryDictionary parent)
        {
            QueryDictionary result = null;
            foreach (char letter in query)
            {
                if (parent.ContainsKey(letter))
                {
                    result = parent[letter];
                }
                if (result != null)
                {
                    parent = result;
                }
                else
                {
                    break;
                }
            }
            return result;
        }
    }
}
