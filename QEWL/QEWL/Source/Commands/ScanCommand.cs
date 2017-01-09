using System;

namespace QEWL
{
    public class ScanCommand : Command
    {
        public ScanCommand()
        {
            Description = "Performs a complete system scan.";
        }

        protected override void OnExecute()
        {
            SystemQueryHandler sys = (SystemQueryHandler)((MainWindow)App.Current.MainWindow).QueryHandlers[typeof(SystemQueryHandler)];
            sys.Scan();
        }
    }
}
