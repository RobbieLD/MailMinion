using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MailMinion
{
    public class FileService : IFileService
    {
        private readonly IConfigurationMananger configurationMananger;
        public IList<string> IgnoreList { get; set; }

        public string Template
        {
            get
            {
                return File.ReadAllText(string.Format("{0}\\folder.cshtml", configurationMananger.Configuration.TemplateDirectory));
            }
        }

        public FileService(IConfigurationMananger manager)
        {
            configurationMananger = manager;
            IgnoreList = File.ReadAllLines(configurationMananger.Configuration.IgnoreListPath);
        }

        private void CreateOutputDirectories(string fileName)
        {
            // Create the output directory for this file
            string outputDirectory = string.Format("{0}{1}", configurationMananger.Configuration.OutputPath, fileName);
            Console.WriteLine("Creating {0}", outputDirectory);
            Directory.CreateDirectory(outputDirectory);

            // Create the attachment directory
            string attachmentDirectory = string.Format("{0}{1}{2}", configurationMananger.Configuration.OutputPath, fileName, configurationMananger.Configuration.AttachmentDirectory);
            Console.WriteLine("Creating {0}", attachmentDirectory);
            Directory.CreateDirectory(attachmentDirectory);
        }

        public void SaveEmail(string fileName, string content, int messageCount)
        {
            string path = string.Format("{0}{1}", configurationMananger.Configuration.OutputPath, fileName);
            string pathAndName = string.Format("{0}\\{1}.html", path, messageCount);
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText(pathAndName, content);
        }

        public string SaveAttachment(MimePart attachment, int messageCount, int attachmentCount)
        {
            string attachmentName = string.Format("{0}{1}_{2}_{3}", configurationMananger.Configuration.AttachmentDirectory, messageCount, attachmentCount, attachment.FileName);

            Console.WriteLine("Saving attachment: {0}", attachmentName);

            using (FileStream fs = File.OpenWrite(attachmentName))
            {
                attachment.Content.DecodeTo(fs);
            }

            return attachmentName;
        }

        public void SaveMailBox(MailBox mailBox, string fileName)
        {
            string path = string.Format("{0}{1}.html", configurationMananger.Configuration.OutputPath, fileName);
            File.WriteAllText(path, mailBox.Html);
        }

        public Stream GetMailStream(string path)
        {
            return new FileStream(path, FileMode.Open);
        }

        public bool IsImage(string fileName)
        {
            return configurationMananger.Configuration.ImageExtensions.Contains(Path.GetExtension(fileName));
        }
    }
}
