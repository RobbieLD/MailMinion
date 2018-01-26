using System;
using System.Collections.Generic;
using System.Text;

namespace MailMinion.Models
{
    public class Folder
    {
        public Folder()
        {
            Messages = new List<Message>();
            Tabs = new List<Tab>();
        }
        public string Name { get; set; }

        public IList<Tab> Tabs { get; set; }

        public IList<Message> Messages { get; set; }
    }
}
