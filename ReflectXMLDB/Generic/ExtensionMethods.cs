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
            List<T> list = collection as List<T>;

            if (list != null)
            {
                list.RemoveAll(new Predicate<T>(predicate));
            }
            else
            {
                List<T> itemsToDelete = collection
                    .Where(predicate)
                    .ToList();

                foreach (var item in itemsToDelete)
                {
                    collection.Remove(item);
                }
            }
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
        public static XDocument Serialize(this object serializableObject, bool useDefaultNamespace = false)
        {
            const string XMLSCHEMAINSTANCE_ATB = "http://www.w3.org/2001/XMLSchema-instance";
            const string XMLSCHEMA_ATB = "http://www.w3.org/2001/XMLSchema";

            XDocument xDocument = null;

            try
            {
                xDocument = new XDocument();

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
            }
            catch (Exception)
            {
                throw;
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
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(xDocument.CreateReader());
        }
        /// <summary>
        /// Deserializes a XML from a path to its corresponding object.
        /// </summary>
        /// <param name="ObjectType"></param>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this string xmlPath)
        {
            if (File.Exists(xmlPath))
            {
                XDocument xDocument = null;

                try
                {
                    xDocument = XDocument.Load(xmlPath);

                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    return (T)serializer.Deserialize(xDocument.CreateReader());
                }
                catch (Exception)
                {
                    throw new Exception(string.Format("The file at {0} is not a valid xml file.", xmlPath));
                }
            }
            else
            {
                throw new FileNotFoundException();
            }
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
