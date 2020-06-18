using ReflectXMLDB.Interface;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ReflectXMLDB.Test.Models
{
    public class SampleDatabase : IDatabase
    {
        [XmlAttribute]
        public string GUID { get; set; }
        public List<Sample> TestObjects { get; set; }

        public SampleDatabase()
        {
            TestObjects = new List<Sample>();
        }
    }
}
