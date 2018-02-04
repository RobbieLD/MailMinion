using MimeKit;
using System;
using System.Collections.Generic;
using MailMinion.Models;
using System.IO;
using RazorTemplates.Core;

namespace MailMinion
{
    public class MailBoxCreator : IMailBoxCreator
    {
        private readonly IFileService fileService;

        public MailBoxCreator(IFileService fs)
        {
            fileService = fs;
        }

        public MailBox Create(string path, IList<Tab> tabs)
        {
            // Create file which wil be used
            // TODO: Find a way to fix files with out escaped 'From' lines in body of message
            //string fixPath = FixFile(path);

            string fileName = Path.GetFileName(path);
            MailBox mailBox = new MailBox(fileName);

            // Create a stream from the input file
            using (var stream = fileService.GetMailStream(path))
            {
                // Create the parser
                var parser = new MimeParser(stream, MimeFormat.Mbox);
                
                Folder folderModel = new Folder
                {
                    Name = fileName,
                    Tabs = tabs
                };

                // Loop through all the messages
                while (!parser.IsEndOfStream)
                {
                    try
                    {
                        var attachments = new List<MimePart>();
                        var multiparts = new List<Multipart>();
                        bool skip = false;

                        mailBox.MessageCount++;
                        var message = parser.ParseMessage();

                        foreach (MailboxAddress fromAddress in message.From.Mailboxes)
                        {
                            if (fromAddress.Address.Split('@').Length < 1)
                            {
                                Console.WriteLine(string.Format("{0}:{1} -> There is a problem with this address: {2}", mailBox.MessageCount, fileName, fromAddress.Address));
                                skip = true;
                                mailBox.ErrorCount++;
                            }
                            else
                            {
                                if (fileService.IgnoreList.Contains(fromAddress.Address.Split('@')[1]))
                                {
                                    mailBox.IgnoreCount++;
                                    skip = true;
                                    break;
                                }
                            }
                        }               

                        if (skip)
                        {
                            skip = false;
                            continue;
                        }

                        using (var iter = new MimeIterator(message))
                        {
                            while (iter.MoveNext())
                            {
                                var part = iter.Current as MimePart;

                                if (iter.Parent is Multipart multipart && part != null && part.IsAttachment)
                                {
                                    multiparts.Add(multipart);
                                    attachments.Add(part);
                                }
                            }
                        }

                        int attachmemntCount = 1;
                        List<string> attachmentNames = new List<string>();

                        foreach (MimePart attachment in attachments)
                        {
                            string attachmentName = fileService.SaveAttachment(attachment, mailBox.MessageCount, attachmemntCount, folderModel.Name);
                            attachmentNames.Add(attachmentName);
                            attachmemntCount++;
                        }

                        // Create a file for the email HTML content
                        string content = message.GetTextBody(MimeKit.Text.TextFormat.Html);

                        if (string.IsNullOrEmpty(content))
                        {
                            content = message.GetTextBody(MimeKit.Text.TextFormat.Text);
                        }

                        // Write out the email HTML file
                        fileService.SaveEmail(fileName, content, mailBox.MessageCount);

                        Message messageModel = new Message()
                        {
                            Date = message.Date.Date,
                            // TODO: Support multiple Addresses
                            To = (message.To[0] as MailboxAddress).Address,
                            From = (message.From[0] as MailboxAddress).Address,
                            Subject = message.Subject,
                            FileName = string.Format(@"{0}\{1}.html", fileName, mailBox.MessageCount)
                        };

                        if (attachments.Count > 0)
                        {
                            foreach(string an in attachmentNames)
                            {
                                messageModel.Attachments.Add(new Attachment()
                                {
                                    Url = fileService.GetAttachmentPath(an, folderModel.Name),
                                    IsImage = fileService.IsImage(an),
                                    Name = fileService.GetAttachmentName(an)
                                });                                
                            }
                        }

                        folderModel.Messages.Add(messageModel);
                    }
                    catch (Exception ex)
                    {
                        mailBox.ErrorCount++;
                        Console.WriteLine(string.Format("{0}:{4} -> {1} ({2}:{3})", mailBox.MessageCount, ex.Message, parser.MboxMarker, parser.MboxMarkerOffset, fileName));
                        stream.Position = parser.Position;
                        parser.SetStream(stream, MimeFormat.Mbox);
                        continue;
                    }
                }

                // Render out the contents of the file
                var template = Template.Compile(fileService.Template);
                mailBox.Html = template.Render(folderModel);
            }

            // Save the file out using the file service
            fileService.SaveMailBox(mailBox, fileName);

            return mailBox;
        }        

        /// <summary>
        /// This method scans over the file and fixes any cases which might cause parsing errors. These are as follows. 
        /// 1. A line starting with 'From ' and not being preceded by a blank line. 
        /// </summary>
        /// <param name="path"></param>
        private string FixFile(string path)
        {
            string fixPath = string.Format("{0}.fix", path);
            Console.WriteLine("Creating .fix file {0}", fixPath);

            if (File.Exists(fixPath))
            {
                File.Delete(fixPath);
            }

            using (StreamReader reader = new StreamReader(path))
            using (StreamWriter writer = new StreamWriter(fixPath))
            {
                string currentLine, previousline = "";
                while((currentLine = reader.ReadLine()) != null)
                {
                    if (currentLine.StartsWith("From "))
                    {
                        if (!string.IsNullOrEmpty(previousline))
                        {
                            currentLine = currentLine.Replace("From ", "from ");
                        }
                    }

                    writer.WriteLine(currentLine);
                    previousline = currentLine;
                }
            }

            return fixPath;
        }
    }
}
