using System.Collections.Generic;

namespace MailMinion
{
    public class Config
    {
        public string OutputPath { get; set; }
        public string InputPath { get; set; }
        public string IgnoreListPath { get; set; }
        public string TemplateDirectory { get; set; }
        public string AttachmentDirectory { get; set; }
        public IList<string> ImageExtensions { get; set; }
    }
}
