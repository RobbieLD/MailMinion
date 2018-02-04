using System;
using System.Collections.Generic;
using System.Text;

namespace MailMinion.Models
{
    public class Attachment
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public bool IsImage { get; set; }
    }
}
