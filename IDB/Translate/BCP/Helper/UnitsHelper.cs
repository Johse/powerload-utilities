using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using bcpDevKit.Entities.UoM;
using log4net;

namespace IDB.Translate.BCP.Helper
{
    class UnitsHelper
    {
        private static readonly ILog Log = LogManager.GetLogger("IDBTranslateBCP");

        private static UnitsHelper _instance;
        public static UnitsHelper Instance
        {
            get { return _instance ?? (_instance = new UnitsHelper()); }
        }

        internal XNamespace UomWrapperNamespace = "http://schemas.autodesk.com/pseb/dm/DataImport/uom/2015-04-14";
        public List<UOMDef> UOMDefs { get; set; }

        public UnitsHelper()
        {
            ReadUomDefFromFile();
        }

        public bool HasCustomUnitDefinitions()
        {
            return UOMDefs != null;
        }

        public int GetUifMid(string unitName)
        {
            var res = 1;
            foreach (var def in UOMDefs.Where(def => string.Compare(def.Abbreviation, unitName, StringComparison.InvariantCultureIgnoreCase) == 0
                                                         || string.Compare(def.UnitName, unitName, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                Log.Debug($"Using custom unit definition for unit'{unitName}': Id = {def.UofMID}");
                return def.UofMID;
            }

            Log.Warn($"No custom unit definition found for unit '{unitName}': Using Id = 1 ('Each')");
            return res;
        }

        public void OverrideCustomUnitDefinitionsFile(string destinationFolder)
        {
            try
            {
                var customUomFile = FindCustomUnitDefinitionsFile();
                var destinationFilename = Path.Combine(destinationFolder, Path.GetFileName(customUomFile));
                File.Copy(customUomFile, destinationFilename, true);
                Log.Info("Successfully copied custom 'UOMwrapper.xml' to BCP folder!");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to copy custom 'UOMwrapper.xml' to BCP folder", ex);
            }
        }

        private void ReadUomDefFromFile()
        {
            try
            {
                var customUomFile = FindCustomUnitDefinitionsFile();
                if (string.IsNullOrEmpty(customUomFile))
                    return;

                // read unit definitions from file
                UOMDefs = new List<UOMDef>();
                var xDoc = XElement.Load(customUomFile);
                foreach (var xDef in xDoc.Descendants(UomWrapperNamespace + "def"))
                {
                    var def = new UOMDef
                    {
                        UofMID = System.Convert.ToInt32(xDef.Attribute("UofMID")?.Value),
                        UnitName = xDef.Attribute("UnitName")?.Value,
                        Abbreviation = xDef.Attribute("Abbreviation")?.Value,
                        Conversion = xDef.Attribute("Conversion")?.Value,
                        BaseUofMID = System.Convert.ToInt32(xDef.Attribute("BaseUofMID")?.Value),
                        SystemName = xDef.Attribute("SystemName")?.Value
                    };
                    UOMDefs.Add(def);
                }
                Log.Info("Using custom unit definitions!");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to read custom 'UOMwrapper.xml'", ex);
                UOMDefs = null;
            }
        }

        private string FindCustomUnitDefinitionsFile()
        {
            try
            {
                var thisAssembly = Assembly.GetExecutingAssembly();
                var uomFile = Path.Combine(Path.GetDirectoryName(thisAssembly.Location), "UOMwrapper.xml");
                return File.Exists(uomFile) ? uomFile : null;
            }
            catch (Exception ex)
            {
                Log.Error("Error finding custom 'UOMwrapper.xml'", ex);
                return null;
            }
        }
    }
}
