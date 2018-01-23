using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MailMinion
{
    public class MailBoxCreator : IMailBoxCreator
    {
        IConfigurationMananger ConfigurationMananger { get; set; }
        IList<string> IgnoreList { get; set; }

        string MessageTemplate { get; set; }
        string FolderTemaplte { get; set; }
        string TabTemaplte { get; set; }
        string ModalTemplate { get; set; }

        public MailBoxCreator(IConfigurationMananger manager)
        {
            ConfigurationMananger = manager;

            LoadIgnoreList();
            LoadTemplates();
        }

        public void Run(string path, IList<string> folders)
        {
            int messageCount = 0;
            int errorCount = 0;
            int ignoreCount = 0;


            string name = Path.GetFileName(path);

            string messagesHtmlList = "";

            // Create a stream from the input file
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                // Create the output directory for this file
                Directory.CreateDirectory(ConfigurationMananger.Configuration.OutputPath + name);

                // Create the attachment directory
                string attachmentDirectory = ConfigurationMananger.Configuration.OutputPath + name + "\\attachments\\";
                Directory.CreateDirectory(attachmentDirectory);

                // Create the parser
                var parser = new MimeParser(stream, MimeFormat.Mbox);

                // Loop through all the messages
                while (!parser.IsEndOfStream)
                {
                    try
                    {
                        var attachments = new List<MimePart>();
                        var multiparts = new List<Multipart>();
                        bool skip = false;

                        messageCount++;
                        var message = parser.ParseMessage();
                        if (message.From.Count < 1)
                        {
                            //Console.WriteLine("Message with no From found: " + message.TextBody);
                            skip = true;
                            break;
                        }
                        else
                        {
                            foreach (MailboxAddress fromAddress in message.From)
                            {
                                if (fromAddress.Address.Split('@').Length < 1)
                                {
                                    Console.WriteLine(messageCount + ": There is a problem with this address: " + fromAddress.Address);
                                    skip = true;
                                }
                                else
                                {
                                    if (IgnoreList.Contains(fromAddress.Address.Split('@')[1]))
                                    {
                                        ignoreCount++;
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
                                var multipart = iter.Parent as Multipart;
                                var part = iter.Current as MimePart;

                                if (multipart != null && part != null && part.IsAttachment)
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
                            string attachmentName = attachmentDirectory + messageCount + "_" + attachmemntCount + "_" + attachment.FileName;

                            using (FileStream fs = File.OpenWrite(attachmentName))
                            {
                                attachment.Content.DecodeTo(fs);
                            }
                            attachmentNames.Add(attachmentName);
                            attachmemntCount++;
                        }

                        // Create a file for the email HTML content
                        string content = message.GetTextBody(MimeKit.Text.TextFormat.Html);
                        string fileName = ConfigurationMananger.Configuration.OutputPath + name + "\\" + messageCount + ".html";

                        if (string.IsNullOrEmpty(content))
                        {
                            content = message.GetTextBody(MimeKit.Text.TextFormat.Text);
                        }

                        File.WriteAllText(fileName, content);

                        // Create the HTML Message View
                        string messageHtml = MessageTemplate;

                        messageHtml = messageHtml.Replace("{BODY}", fileName);                        
                        messageHtml = messageHtml.Replace("{DATE}", message.Date.ToString());
                        messageHtml = messageHtml.Replace("{TO}", (message.To[0] as MailboxAddress).Address);
                        messageHtml = messageHtml.Replace("{FROM}", (message.From[0] as MailboxAddress).Address);
                        messageHtml = messageHtml.Replace("{ATTACHMENTS}", attachments.Count > 0 ? attachments.Count.ToString() : "");
                        messageHtml = messageHtml.Replace("{SUBJECT}", message.Subject);

                        if (attachments.Count > 0)
                        {
                            string attachmentHtml = "<table><tr>";

                            foreach(string an in attachmentNames)
                            {
                                if(an.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                                    an.EndsWith("png", StringComparison.OrdinalIgnoreCase) ||
                                    an.EndsWith("bmp", StringComparison.OrdinalIgnoreCase) ||
                                    an.EndsWith("gif", StringComparison.OrdinalIgnoreCase) ||
                                    an.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase))
                                {
                                    string modalHtml = ModalTemplate.Replace("{URL}", an);
                                    attachmentHtml += "<td><a class='modal-link' href='#nogo'><img src='" + an + "' height='50px' width='50x'/></a>" + modalHtml + "</td>";
                                }
                                else
                                {
                                    attachmentHtml += "<a href='" + an + "'a>" + Path.GetFileName(an) + "</a>";
                                }
                            }

                            attachmentHtml += "</tr></table>";
                            messageHtml = messageHtml.Replace("{ATTACHMENT}", attachmentHtml);
                        }
                        

                        messagesHtmlList += messageHtml;


                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        Console.WriteLine(messageCount + ":" + ex.Message);
                        stream.Position = parser.Position;
                        parser.SetStream(stream, MimeFormat.Mbox);
                        continue;
                    }
                }

                // Setup the tabs
                string tabsHtmlList = "";

                foreach (string p in folders)
                {
                    string fn = Path.GetFileName(p);

                    if (fn == name)
                        continue;

                    string tabsHtml = TabTemaplte;
                    tabsHtml = tabsHtml.Replace("{NAME}", fn);
                    tabsHtml = tabsHtml.Replace("{URL}", fn + ".html");
                    tabsHtmlList += tabsHtml;
                }

                // Create the folder
                string outputPath = ConfigurationMananger.Configuration.OutputPath + name + ".html";
                string folderHtml = FolderTemaplte;
                folderHtml = folderHtml.Replace("{TABS}", tabsHtmlList);
                folderHtml = folderHtml.Replace("{NAME}", name);
                folderHtml = folderHtml.Replace("{MAIL}", messagesHtmlList);

                                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                File.WriteAllText(outputPath, folderHtml);

            }

            Console.WriteLine("Finished Parsing " + messageCount + "(" + (messageCount - (errorCount + ignoreCount)) + ") messages with " + errorCount + " errors and " + ignoreCount + " ignored");
        }

        private void LoadTemplates()
        {
            MessageTemplate = File.ReadAllText("Resources\\message.html");
            TabTemaplte = File.ReadAllText("Resources\\tab.html");
            FolderTemaplte = File.ReadAllText("Resources\\folder.html");
            ModalTemplate = File.ReadAllText("Resources\\modal.html");
        }

        private void LoadIgnoreList()
        {
            IgnoreList = File.ReadAllLines(ConfigurationMananger.Configuration.IgnoreListPath);
        }
    }
}
