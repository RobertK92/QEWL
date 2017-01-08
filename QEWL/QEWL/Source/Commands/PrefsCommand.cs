
using Utils;
using System.Windows;

namespace QEWL
{
    public class PrefsCommand : Command
    {
        public PrefsCommand()
        {
            Description = "Opens the preferences window";
        }

        protected override void OnExecute()
        {
            bool prefsOpened = false;
            foreach(Window window in Application.Current.Windows)
            {
                if(window.GetType() == typeof(PrefsWindow))
                {
                    Log.Warning("Prefs window already open");
                    prefsOpened = true;
                    break;
                }
            }

            if (!prefsOpened)
            {
                new PrefsWindow().Show();
            }
        }
    }
}
