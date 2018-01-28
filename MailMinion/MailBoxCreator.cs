using System.Reflection;
using RazorLight;
using MimeKit;
using System;
using System.Collections.Generic;
using MailMinion.Models;
using System.IO;

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
            MailBox mailBox = new MailBox();
            string fileName = Path.GetFileName(path);

            // Create a stream from the input file
            using (var stream = fileService.GetMailStream(path))
            {
                // Create the parser
                var parser = new MimeParser(stream, MimeFormat.Mbox);

                Folder folderModel = new Folder();

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
                        if (message.From.Count < 1)
                        {
                            skip = true;
                            break;
                        }
                        else
                        {
                            foreach (MailboxAddress fromAddress in message.From)
                            {
                                if (fromAddress.Address.Split('@').Length < 1)
                                {
                                    Console.WriteLine(string.Format("{0}: There is a problem with this address: {1}", mailBox.MessageCount, fromAddress.Address));
                                    skip = true;
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
                            string attachmentName = fileService.SaveAttachment(attachment, mailBox.MessageCount, attachmemntCount);
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
                            FileName = fileName
                        };

                        if (attachments.Count > 0)
                        {
                            foreach(string an in attachmentNames)
                            {
                                messageModel.Attachments.Add(new Attachment()
                                {
                                    Url = an,
                                    IsImage = fileService.IsImage(an)
                                });                                
                            }
                        }

                        folderModel.Messages.Add(messageModel);
                    }
                    catch (Exception ex)
                    {
                        mailBox.ErrorCount++;
                        Console.WriteLine(string.Format("{0}:{1}", mailBox.MessageCount, ex.Message));
                        stream.Position = parser.Position;
                        parser.SetStream(stream, MimeFormat.Mbox);
                        continue;
                    }
                }

                // Render out the contents of the file
                var engine = new RazorLightEngineBuilder()
                    .UseMemoryCachingProvider()
                    .Build();

                mailBox.Html = engine.CompileRenderAsync("folder", fileService.Template, folderModel).Result;
            }

            // Save the file out using the file service
            fileService.SaveMailBox(mailBox, fileName);

            return mailBox;
        }        
    }
}
