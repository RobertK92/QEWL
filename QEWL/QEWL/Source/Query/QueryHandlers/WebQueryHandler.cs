using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QEWL
{
    public class WebQueryHandler : QueryHandler
    {
        public WebQueryHandler(MainWindow mainWindow)
            : base(mainWindow)
        {

        }

        protected override void OnScan()
        {

        }

        protected override bool OnConfirmed(UIResultItem result)
        {
            return true;
        }

        protected override void OnQuery(string text)
        {
            UIResults results = new UIResults();
            //TODO: implement query
            results.Add(new UIResultItem(Environment.CurrentDirectory + "/Images/WebIcon.png", "Internet Search", "Not implemented yet"));

            
            QueryEnd(results);
        }
    }
}
