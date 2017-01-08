
using System.Windows;

namespace QEWL
{
    public class ShutdownCommand : Command
    {
        public ShutdownCommand()
        {
            Description = string.Format("Shutdown all {0} processes", App.APP_NAME);
        }

        protected override void OnExecute()
        {
            Application.Current.Shutdown();
        }
    }
}
