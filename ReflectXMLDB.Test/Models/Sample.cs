﻿using ReflectXMLDB.Interface;
using System.Xml.Serialization;

namespace ReflectXMLDB.Test.Models
{
    public class Sample : ICollectableObject
    {
        [XmlAttribute]
        public uint EID { get; set; }
        [XmlAttribute]
        public string GUID { get; set; }

        public string SomeData { get; set; }

        public Sample()
        {

        }
    }
}
