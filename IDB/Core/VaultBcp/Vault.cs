using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace IDB.Core.VaultBcp
{
    [Serializable]
    public class Vault
    {
        [XmlElement("Root")] public Root Root { get; set; }
        [XmlElement("Behaviors")] public Behaviors Behaviors { get; set; }
    }

    [Serializable]
    public class Behaviors
    {
        [XmlElement("PropertyDefinition")]
        public List<PropertyDefinition> PropertyDefinitions { get; } = new List<PropertyDefinition>();

        [Serializable]
        public class PropertyDefinition
        {
            [XmlAttribute] public string Name { get; set; } = "";
            [XmlAttribute] public string Type { get; set; } = "String"; //String, Numeric, DateTime, Bool
            [XmlElement("Assignment")] public List<Assignment> Assignments { get; } = new List<Assignment>();
        }

        [Serializable]
        public class Assignment
        {
            [XmlAttribute] public string Class { get; set; } = "";
        }
    }

    [Serializable]
    public class Root
    {
        [XmlElement("Folder")] public List<Folder> Folders { get; } = new List<Folder>();
        [XmlElement("File")] public List<File> Files { get; } = new List<File>();
        [XmlElement("Link")] public List<Link> Links { get; } = new List<Link>();
    }

    [Serializable]
    public class Folder : Root
    {
        [XmlAttribute] public string Name { get; set; } = "";
        [XmlAttribute] public string IsLibrary { get; set; }
        [XmlAttribute] public string Category { get; set; }
        [XmlAttribute] public string Id { get; set; }
        [XmlElement("Created")] public Created Created { get; set; } = new Created();
        [XmlElement("State")] public State State { get; set; }
        [XmlElement("UDP")] public List<UDP> UDPs { get; } = new List<UDP>();
    }

    [Serializable]
    public class File
    {
        [XmlAttribute] public string Name { get; set; } = "";
        [XmlAttribute] public string Classification { get; set; }
        [XmlAttribute] public string Hidden { get; set; }
        [XmlAttribute] public string Category { get; set; }
        [XmlElement("Revision")] public List<Revision> Revisions { get; } = new List<Revision>();

        [Serializable]
        public class Revision
        {
            [XmlAttribute] public string Definition { get; set; }
            [XmlAttribute] public string Label { get; set; }
            [XmlElement("IterationRef")] public List<IterationRef> IterationRefs { get; } = new List<IterationRef>();
            [XmlElement("Iteration")] public List<Iteration> Iterations { get; } = new List<Iteration>();

            [Serializable]
            public class IterationRef
            {
                [XmlAttribute] public DateTime CreateDate { get; set; }
                [XmlAttribute] public int Checksum { get; set; }
                [XmlAttribute] public string Id { get; set; }
            }

            [Serializable]
            public class Iteration
            {
                [XmlAttribute] public string Comment { get; set; }
                [XmlAttribute] public DateTime Modified { get; set; }
                [XmlAttribute] public string LocalPath { get; set; } = "";
                [XmlAttribute] public string Id { get; set; }
                [XmlAttribute] public string ContentSource { get; set; }
                [XmlElement("Created")] public Created Created { get; set; } = new Created();
                [XmlElement("State")] public State State { get; set; }
                [XmlElement("UDP")] public List<UDP> UDPs { get; } = new List<UDP>();
                [XmlElement("Association")] public List<Association> Associations { get; } = new List<Association>();

                [Serializable]
                public class Association
                {
                    [XmlAttribute] public string ChildId { get; set; } = "";
                    [XmlAttribute] public string Type { get; set; }
                    [XmlAttribute] public string Source { get; set; }
                    [XmlAttribute] public string RefId { get; set; }
                    [XmlAttribute] public string NeedsResolution { get; set; }
                }
            }
        }
    }

    [Serializable]
    public class Link
    {
        [XmlAttribute] public string TargetId { get; set; } = "";
        [XmlElement("UDP")] public List<UDP> UDPs { get; } = new List<UDP>();
    }

    [Serializable]
    public class Created
    {
        [XmlAttribute] public string User { get; set; } = "";
        [XmlAttribute] public DateTime Date { get; set; }

    }

    [Serializable]
    public class State
    {
        [XmlAttribute] public string Definition { get; set; } = "";
        [XmlAttribute] public string Name { get; set; } = "";
    }

    [Serializable]
    public class UDP
    {
        [XmlAttribute] public string Name { get; set; }
        [XmlText] public string Value { get; set; }
    }
}