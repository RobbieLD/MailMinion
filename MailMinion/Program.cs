using System;
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
                .AddSingleton<IFileService, FileService>()
                .BuildServiceProvider();

            serviceProvider.GetService<IMailBoxManager>().GenerateMailBoxes();

            Console.WriteLine("Processing Done");
            Console.ReadKey();
        }
    }
}
