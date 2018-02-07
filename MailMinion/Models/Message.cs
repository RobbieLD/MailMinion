using System;
using System.Collections.Generic;
using System.Text;

namespace MailMinion.Models
{
    public class Message
    {
        public Message()
        {
            Attachments = new List<Attachment>();
        }

        public DateTime Date { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string FileName { get; set; }
        public IList<Attachment> Attachments { get; set; }
        public int AttachmentCount
        {
            get
            {
                return Attachments.Count;
            }
        }
    }
}
