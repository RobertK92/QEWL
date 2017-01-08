using System;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Utils;

namespace QEWL
{
    public partial class App : Application
    {
        public const string APP_NAME = "QEWL";
        public const string PREFS_PATH = @"Prefs.xml";

        public static Prefs Prefs { get; private set; }

        public static void LoadPrefs()
        {
            if (!File.Exists(PREFS_PATH))
            {
                Log.Message("No prefs file found, creating one...");
                SavePrefs();
            }

            using (FileStream file = new FileStream(PREFS_PATH, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Prefs));
                    Prefs = (Prefs)serializer.Deserialize(reader);
                }
            }
        }

        public static void SavePrefs()
        {
            using (FileStream file = new FileStream(PREFS_PATH, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Prefs));
                    serializer.Serialize(writer, Prefs);
                }
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Message(string.Format("Initialising {0}...", APP_NAME));
            Prefs = Prefs.Default;
            LoadPrefs();
            Log.Success("Initialisation complete");
        }

        protected override void OnActivated(EventArgs e)
        {

        }
    }
}
