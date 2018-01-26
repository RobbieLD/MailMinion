using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MailMinion
{
    public class MailBox
    {
        public string Html { get; set; }
        public int MessageCount { get; set; }
        public int ErrorCount { get; set; }
        public int IgnoreCount { get; set; }

        public string GetSummary()
        {
            return string.Format("MailBox Summary -> Total Messages: {0}, Totall Successful Messages: {1}, Total Errors: {2}, Total Ignored: {3}", 
                MessageCount, MessageCount - (ErrorCount + IgnoreCount), ErrorCount, IgnoreCount);
        }
    }
}
