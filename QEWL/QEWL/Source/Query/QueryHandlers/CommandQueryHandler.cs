using Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace QEWL
{
    public class CommandQueryHandler : QueryHandler
    {
        private Dictionary<string, Command> _commands = new Dictionary<string, Command>();

        public CommandQueryHandler(MainWindow mainWindow)
            : base(mainWindow)
        {
            /* Only use lowercases for command keys! */
            _commands.Add("/scan", new ScanCommand());
            _commands.Add("/prefs", new PrefsCommand());
            _commands.Add("/shutdown", new ShutdownCommand());
        }

        protected override void OnScan()
        {

        }
        
        protected override void OnQuery(string text)
        {
            UIResults results = new UIResults();
            foreach (KeyValuePair<string, Command> cmd in _commands)
            {
                results.Add(new UIResultItem("Images/CommandIcon.png", cmd.Key, cmd.Value.Description));
            }

            ShowResults(results, text);
            QueryEnd(results, text);
        }
        
        protected override bool OnConfirmed(UIResultItem result)
        {
            string text = result.ResultName.ToLower();
            if (_commands.ContainsKey(text))
            {
                _commands[text].Execute();
                return true;
            }
            else
            {
                Log.Warning(string.Format("Command {0} not found, no such command exists", text));
                return false;
            }
        }
    }
}
