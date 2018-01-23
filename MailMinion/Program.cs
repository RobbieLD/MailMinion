using System;
using System.IO;
using MimeKit;
using MailKit.Net.Imap;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MailMinion
{
    class Program
    {
        static void Main(string[] args)
        {
            // Hook up the DI
            IServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton<IConfigurationMananger, ConfigurationMananger>(provider => new ConfigurationMananger("app.json"))
                .AddSingleton<IMailBoxManager, MailBoxManager>()
                .BuildServiceProvider();

            serviceProvider.GetService<IMailBoxManager>().GenerateMailBoxes();

            Console.WriteLine("Processing Done");
            Console.ReadKey();
        }
    }
}
