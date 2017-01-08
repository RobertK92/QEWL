using Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QEWL
{
    public class TestCommand : Command
    {
        protected override void OnExecute()
        {
            Log.Message("TEST COMMAND EXECUTED");
        }
    }
}
