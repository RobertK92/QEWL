using Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QEWL
{
    public abstract class Command
    {
        public string Description { get; protected set; }

        protected abstract void OnExecute();
        
        public Command(string description = "")
        {
            Description = description;
        }

        public void Execute()
        {
            Log.Message(string.Format("Executing command: '{0}'", GetType().Name));
            OnExecute();
        }
    }
}
