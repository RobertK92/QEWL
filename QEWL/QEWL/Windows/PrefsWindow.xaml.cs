using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utils;

namespace QEWL
{
    /// <summary>
    /// Interaction logic for PrefsWindow.xaml
    /// </summary>
    public partial class PrefsWindow : Window
    {
        private Theme _theme;
        private Key _activationKey;
        private KeyModifier _modKey;

        public PrefsWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // TODO: Fix light theme and remove this line.
            ComboBoxTheme.IsEnabled = false;

            ComboBoxTheme.ItemsSource = Enum.GetValues(typeof(Theme));
            ComboBoxTheme.SelectedItem = App.Prefs.Theme;

            ComboBoxKeyModifier.ItemsSource = Enum.GetValues(typeof(KeyModifier));
            ComboBoxKeyModifier.SelectedItem = App.Prefs.ActivationKeyModifier;

            ComboBoxActivationKey.ItemsSource = Enum.GetValues(typeof(Key));
            ComboBoxActivationKey.SelectedItem = App.Prefs.ActivationKey;

            ButtonCancel.Click += OnCancelButtonClicked;
            ButtonSave.Click += OnSaveButtonClicked;

            ComboBoxTheme.SelectionChanged += (object sender, SelectionChangedEventArgs args) =>
            {
                _theme = (Theme)ComboBoxTheme.SelectedItem;
            };

            ComboBoxActivationKey.SelectionChanged += (object sender, SelectionChangedEventArgs args) =>
            {
                _activationKey = (Key)ComboBoxActivationKey.SelectedItem;
            };

            ComboBoxKeyModifier.SelectionChanged += (object sender, SelectionChangedEventArgs args) =>
            {
                _modKey = (KeyModifier)ComboBoxKeyModifier.SelectedItem;
            };

            Log.Success(string.Format("{0} initialised", GetType().Name));
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Log.Message(string.Format("{0} activated", GetType().Name));
            LoadValues();
        }

        private void LoadValues()
        {
            App.LoadPrefs();

            ComboBoxTheme.SelectedItem = App.Prefs.Theme;
            ComboBoxKeyModifier.SelectedItem = App.Prefs.ActivationKeyModifier;
            ComboBoxActivationKey.SelectedItem = App.Prefs.ActivationKey;

            _theme = App.Prefs.Theme;
            _activationKey = App.Prefs.ActivationKey;
            _modKey = App.Prefs.ActivationKeyModifier;
        }

        private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
        {
            App.Prefs.Theme = _theme;
            App.Prefs.ActivationKey = _activationKey;
            App.Prefs.ActivationKeyModifier = _modKey;

            App.SavePrefs();
            Close();
        }

        private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
