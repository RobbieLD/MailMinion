using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;

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
                return File.ReadAllText(string.Format(@"Views\folder.cshtml"));
            }
        }

        public FileService(IConfigurationMananger manager)
        {
            configurationMananger = manager;

            if (File.Exists(configurationMananger.Configuration.IgnoreListPath))
            {
                IgnoreList = File.ReadAllLines(configurationMananger.Configuration.IgnoreListPath);
            }else
            {
                IgnoreList = new List<string>();
            }
        }

        private void CreateOutputDirectories(string fileName)
        {
            // Create the output directory for this file
            string outputDirectory = string.Format("{0}{1}", configurationMananger.Configuration.OutputPath, fileName);
            //Console.WriteLine("Creating {0}", outputDirectory);
            Directory.CreateDirectory(outputDirectory);

            // Create the attachment directory
            string attachmentDirectory = string.Format("{0}{1}{2}", configurationMananger.Configuration.OutputPath, fileName, configurationMananger.Configuration.AttachmentDirectory);
            //Console.WriteLine("Creating {0}", attachmentDirectory);
            Directory.CreateDirectory(attachmentDirectory);
        }

        public void SaveEmail(string fileName, string content, int messageCount)
        {
            string path = string.Format("{0}{1}", configurationMananger.Configuration.OutputPath, fileName);
            string pathAndName = string.Format(@"{0}\{1}.html", path, messageCount);
            
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText(pathAndName, content);
        }

        public string SaveAttachment(MimePart attachment, int messageCount, int attachmentCount, string folderName)
        {
            string directoryPath = string.Format("{0}{1}{2}", 
                configurationMananger.Configuration.OutputPath,
                folderName,
                configurationMananger.Configuration.AttachmentDirectory);

            string attachmentName = string.Format("{0}_{1}_{2}",  
                messageCount, 
                attachmentCount, 
                attachment.FileName);

            string attachmentPath = string.Format("{0}{1}", directoryPath, attachmentName);

            //Console.WriteLine("Saving attachment: {0}", attachmentPath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (FileStream fs = File.OpenWrite(attachmentPath))
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
            return configurationMananger.Configuration.ImageExtensions.FindIndex(x => x.Equals(Path.GetExtension(fileName), StringComparison.OrdinalIgnoreCase)) != -1;
        }

        public string GetAttachmentPath(string attachmentName, string folderName)
        {
            return string.Format(@"{0}\{1}\{2}", folderName, configurationMananger.Configuration.AttachmentDirectory, attachmentName);
        }

        public string GetAttachmentName(string path)
        {
            return Path.GetFileName(path);
        }
    }
}
