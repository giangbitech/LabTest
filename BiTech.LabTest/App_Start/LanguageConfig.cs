using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace BiTech.LabTest
{
    public class LanguageConfig
    {
        public static Dictionary<string, string> Dictionary { get; set; }

        public static void RegisterLanguage()
        {
            string jsonText = File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("/language.json"));

            var languageInJson = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(jsonText);

            Dictionary = new Dictionary<string, string>();
            foreach (var item in languageInJson)
            {
                Dictionary.Add(item.Key, item.Value.ToString());
            }
        }
    }
}