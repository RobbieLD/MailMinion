using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MailMinion
{
    public class ConfigurationMananger : IConfigurationMananger
    {
        public Config Configuration { get; set; }

        public ConfigurationMananger(string path)
        {
            using (StreamReader r = new StreamReader(path, true))
            {
                Console.WriteLine("Reading config file {0}", path);
                string json = r.ReadToEnd();

                if (json[0] == 65279)
                {
                    json = json.Substring(1, json.Length - 1);
                }

                Console.WriteLine(json);
                Configuration = JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }
}
