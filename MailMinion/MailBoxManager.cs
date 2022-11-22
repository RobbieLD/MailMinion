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
            string[] files = Directory.GetFiles(configurationMananger.Configuration.InputPath, "*", SearchOption.AllDirectories);

            List<Tab> tabs = new List<Tab>();

            foreach(string file in files)
            {
                string fn = Path.GetFileName(file);
                string ext = Path.GetFileNameWithoutExtension(file);

                if (fn == ext)
                {
                    tabs.Add(new Tab()
                    {
                        Name = fn,
                        Url = string.Format("{0}.html", fn),
                        RawPath = file
                    });
                }
            }

            Console.WriteLine("The following files will be processed");

            foreach(Tab tab in tabs)
            {
                Console.WriteLine(tab.RawPath);
            }

            Console.Write("Is this correct? (y/n):");
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            if (keyInfo.KeyChar != 'Y' && keyInfo.KeyChar != 'y')
            {
                Console.WriteLine();
                return;
            }

            Console.WriteLine();

            List<Task> tasks = new List<Task>();

            // Read All the files in the input folder and process them
            foreach (Tab tab in tabs)
            {
                Console.WriteLine("Processing " + tab.Url);

                tasks.Add(Task.Factory.StartNew(() => {
                    MailBoxCreator mailBoxCreator = new MailBoxCreator(fileService);
                    MailBox box = mailBoxCreator.Create(tab.RawPath, tabs);
                    Console.WriteLine(box.GetSummary());
                }));
            }

            Task.WaitAll(tasks.ToArray());

            // Wait for above tasks to complete so as "Dump" folder is known to exist
            File.Copy(@"Resources\bulma.css", Path.Combine(configurationMananger.Configuration.OutputPath, "bulma.css"), true);
        }
    }
}
