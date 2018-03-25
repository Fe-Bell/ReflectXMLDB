using ReflectXMLDB.Generic;
using ReflectXMLDB.Interface;
using System;
using System.Collections;
using System.Linq;

namespace ReflectXMLDB.Model
{
    /// <summary>
    /// Returns the Database type, Item type and the Collection name for an ICollectableObject.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DatabaseItemInfo<T> where T : ICollectableObject
    {
        /// <summary>
        /// Returns the type of T.
        /// </summary>
        public Type ItemType { get; private set; }
        /// <summary>
        /// Returns the database type for the type of T.
        /// </summary>
        public Type DatabaseType { get; private set; }
        /// <summary>
        /// Returns the collection name for the type of T. 
        /// </summary>
        public string CollectionName { get; private set; }

        /// <summary>
        /// Constructor. Initializes DatabaseType, ItemType and the CollectionName properties.
        /// </summary>
        public DatabaseItemInfo()
        {    
            //Takes the item type from T
            ItemType = typeof(T);
            if (ItemType.IsNull())
            {
                throw new Exception(string.Format("Could not resolve the collection item type."));
            }

            //Resolves the database type by searching the source assembly
            string dbName = string.Format("{0}.{1}Database", ItemType.Namespace, ItemType.Name);
            string dbQualifiedName = ItemType.AssemblyQualifiedName.Replace(ItemType.FullName, dbName);
            DatabaseType = Type.GetType(dbQualifiedName);
            if(DatabaseType.IsNull())
            {
                throw new Exception(string.Format("Could not find an IDatabase implementation with name {0}.", dbName));
            }

            //Looks for an ICollection in the specified database type. The first result is selected.
            //To be improved, needs to search for an ICollection of the specified item type.
            var collectionProp = DatabaseType.GetProperties().FirstOrDefault(prop => typeof(ICollection).IsAssignableFrom(prop.PropertyType));
            CollectionName = collectionProp.IsNull() ? null : collectionProp.Name;
            if (CollectionName.IsNull())
            {
                throw new Exception(string.Format("Could not find a valid ICollection for the objects of type {0}.", ItemType.Name));
            }
        }
    }
}
