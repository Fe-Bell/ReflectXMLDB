using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ReflectXMLDB.Generic
{
    /// <summary>
    /// Provides extension methods for the ReflectXMLDB.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Deletes the contents of a directory.
        /// </summary>
        /// <param name="directory"></param>
        public static void Clean(this DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                subDirectory.Delete(true);
            }
        }

        /// <summary>
        /// Return true if the object is null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNull<T>(this T value)
        {
            return value == null;
        }

        /// <summary>
        /// Enables "foreach" looping for ObservableCollections.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="action"></param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var cur in enumerable)
            {
                action(cur);
            }
        }

        /// <summary>
        /// Removes a selected list of items from a collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="predicate"></param>
        public static void RemoveAll<T>(this ICollection<T> collection, Func<T, bool> predicate)
        {
            List<T> list = GetList(collection);

            if (!list.IsNull())
            {
                list.RemoveAll(new Predicate<T>(predicate));
            }
            else
            {
                List<T> itemsToDelete = collection.Where(predicate).ToList();

                foreach (var item in itemsToDelete)
                {
                    collection.Remove(item);
                }
            }
        }

        /// <summary>
        /// Returns a list from an ICollection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        private static List<T> GetList<T>(ICollection<T> collection)
        {
            return collection as List<T>;
        }

        /// <summary>
        /// Converts enumerable to ObservableCollection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> coll)
        {
            var c = new ObservableCollection<T>();
            foreach (var e in coll)
            {
                c.Add(e);
            }
            return c;
        }

        /// <summary>
        /// Gets a byte array representing the xml file.
        /// </summary>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this XDocument xDocument)
        {
            byte[] byteArray = null;

            using (MemoryStream ms = new MemoryStream())
            {
                xDocument.Save(ms);
                byteArray = ms.ToArray();
            }

            return byteArray;
        }

        /// <summary>
        /// Serializes a class to its equivalent XDocument.
        /// </summary>
        /// <param name="serializableObject"></param>
        /// <returns></returns>
        public static XDocument Serialize<T>(this T serializableObject, bool useDefaultNamespace = true) where T : new()
        {
            if (!typeof(T).IsSerializable && !(typeof(System.Runtime.Serialization.ISerializable).IsAssignableFrom(typeof(T))))
            {
                throw new InvalidOperationException("A serializable Type is required");
            }

            const string XMLSCHEMAINSTANCE_ATB = "http://www.w3.org/2001/XMLSchema-instance";
            const string XMLSCHEMA_ATB = "http://www.w3.org/2001/XMLSchema";

            XDocument xDocument = new XDocument();

            using (System.Xml.XmlWriter writer = xDocument.CreateWriter())
            {
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                serializer.Serialize(writer, serializableObject);
            }

            if (!useDefaultNamespace)
            {
                //Removes unwanted namespaces that are added automatically by the serializer

                if (xDocument.Root.Attributes().Any(attrib => attrib.Value == XMLSCHEMAINSTANCE_ATB))
                {
                    xDocument.Root.Attributes().FirstOrDefault(attrib => attrib.Value == XMLSCHEMAINSTANCE_ATB).Remove();
                }

                if (xDocument.Root.Attributes().Any(attrib => attrib.Value == XMLSCHEMA_ATB))
                {
                    xDocument.Root.Attributes().FirstOrDefault(attrib => attrib.Value == XMLSCHEMA_ATB).Remove();
                }
            }

            return xDocument;
        }

        /// <summary>
        /// Deserializes a XML to its corresponding object.
        /// </summary>
        /// <param name="ObjectType"></param>
        /// <param name="xDocument"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this XDocument xDocument)
        {    
            //Checks if the XDocument is null and throws exception if yes.
            if(xDocument.IsNull())
            {
                throw new NullReferenceException("The XDocument cannot be null.");
            }

            T newObj = default(T);

            using (var reader = xDocument.CreateReader())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                newObj = (T)serializer.Deserialize(xDocument.CreateReader());
            }

            return newObj;
        }
        /// <summary>
        /// Deserializes a XML from a path to its corresponding object.
        /// </summary>
        /// <param name="ObjectType"></param>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this string xmlPath)
        {
            //Checks if the file is a xml file
            if(Path.GetExtension(xmlPath) == "xml")
            {
                throw new Exception(string.Format("The file at {0} is not a xml file.", xmlPath));
            }

            //Checks if the file exists in the path selected
            if (!File.Exists(xmlPath))
            {            
                throw new FileNotFoundException();
            }

            //Loads the XML and serializes it back to the caller.
            XDocument xDocument = XDocument.Load(xmlPath);

            T newObj = default(T);

            using (var reader = xDocument.CreateReader())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                newObj = (T)serializer.Deserialize(xDocument.CreateReader());
            }

            return newObj;
        }
     
        /// <summary>
        /// Deep copies an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToClone"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(this T objectToClone)
        {
            if (objectToClone.IsNull())
            {
                throw new NullReferenceException("Source cannot be null.");
            }

            Type objectType = typeof(T);

            T newObject = (T)Activator.CreateInstance(objectType);

            objectType.GetProperties().ForEach(prop =>
            {
                newObject.GetType().GetProperty(prop.Name).SetValue(newObject, prop.GetValue(objectToClone));
            });

            return newObject;
        }
    }
}
