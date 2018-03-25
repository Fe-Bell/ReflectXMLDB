using ReflectXMLDB.Generic;
using ReflectXMLDB.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ReflectXMLDB.Model
{
    /// <summary>
    /// Base class that provides generic method for the ReflectXMLDB.
    /// </summary>
    public abstract class DatabaseHandlerBase
    {
        #region Public properties

        /// <summary>
        /// Returns the current workspace of the database.
        /// </summary>
        public string CurrentWorkspace { get; protected set; }
        /// <summary>
        /// Returns a collection of  databases initialized.
        /// </summary>
        public ICollection<Type> CurrentDatabases { get; protected set; }

        #endregion

        /// <summary>
        /// Gets the next non repeated GUID in a collection of collectables.
        /// </summary>
        /// <param name="identifiableObjects"></param>
        /// <returns></returns>
        protected string GetNextGUID<T>(ICollection<T> identifiableObjects = null) where T : IIdentifiableObject
        {
            string newGUID = Guid.NewGuid().ToString().ToUpper();

            if(identifiableObjects.IsNull() || !identifiableObjects.Any())
            {
                return newGUID;
            }
            else
            {
                if (identifiableObjects.Any(collectableObject => collectableObject.GUID == newGUID))
                {
                    return GetNextGUID(identifiableObjects);
                }
                else
                {
                    return newGUID;
                }
            }
        }
        /// <summary>
        /// Gets a the next available ID in a collection of collectables.
        /// </summary>
        /// <param name="collectableObjects"></param>
        /// <returns></returns>
        protected uint GetNextID<T>(ICollection<T> collectableObjects = null) where T : ICollectableObject
        {
            return GetNextID(collectableObjects, 0);
        }
        /// <summary>
        /// Gets a the next available ID in a collection of collectables.
        /// </summary>
        /// <param name="collectableObjects"></param>
        /// <returns></returns>
        protected uint GetNextID<T>(ICollection<T> collectableObjects, int startAt = 0) where T : ICollectableObject
        {
            if (collectableObjects.IsNull() || !collectableObjects.Any())
            {
                return 0;
            }
            else
            {
                IEnumerable<int> idList = collectableObjects.Select(collectableObject => (int)collectableObject.EID);

                return (uint)Enumerable.Range(startAt, Int32.MaxValue).Except(idList).First();
            }
        }
        /// <summary>
        /// Returns a properly enumerated collection of ICollectableObjects.
        /// </summary>
        /// <param name="collectableObjects"></param>
        /// <returns></returns>
        protected ICollection<T> EnumerateCollection<T>(ICollection<T> collectableObjects, uint startIndex = 0) where T : ICollectableObject
        {
            collectableObjects.ForEach(iCollectableObject =>
            {
                iCollectableObject.EID = startIndex;
                startIndex++;
            });

            return collectableObjects;
        }
    }
}
