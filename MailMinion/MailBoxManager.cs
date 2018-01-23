using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MailMinion
{
    public class MailBoxManager : IMailBoxManager
    {
        IConfigurationMananger ConfigurationMananger { get; set; }

        public MailBoxManager(IConfigurationMananger manager)
        {
            ConfigurationMananger = manager;
        }

        public void GenerateMailBoxes()
        {
            // Load config file file 
            string[] files = Directory.GetFiles(ConfigurationMananger.Configuration.InputPath);

            List<Task> tasks = new List<Task>();

            // Read All the files in the input folder and process them
            foreach (string file in files)
            {
                Console.WriteLine("Processing " + file);
                tasks.Add(Task.Factory.StartNew(() => {
                    new MailBoxCreator(ConfigurationMananger).Run(file, files);
                }));
            }

            File.Copy("Resources\\bulma.css", ConfigurationMananger.Configuration.OutputPath + "bulma.css", true);
            
            Task.WaitAll(tasks.ToArray());
        }
    }
}
