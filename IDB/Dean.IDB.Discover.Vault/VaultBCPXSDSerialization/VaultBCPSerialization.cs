using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

using System.Xml.Serialization;
using System.Xml;

namespace VaultBCPXSDSerialization
{
    public class VaultBCPSerialization
    {
        // "Statistics", "Security", "Behaviors", "Folder", "File"

        // method to get the statistics from a Vault.xml file
        static public VaultStatistics GetVaultStatistics(string inputXMLFilename)
        {
            VaultStatistics vaultStatistics = null;

            // setup the settings, Filestream, and reader
            XmlReaderSettings xmlReadSet = new XmlReaderSettings();
            xmlReadSet.IgnoreComments = true;
            xmlReadSet.IgnoreWhitespace = true;
            FileStream mFileStream = new FileStream(inputXMLFilename, FileMode.Open);
            XmlReader mXmlReader = XmlReader.Create(mFileStream, xmlReadSet);


            // read to the first ExtractedDataNode
            try
            {
                if (mXmlReader.ReadToFollowing("Statistics"))
                {
                    // get the namespace of the current node
                    string xmlNameSpace = mXmlReader.NamespaceURI;

                    // make sure to include the encoding
                    string sOuterXML = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + mXmlReader.ReadOuterXml();

                    // create the serializer
                    XmlRootAttribute xRoot = new XmlRootAttribute();
                    xRoot.ElementName = "Statistics";
                    xRoot.IsNullable = true;
                    xRoot.Namespace = xmlNameSpace;
                    XmlSerializer mDeserializer = new XmlSerializer(typeof(VaultStatistics), xRoot);

                    vaultStatistics = (VaultStatistics)mDeserializer.Deserialize(new StringReader(sOuterXML));
                }
            }
            catch (Exception exc)
            {
                // consume the exception as the file propbably does not have a Statistics section
            }

            // close the resources
            mXmlReader.Close();
            mFileStream.Close();

            return (vaultStatistics);
        }

        // method to get the Security from a Vault.xml file
        static public VaultSecurity GetVaultSecurity(string inputXMLFilename)
        {
            VaultSecurity vaultSecurity = null;

            // setup the settings, Filestream, and reader
            XmlReaderSettings xmlReadSet = new XmlReaderSettings();
            xmlReadSet.IgnoreComments = true;
            xmlReadSet.IgnoreWhitespace = true;
            FileStream mFileStream = new FileStream(inputXMLFilename, FileMode.Open);
            XmlReader mXmlReader = XmlReader.Create(mFileStream, xmlReadSet);

            // read to the first ExtractedDataNode
            try
            {
                if (mXmlReader.ReadToDescendant("Security"))
                {
                    // get the namespace of the current node
                    string xmlNameSpace = mXmlReader.NamespaceURI;

                    // make sure to include the encoding
                    string sOuterXML = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + mXmlReader.ReadOuterXml();

                    // create the serializer
                    XmlRootAttribute xRoot = new XmlRootAttribute();
                    xRoot.ElementName = "Security";
                    xRoot.IsNullable = true;
                    xRoot.Namespace = xmlNameSpace;
                    XmlSerializer mDeserializer = new XmlSerializer(typeof(VaultSecurity), xRoot);

                    vaultSecurity = (VaultSecurity)mDeserializer.Deserialize(new StringReader(sOuterXML));
                }
            }
            catch (Exception exc)
            {
                // consume the exception as the file propbably does not have a Security section
            }

            // close the resources
            mXmlReader.Close();
            mFileStream.Close();

            return (vaultSecurity);
        }

        // method to get the VaultBehaviors from a Vault.xml file
        static public VaultBehaviors GetVaultBehaviors(string inputXMLFilename)
        {
            VaultBehaviors vaultBehaviors = null;

            // setup the settings, Filestream, and reader
            XmlReaderSettings xmlReadSet = new XmlReaderSettings();
            xmlReadSet.IgnoreComments = true;
            xmlReadSet.IgnoreWhitespace = true;
            FileStream mFileStream = new FileStream(inputXMLFilename, FileMode.Open);
            XmlReader mXmlReader = XmlReader.Create(mFileStream, xmlReadSet);

            // read to the first ExtractedDataNode
            try
            {
                if (mXmlReader.ReadToDescendant("Behaviors"))
                {
                    // get the namespace of the current node
                    string xmlNameSpace = mXmlReader.NamespaceURI;

                    // make sure to include the encoding
                    string sOuterXML = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>" + mXmlReader.ReadOuterXml();

                    // create the serializer
                    XmlRootAttribute xRoot = new XmlRootAttribute();
                    xRoot.ElementName = "Behaviors";
                    xRoot.IsNullable = true;
                    xRoot.Namespace = xmlNameSpace;
                    XmlSerializer mDeserializer = new XmlSerializer(typeof(VaultBehaviors), xRoot);

                    vaultBehaviors = (VaultBehaviors)mDeserializer.Deserialize(new StringReader(sOuterXML));
                }
            }
            catch (Exception exc)
            {
                // consume the exception as the file propbably does not have a Behaviors section
            }


            // close the resources
            mXmlReader.Close();
            mFileStream.Close();

            return (vaultBehaviors);
        }

        // method to get the whole Vault from a Vault.xml file
        static public Vault GetVault(string inputXMLFilename)
        {
            Vault vault = null;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            XmlSerializer serializer = new XmlSerializer(typeof(Vault));

            StreamReader sr = new StreamReader(inputXMLFilename);
            vault = (Vault)serializer.Deserialize(sr);

            sw.Stop();

            return (vault);
        }

        // method to write the Vault to an Vault.xml file
        static public void WriteVault(Vault sourceVault, string outputXMLFilename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Vault));

            StreamWriter sw = new StreamWriter(outputXMLFilename);
            serializer.Serialize(sw, sourceVault);
        }

        // method to get the whole Items from a ItemsWrapper.xml file
        static public Itemswrapper GetItemsWrapper(string inputXMLFilename)
        {
            Itemswrapper itemsWrapper = null;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            XmlSerializer serializer = new XmlSerializer(typeof(Itemswrapper));

            StreamReader sr = new StreamReader(inputXMLFilename);
            itemsWrapper = (Itemswrapper)serializer.Deserialize(sr);

            sw.Stop();

            return (itemsWrapper);
        }

        // method to write the ItemsWrapper to an ItemsWrapper.xml file
        static public void WriteItemsWrapper(Itemswrapper sourceItemsWrapper, string outputXMLFilename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Itemswrapper));

            StreamWriter sw = new StreamWriter(outputXMLFilename);
            serializer.Serialize(sw, sourceItemsWrapper);
        }

        // method to get the whole Vault from a Vault.xml file
        static public BOMwrapper GetBomWrapper(string inputXMLFilename)
        {
            BOMwrapper bomWrapper = null;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            XmlSerializer serializer = new XmlSerializer(typeof(BOMwrapper));

            StreamReader sr = new StreamReader(inputXMLFilename);
            bomWrapper = (BOMwrapper)serializer.Deserialize(sr);

            sw.Stop();

            return (bomWrapper);
        }

        // method to write the BOMwrapper to an BomWrapper.xml file
        static public void WriteBomWrapper(BOMwrapper sourceBomWrapper, string outputXMLFilename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BOMwrapper));

            StreamWriter sw = new StreamWriter(outputXMLFilename);
            serializer.Serialize(sw, sourceBomWrapper);
        }

    }


    // NOTE: currently not using/developing this until we get an XML file too large
    // class that manages the Asynchronous Deserialization Of Files and Folders
    // string sInputXMLFilename, int nSkipFirstNRecords = 0, int nNumberToGet = 0)
    public class AsynchronousDeserializeManagementObject
    {
        protected string sInputXMLFilename;
        protected int nSkipFirstNRecords;
        protected int nNumberToGet;

        protected FileStream mFileStream;
        protected XmlReader mXmlReader;
        protected XmlSerializer mFileDeserializer;
        protected XmlSerializer mFolderDeserializer;

        protected int nIndex;


        // constructor
        public AsynchronousDeserializeManagementObject(string inputXMLFilename, int skipFirstNRecords = 0, int numberToGet = 0)
        {
            // set the attributes
            sInputXMLFilename = inputXMLFilename;
            nSkipFirstNRecords = skipFirstNRecords;
            nNumberToGet = numberToGet;

            // setup the settings, Filestream, and reader
            XmlReaderSettings xmlReadSet = new XmlReaderSettings();
            xmlReadSet.IgnoreComments = true;
            xmlReadSet.IgnoreWhitespace = true;
            mFileStream = new FileStream(sInputXMLFilename, FileMode.Open);
            mXmlReader = XmlReader.Create(mFileStream, xmlReadSet);


            // create the serializer
            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "Node";
            xRoot.IsNullable = true;
            mFileDeserializer = new XmlSerializer(typeof(File), xRoot);

            // read to the first ExtractedDataNode
            bool bContinue = mXmlReader.ReadToDescendant("Node");

            // TODO: figure out what we do if we find no ExtractedDataNode in the XML file

            // set the start index to cound records
            nIndex = 0;
        }

        //// get the next node avaliable
        //public ExtractedDataNode GetNextExtractedDataNode()
        //{
        //    // process until we find the first of interest
        //    // process white spaces
        //    bool bContinue = true;

        //    // check to see if we are past the number to get
        //    if ((nNumberToGet > 0) && ((nIndex - nSkipFirstNRecords) > nNumberToGet))
        //    {
        //        bContinue = false;
        //    }

        //    // parse until we have found the first entity or the last
        //    // next time it is called, will close resources and return null
        //    while (bContinue)
        //    {
        //        if (mXmlReader.IsStartElement() && (string.Compare(mXmlReader.Name, "Node", true) == 0))
        //        {
        //            // make sure to include the encoding - used by our AcadXrefsRealDWG extractor
        //            // keep ReadOuterXml as it will advance to the next node
        //            string sOuterXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?> " + mXmlReader.ReadOuterXml();

        //            nIndex++;
        //            if (nIndex > nSkipFirstNRecords)
        //            {
        //                ExtractedDataNode extractedDataNode = (ExtractedDataNode)mDeserializer.Deserialize(new StringReader(sOuterXml));
        //                return (extractedDataNode);
        //            }
        //        }
        //        else
        //        {
        //            // read the next element or content
        //            // until we find "Node" or the end of the stream
        //            bContinue = mXmlReader.Read();
        //        }
        //    }

        //    // close the read and filestream
        //    mXmlReader.Close();
        //    mFileStream.Close();

        //    // set them to null
        //    mXmlReader = null;
        //    mFileStream = null;

        //    return (null);
        //}

    }

}
