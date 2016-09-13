using System.Configuration;

namespace BiTech.LabTest.BLL
{
    public class Tool
    {
        public static string GetConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key]?.ToString();
        }
    }
}
