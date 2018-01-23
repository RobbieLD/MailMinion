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
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                Configuration = JsonConvert.DeserializeObject<Config>(json);
            }
        }
    }
}
