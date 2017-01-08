using System.Windows.Input;
using System.Xml.Serialization;
using System.Windows;
using System;
using Utils;

namespace QEWL
{
    [XmlRoot("Prefs")]
    public class Prefs : ICloneable
    {
        private static Prefs _default;
        public static Prefs Default
        {
            get
            {
                if(_default == null)
                {
                    _default = new Prefs();
                    _default.ActivationKeyModifier = KeyModifier.LeftCtrl;
                    _default.ActivationKey = Key.Space;
                    _default.Theme = Theme.Dark;
                }
                return _default;
            }
        }

        private Theme _theme;
        [XmlElement("Theme")]
        public Theme Theme
        {
            get { return _theme; }
            set
            {
                if (_theme != value)
                {
                    ResourceDictionary skin = new ResourceDictionary();
                    switch (value)
                    {
                        case Theme.Dark:
                            skin.Source = new Uri(@"Themes\DarkTheme.xaml", UriKind.Relative);
                            break;
                        case Theme.Light:
                            skin.Source = new Uri(@"Themes\LightTheme.xaml", UriKind.Relative);
                            break;
                        default:
                            break;
                    }
                    App.Current.Resources.MergedDictionaries.Clear();
                    App.Current.Resources.MergedDictionaries.Add(skin);
                }
                _theme = value;
            }
        }
        
        [XmlElement("ActivationKeyModifier")]
        public KeyModifier ActivationKeyModifier { get; set; }

        [XmlElement("ActivationKey")]
        public Key ActivationKey { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
