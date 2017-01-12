using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Utils;
using Native;

namespace QEWL
{
    public partial class MainWindow : Window
    {
        public const float HEIGHT_PERCENTAGE = 0.15f;
        public const float WIDTH_PERCENTAGE = 0.5f;
        public const int ACTIVATION_KEY_TIMER_INTERVAL_MS = 100;
        public const int MIN_TIME_BETWEEN_QUERIES_MS = 500;

        public const string WEB_PREFIX = "!";
        public const string CMD_PREFIX = "/";

        public readonly double HeightBarOnly;
        public readonly double HeightTotal;

        public bool SearchResultsPaneIsVisible { get; private set; }
        public Dictionary<Type, QueryHandler> QueryHandlers { get; private set; }

        private KeyboardHooks _hooks;
        private DispatcherTimer _activationKeyTimer;
        private bool _canTypeActivationKey;
        private bool _modkeyIsDown;
        private bool _textChangedRecently;
        private DispatcherTimer _queryTimer;
        private string _cachedQuery;
        private string _lastSuccessfulQuery;

        private QueryHandler _activeQueryHandler;
        private Previewer _previewer;

        public MainWindow()
        {
            InitializeComponent();

            QueryHandlers = new Dictionary<Type, QueryHandler>();

            ListBoxResults.SelectionMode = SelectionMode.Single;

            _hooks = new KeyboardHooks();
            _hooks.OnKeyPressed += OnHookedKeyPressed;
            _hooks.OnKeyReleased += OnHookedKeyReleased;
            _hooks.HookKeyboard();

            HeightBarOnly = 87;
            HeightTotal = Height;

            HideResultsPane();
            ResetLocation();

            QueryHandlers.Add(typeof(CommandQueryHandler), new CommandQueryHandler(this));
            QueryHandlers.Add(typeof(SystemQueryHandler), new SystemQueryHandler(this));
            QueryHandlers.Add(typeof(WebQueryHandler), new WebQueryHandler(this));

            Log.ReportMemoryUsage();
            foreach (KeyValuePair<Type, QueryHandler> qHandlers in QueryHandlers)
            {
                qHandlers.Value.OnQueryBegin    += () => { FrameLoadResults.Visibility = Visibility.Visible; };
                qHandlers.Value.OnQueryEnd      += (QueryResults results) => { FrameLoadResults.Visibility = Visibility.Hidden; };
                qHandlers.Value.OnScanComplete  += () => { Query(_cachedQuery); };
                qHandlers.Value.Scan();
            }

            _queryTimer = new DispatcherTimer();
            _queryTimer.Interval = new TimeSpan(0, 0, 0, 0, MIN_TIME_BETWEEN_QUERIES_MS);
            _queryTimer.Tick += (sender, args) =>
            {
                if (_textChangedRecently)
                {
                    _textChangedRecently = false;
                    if (_lastSuccessfulQuery != _cachedQuery)
                        Query(_cachedQuery);
                }
                _queryTimer.Stop();
            };

            _previewer = new Previewer(PreviewGrid, PreviewImage, PreviewName, PreviewDesc);
            ListBoxResults.SelectionChanged += (sender, args) =>
            {
                if (ListBoxResults.SelectedIndex >= 0)
                {
                    QueryResultItem item = (QueryResultItem)ListBoxResults.SelectedItem;
                    if (item != null)
                    {
                        _previewer.PreviewFromQueryResultItem(item);
                    }
                    else
                    {
                        Log.Error("Selected item is null");
                    }
                }
            };
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
#if DEBUG
            Topmost = false;
#else
            Topmost = true;
            //Hide();
#endif
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Log.Message(string.Format("{0} activated", GetType().Name));
            ResetLocation();
            TextBoxSearchBar.Clear();
            TextBoxSearchBar.Focus();
            _canTypeActivationKey = false;
            _activationKeyTimer = new DispatcherTimer();
            _activationKeyTimer.Interval = new TimeSpan(0, 0, 0, 0, ACTIVATION_KEY_TIMER_INTERVAL_MS);
            _activationKeyTimer.Tick += (sender, ev) =>
            {
                _activationKeyTimer.Stop();
                _canTypeActivationKey = true;
            };
            _activationKeyTimer.Start();
            TextBoxSearchBar.TextChanged += OnTextChanged;
            Win32.SetForegroundWindow(new WindowInteropHelper(this).Handle);
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            PreviewName.Text = string.Empty;
            PreviewDesc.Text = string.Empty;
            PreviewImage.Source = null;

            string text = TextBoxSearchBar.Text;
            _lastSuccessfulQuery = string.Empty;
            _cachedQuery = text;

            // Capatalise first letter
            if (text.Length == 1)
            {
                TextBoxSearchBar.Text = text.ToUpper();
                text = TextBoxSearchBar.Text;
                TextBoxSearchBar.CaretIndex = 1;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {

                if (!SearchResultsPaneIsVisible)
                {
                    ShowResultsPane();
                }
            }
            else
            {
                if (SearchResultsPaneIsVisible)
                {
                    HideResultsPane();
                }
            }

            // Don't query if we queried within MIN_TIME_BETWEEN_QUERIES_MS
            if (_textChangedRecently)
                return;

            _textChangedRecently = true;
            Query(text);
        }

        public void Query(string query)
        {
            if (_queryTimer.IsEnabled)
            {
                _queryTimer.Stop();
            }
            _queryTimer.Start();
            
            // Scroll to top
            if (ListBoxResults.HasItems)
            {
                DependencyObject dpo = VisualTreeHelper.GetChild(ListBoxResults, 0);
                ScrollViewer viewer = (ScrollViewer)((Decorator)dpo).Child;
                viewer.ScrollToTop();
            }

            if (!string.IsNullOrEmpty(query))
            {
                if (query.StartsWith(CMD_PREFIX))
                {
                    _activeQueryHandler = QueryHandlers[typeof(CommandQueryHandler)];
                    _activeQueryHandler.Query(query);
                }
                else if (query.StartsWith(WEB_PREFIX))
                {
                    _activeQueryHandler = QueryHandlers[typeof(WebQueryHandler)];
                    _activeQueryHandler.Query(query);
                }
                else // System search
                {
                    _activeQueryHandler = QueryHandlers[typeof(SystemQueryHandler)];
                    _activeQueryHandler.Query(query);
                }
                _lastSuccessfulQuery = query;
            }
        }

        public void ShowResultsPane()
        {
            Height = HeightTotal;
            SearchResultsPaneIsVisible = true;
        }

        public void HideResultsPane()
        {
            Height = HeightBarOnly;
            SearchResultsPaneIsVisible = false;
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Log.Message("BarWindow deactivated");
            TextBoxSearchBar.Clear();
        }

        private void OnHookedKeyReleased(object sender, KeyboardHooksEventArgs e)
        {
            if (e.Key == App.Prefs.ActivationKeyModifier.ToKey())
            {
                _modkeyIsDown = false;
            }
        }

        private void OnHookedKeyPressed(object sender, KeyboardHooksEventArgs e)
        {
            if (e.Key == App.Prefs.ActivationKeyModifier.ToKey())
            {
                _modkeyIsDown = true;
            }

            if (e.Key == App.Prefs.ActivationKey && (_modkeyIsDown || App.Prefs.ActivationKeyModifier == KeyModifier.None))
            {
                if (IsVisible)
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == App.Prefs.ActivationKey)
            {
                e.Handled = !_canTypeActivationKey;
            }
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Down)
            {
                if (TextBoxSearchBar.IsFocused)
                {
                    if (ListBoxResults.HasItems)
                    {
                        ListBoxResults.SelectedIndex = 0;

                        ListBoxResults.UpdateLayout();

                        var listBoxItem = (ListBoxItem)ListBoxResults
                            .ItemContainerGenerator
                            .ContainerFromItem(ListBoxResults.Items[0]);

                        listBoxItem.Focus();
                        ListBoxResults.Items.Refresh();
                        ListBoxResults.ScrollIntoView(listBoxItem);
                    }
                }

                if (ListBoxResults.SelectedIndex == ListBoxResults.Items.Count - 1)
                {
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (ListBoxResults.SelectedIndex == 0)
                {
                    e.Handled = true;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            TextBoxSearchBar.Focus();
            Keyboard.Focus(TextBoxSearchBar);

            if (e.Key == Key.Enter)
            {
                bool shouldHide = false;
                if (_activeQueryHandler != null)
                {
                    shouldHide = _activeQueryHandler.Confirm();
                }
                if (shouldHide)
                {
                    Hide();
                }
            }

#if DEBUG
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
#else
            if (e.Key == Key.Escape)
            {
                Hide();
            }
#endif
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            if (_activeQueryHandler != null)
            {
                _activeQueryHandler.Confirm();
            }
        }

        private void ResetLocation()
        {
            Top = (SystemParameters.PrimaryScreenHeight * HEIGHT_PERCENTAGE) - (Height / 2);
            Left = (SystemParameters.PrimaryScreenWidth * WIDTH_PERCENTAGE) - (Width / 2);
        }
    }
}
