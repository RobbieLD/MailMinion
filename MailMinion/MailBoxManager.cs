using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MailMinion.Models;

namespace MailMinion
{
    public class MailBoxManager : IMailBoxManager
    {
        private readonly IConfigurationMananger configurationMananger;
        private readonly IFileService fileService;

        public MailBoxManager(IConfigurationMananger cm, IFileService fs)
        {
            configurationMananger = cm;
            fileService = fs;
        }

        public void GenerateMailBoxes()
        {
            // Load config file file 
            string[] files = Directory.GetFiles(configurationMananger.Configuration.InputPath);

            List<Task> tasks = new List<Task>();

            // Generate the tabs content to be injected into the Razor view for each folder
            List<Tab> tabs = new List<Tab>();

            foreach (string file in files)
            {
                string fn = Path.GetFileName(file);

                tabs.Add(new Tab()
                {
                    Name = fn,
                    Url = string.Format("{0}.html", fn),
                });
            }

            // Read All the files in the input folder and process them
            foreach (string file in files)
            {
                Console.WriteLine("Processing " + file);

                tasks.Add(Task.Factory.StartNew(() => {
                    MailBoxCreator mailBoxCreator = new MailBoxCreator(fileService);
                    MailBox box = mailBoxCreator.Create(file, tabs);
                    Console.WriteLine(box.GetSummary());
                }));
            }

            File.Copy("Resources\\bulma.css", configurationMananger.Configuration.OutputPath + "bulma.css", true);
            
            Task.WaitAll(tasks.ToArray());
        }
    }
}
