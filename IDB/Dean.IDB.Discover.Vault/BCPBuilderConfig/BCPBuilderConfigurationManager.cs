using System;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Xml;

using log4net;

namespace BCPBuilderConfig
{
    public class BCPBuilderConfigurationManager
    {
        // setup logger information
        // this is used by all dll's in the solution
        static private string configFile = Assembly.GetExecutingAssembly().Location + ".config";
        static private ILog log = BCPBuilderConfigurationManager.GetLogger(typeof(BCPBuilderConfigurationManager));
        static private Dictionary<string, Log4NetExtender> logtExtenderDictionary = new Dictionary<string, Log4NetExtender>();

        // constructor
        public BCPBuilderConfigurationManager() { }

        // get the logger for this dll and all that want to use the same logger
        public static ILog GetLogger(System.Type type)
        {
            /// get the logger managed by this dll
            if (log4net.LogManager.GetCurrentLoggers().Length == 0)
            {
                // load logger config with XmlConfigurator
                string dllPath = Assembly.GetExecutingAssembly().Location;
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(dllPath);

                // get the configuration section
                ConfigurationSection configSection = configuration.GetSection("log4net");
                string sLog4NetString = configSection.SectionInformation.GetRawXml();

                // get the memory stream from the string
                MemoryStream memoryStream = ConvertStringToStream(sLog4NetString);

                // use the configuration from the dll app.config to configure Log4Net
                log4net.Config.XmlConfigurator.Configure(memoryStream);

                // indicate that logging has started
                ILog logger = LogManager.GetLogger(type);
                logger.Info("Logging enabled and configured in: " + configFile);
            }

            // return the logger
            return (LogManager.GetLogger(type));
        }

        public static Log4NetExtender GetExtendedLogger(System.Type type)
        {
            Log4NetExtender log4NetExtender;
            if (!logtExtenderDictionary.ContainsKey(type.Name))
            {
                ILog iLog = LogManager.GetLogger(type);
                Log4NetExtender logExtender = new Log4NetExtender(iLog);
                logtExtenderDictionary.Add(type.Name, logExtender);
            }

            log4NetExtender = logtExtenderDictionary[type.Name];

            // return the logger
            return (log4NetExtender);
        }





        // convert the string to a stream
        public static MemoryStream ConvertStringToStream(string sVal)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(sVal);
            MemoryStream stream = new MemoryStream(byteArray);

            return (stream);
        }


        // method to get the connection string from the configuration file
        public static string GetConnectionString(string sConnectStringName)
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(dllPath);

            string connectionString = configuration.ConnectionStrings.ConnectionStrings[sConnectStringName].ConnectionString;

            // set the connection string
            return (connectionString);
        }

        // get app settings values
        public static string GetAppSetting(string sAppSettingName)
        {
            string dllPath = Assembly.GetExecutingAssembly().Location;
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(dllPath);

            string result = configuration.AppSettings.Settings[sAppSettingName].Value;

            // check to see if it is null
            if (result == null)
            {
                result = "Not found";
            }

            // return the value found
            return (result);
        }

        // mehtod to get a configuraiton collection
        public static NameValueCollection GetNameValueCollection(string sCollectionName)
        {
            // setup a null name value collection
            NameValueCollection nameValueCollection = null;

            try
            {
                // load logger config with XmlConfigurator
                string dllPath = Assembly.GetExecutingAssembly().Location;
                Configuration configuration = ConfigurationManager.OpenExeConfiguration(dllPath);

                // get the dictionary section
                ConfigurationSection configSection = configuration.GetSection(sCollectionName);

                // get the raw xml to read collection
                string configSectionRawXml = configSection.SectionInformation.GetRawXml();
                XmlDocument sectionXmlDoc = new XmlDocument();
                sectionXmlDoc.Load(new StringReader(configSectionRawXml));

                // create a NameValueSectionHandler to parse the content
                NameValueSectionHandler handler = new NameValueSectionHandler();

                // get the name value collection
                nameValueCollection = (NameValueCollection)handler.Create(null, null, sectionXmlDoc.DocumentElement);
            }
            catch (Exception exc)
            {
                log.Error("Could not parse name value section: " + sCollectionName, exc);
            }

            return (nameValueCollection);
        }
    }
}

