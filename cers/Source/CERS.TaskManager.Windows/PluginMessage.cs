using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CERS.TaskManager.Windows
{
    public class PluginMessage
    {
        public string PluginName { get; set; }

        public string Message { get; set; }

        public DateTime Received { get; set; }

        public bool Error { get; set; }

        public string ReceivedDisplay
        {
            get
            {
                return Received.ToString("MM/dd h:mm tt");
            }
        }
    }
}