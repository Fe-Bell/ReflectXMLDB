using ReflectXMLDB.Generic;
using ReflectXMLDB.Interface;
using ReflectXMLDB.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace ReflectXMLDB
{
    /// <summary>
    /// Provides resources for manipulating XML-based databases.
    /// </summary>
    public class DatabaseHandler : DatabaseHandlerBase
    {
        #region Private fields

        /// <summary>
        /// Watches the database's current folder.
        /// </summary>
        private FileSystemWatcher fileSystemWatcher = null;
        /// <summary>
        /// Stores the paths for each initialized XML.
        /// </summary>
        private IEnumerable<string> paths = null;
        /// <summary>
        /// Stores the name of the last modifiled file in the database folder.
        /// </summary>
        private string lastFileChanged = string.Empty;
        /// <summary>
        /// Object used for cross-thread protection.
        /// </summary>
        private static readonly object lockObject = new object();

        #endregion

        #region Public properties

        

        #endregion

        #region Public events

        /// <summary>
        /// Event fired when there is a change in one of the database files.
        /// </summary>
        public event OnDatabaseChangedEventHandler OnDatabaseChanged = null;
        /// <summary>
        /// Delegate event handler for the OnDatabaseChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void OnDatabaseChangedEventHandler(object sender, OnDatabaseChangedEventArgs e);
        /// <summary>
        /// EventArgs delivered with OnDatabaseChanged event.
        /// </summary>
        public class OnDatabaseChangedEventArgs : EventArgs
        {
            public string DatabaseName { get; private set; }
            public DateTime Time { get; private set; }

            public OnDatabaseChangedEventArgs(string databaseName, DateTime time)
            {
                DatabaseName = databaseName;
                Time = time;
            }
        }

        /// <summary>
        /// EventArgs delivered with OnDatabaseImported event.
        /// </summary>
        public class OnDatabaseImportedEventArgs
        {
            public string DatabaseName { get; private set; }
            public DateTime Time { get; private set; }

            public OnDatabaseImportedEventArgs(string databaseName, DateTime date)
            {
                DatabaseName = databaseName;
                Time = Time;
            }
        }
        /// <summary>
        /// Delegate event handler for the OnDatabaseImported event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void OnDatabaseImportedEventHandler(object sender, OnDatabaseImportedEventArgs e);
        /// <summary>
        /// Event fired when a database import is completed.
        /// </summary>
        public OnDatabaseImportedEventHandler OnDatabaseImported = null;

        /// <summary>
        /// EventArgs delivered with OnDatabaseExported event.
        /// </summary>
        public class OnDatabaseExportedEventArgs
        {
            public string DatabaseName { get; private set; }
            public DateTime Time { get; private set; }

            public OnDatabaseExportedEventArgs(string databaseName, DateTime date)
            {
                DatabaseName = databaseName;
                Time = Time;
            }
        }
        /// <summary>
        /// Delegate event handler for the OnDatabaseExported event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void OnDatabaseExportedEventHandler(object sender, OnDatabaseExportedEventArgs e);
        /// <summary>
        /// Event fired when a database export is completed.
        /// </summary>
        public OnDatabaseExportedEventHandler OnDatabaseExported = null;

        #endregion

        #region Private methods

        /// <summary>
        /// Handles file changed events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!lastFileChanged.Equals(e.Name))
            {
                string fileName = e.Name;

                if (fileName.Contains(".xml"))
                {
                    fileName = fileName.Replace(".xml", "");

                    if (fileName.Contains("Database"))
                    {
                        OnDatabaseChanged?.Invoke(this, new OnDatabaseChangedEventArgs(fileName, DateTime.Now));
                    }
                }

                lastFileChanged = e.Name;
            }
            else
            {
                lastFileChanged = string.Empty;
            }

            Console.WriteLine("Changed " + e.FullPath);
        }
        /// <summary>
        /// Handles file created events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Created " + e.FullPath);
        }
        /// <summary>
        /// Handles file deleted events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine("Deleted " + e.FullPath);
        }
        /// <summary>
        /// Handles file renamed events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("Renamed " + e.FullPath);
        }
        /// <summary>
        /// Gets the database information struct for an object of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private DatabaseItemInfo<T> GetDatabaseItemInfo<T>() where T : ICollectableObject
        {
            return new DatabaseItemInfo<T>();
        }
        /// <summary>
        /// Gets the database path of a database.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetDatabasePath(Type type)
        {
            FieldInfo dbPathField = GetType().GetField("paths", BindingFlags.NonPublic | BindingFlags.Instance);
            if (!dbPathField.IsNull())
            {
                var paths = dbPathField.GetValue(this);
                return (paths as IEnumerable<string>).FirstOrDefault(path => path.Contains(type.Name));
            }
            else
            {
                throw new Exception("Could not find any paths.");
            }
        }
        /// <summary>
        /// Monitors a specified directory. This creates a filewatcher.
        /// </summary>
        /// <param name="path"></param>
        private void StartMonitoringDirectory(string path)
        {
            fileSystemWatcher = new FileSystemWatcher
            {
                Path = path
            };
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
            fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;

            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileSystemWatcher.EnableRaisingEvents = true;
        }
        /// <summary>
        /// Stops monitoring a folder. This disposes the Filewatcher and its events.
        /// </summary>
        private void StopMonitoringDirectory()
        {
            if (!fileSystemWatcher.IsNull())
            {
                fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
                fileSystemWatcher.Created -= FileSystemWatcher_Created;
                fileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;
                fileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;

                fileSystemWatcher.Dispose();
                fileSystemWatcher = null;
            }
        }
        /// <summary>
        /// Gets a database that inherits from IDatabase
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetDatabase<T>() where T : IDatabase
        {
            string path = GetDatabasePath(typeof(T));

            if (!string.IsNullOrEmpty(path))
            {
                T database = default(T);

                lock(lockObject)
                {
                    database = path.Deserialize<T>();
                }

                return database;
            }
            else
            {
                return default(T);
            }
        }
        /// <summary>
        /// Gets a selection of items that inherit from ICollectable object from their database collection.
        /// If propertyName and propertyValue are null, then returns all items of the selected type T in the database.
        /// If propertyName and propertyValue are NOT null, then all items matching the query (propertyNames that have propertyValues as their value).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private ICollection<T> Get<T>(string propertyName = null, object propertyValue = null, bool dummyParameter = false) where T : ICollectableObject
        {
            if (string.IsNullOrEmpty(propertyName) && !propertyValue.IsNull())
            {
                throw new Exception("If propertyName is null, then propertyValue must also be null.");
            }
          
            bool getAllItems = string.IsNullOrEmpty(propertyName) && propertyValue.IsNull();

            DatabaseItemInfo<T> dbInfo = GetDatabaseItemInfo<T>();

            var db = GetType().GetMethod("GetDatabase", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(dbInfo.DatabaseType).Invoke(this, null);
            var dbCollection = db.GetType().GetProperty(dbInfo.CollectionName).GetValue(db) as ICollection<T>;

            ICollection<T> items = null;

            if (getAllItems)
            {
                items = dbCollection;
            }
            else
            {
                List<T> col = new List<T>();
                foreach (var item in dbCollection)
                {
                    var prop = item.GetType().GetProperty(propertyName);
                    if (!prop.IsNull())
                    {
                        var propValue = prop.GetValue(item);
                        if (!propValue.IsNull())
                        {
                            if (prop.GetValue(item).ToString() == (propertyValue ?? "").ToString())
                            {
                                col.Add(item);
                            }
                        }
                    }
                }
                items = col.Any() ? col : null;
            }

            //A collection or null should be returned.
            return items;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Clears the current database handler. This causes the all internal properties to be null and the workspace to be deleted.
        /// </summary>
        public void ClearHandler()
        {
            StopMonitoringDirectory();

            if (Directory.Exists(CurrentWorkspace))
            {
                Directory.Delete(CurrentWorkspace, true);
            }

            CurrentWorkspace = null;
            CurrentDatabases = null;
            paths = null;
            lastFileChanged = null;

            if (!OnDatabaseChanged.IsNull())
            {
                OnDatabaseChanged = null;
            }
            if (!OnDatabaseExported.IsNull())
            {
                OnDatabaseExported = null;
            }
            if (!OnDatabaseImported.IsNull())
            {
                OnDatabaseImported = null;
            }
        }
        /// <summary>
        /// Creates a database file of a type T that inherits from IDatabase.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void CreateDatabase<T>() where T : IDatabase
        {
            Type objectType = typeof(T);
            string path = GetDatabasePath(objectType);

            var db = Activator.CreateInstance(objectType);
            ((IDatabase)db).GUID = GetNextGUID<T>();
            var xml = db.Serialize();

            lock(lockObject)
            {
                xml.Save(path);
            }
        }
        /// <summary>
        /// Deletes a database file from the computer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void DeleteDatabase<T>() where T : IDatabase
        {
            Type objectType = typeof(T);
            string path = GetDatabasePath(objectType);

            if (File.Exists(path))
            {
                lock(lockObject)
                {
                    File.Delete(path);
                }
            }
        }       
        /// <summary>
        /// Gets a selection of items that inherit from ICollectable object from their database collection.
        /// Returns all items of the selected type T in the database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ICollection<T> Get<T>() where T : ICollectableObject
        {
            return Get<T>(null, null);
        }
        /// <summary>
        /// Gets a selection of items that inherit from ICollectable object from their database collection.
        /// If propertyName and propertyValue are null, then returns all items of the selected type T in the database.
        /// If propertyName and propertyValue are NOT null, then all items matching the query (propertyNames that have propertyValues as their value).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public ICollection<T> Get<T>(string propertyName, object propertyValue) where T : ICollectableObject
        {
            return Get<T>(propertyName, propertyValue, false);
        }
        /// <summary>
        /// Inserts a selection of items that inherit from ICollectable object in their database collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public void Insert<T>(ICollection<T> items) where T : ICollectableObject
        {
            if (items.IsNull())
            {
                throw new ArgumentNullException("The parameter items cannot be null.");
            }

            DatabaseItemInfo<T> dbInfo = GetDatabaseItemInfo<T>();

            var db = GetType().GetMethod("GetDatabase", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(dbInfo.DatabaseType).Invoke(this, null);
            var dbCollection = db.GetType().GetProperty(dbInfo.CollectionName).GetValue(db) as ICollection<T>;

            List<T> newCollection = dbCollection.IsNull() ? new List<T>() : dbCollection.ToList();
    
            items.ForEach(item =>
            {
                item.EID = GetNextID(dbCollection);
                item.GUID = GetNextGUID(dbCollection);
                newCollection.Add(item);
            });

            Save<T>(newCollection);
        }
        /// <summary>
        /// Removes a selection of items that inherit from ICollectable object from their database collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public void Remove<T>(ICollection<T> items) where T : ICollectableObject
        {
            if (items.IsNull())
            {
                throw new ArgumentNullException("The parameter items cannot be null.");
            }

            DatabaseItemInfo<T> dbInfo = GetDatabaseItemInfo<T>();

            var db = GetType().GetMethod("GetDatabase", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(dbInfo.DatabaseType).Invoke(this, null);
            var dbCollection = db.GetType().GetProperty(dbInfo.CollectionName).GetValue(db) as ICollection<T>;

            List<T> newCollection = dbCollection.IsNull() ? new List<T>() : dbCollection.ToList();
            items.ForEach(item => newCollection.RemoveAll(i => ((ICollectableObject)i).GUID == item.GUID));

            Save<T>(newCollection);
        }
        /// <summary>
        /// Saves a selection of items in its respective database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public void Save<T>(ICollection<T> items) where T : ICollectableObject
        {
            if (items.IsNull())
            {
                throw new ArgumentNullException("The parameter items cannot be null.");
            }

            DatabaseItemInfo<T> dbInfo = GetDatabaseItemInfo<T>();

            var db = GetType().GetMethod("GetDatabase", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(dbInfo.DatabaseType).Invoke(this, null);

            Type typeOfCollection = db.GetType().GetProperty(dbInfo.CollectionName).PropertyType;
            object newItems = EnumerateCollection<T>(items);

            if (typeOfCollection.BaseType == typeof(Array))
            {
                newItems = (newItems as ICollection<T>).ToArray();
            }
            else if (typeOfCollection.BaseType == typeof(Collection<T>))
            {
                newItems = (newItems as ICollection<T>).ToObservableCollection();
            }

            db.GetType().GetProperty(dbInfo.CollectionName).SetValue(db, newItems);

            string path = GetDatabasePath(dbInfo.DatabaseType);

            lock (lockObject)
            {
                db.Serialize().Save(path);
            }
        }
        /// <summary>
        /// Sets the workspace of this class. Type parameters must all inherit from IDatabase.
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="databaseTypes"></param>
        public void SetWorkspace(string workspace, ICollection<Type> databaseTypes)
        {
            //Checks parameters
            //---------------------------------------------------------------------------
            if (string.IsNullOrEmpty(workspace))
            {
                throw new Exception("Cannot have a null workspace.");
            }

            databaseTypes.ForEach(item =>
            {
                if (!typeof(IDatabase).GetTypeInfo().IsAssignableFrom(item))
                {
                    throw new Exception("All types must inherit from IDatabase.");
                }
            });
            //---------------------------------------------------------------------------

            //Checks if the workspace path ends with a slash and if not, adds it.
            if(!workspace.Last().Equals('\\'))
            {
                workspace += '\\';
            }
            
            //Saves the database tyoes and the workspace in the current instance.
            CurrentDatabases = databaseTypes;
            CurrentWorkspace = workspace;

            //Stop monitoring the current workspace
            StopMonitoringDirectory();

            //Creates the workspace folder if it does not exist.
            if (!Directory.Exists(workspace))
            {
                Directory.CreateDirectory(workspace);
            }

            //Start monitoring the current workspace
            StartMonitoringDirectory(workspace);

            //Creates the paths for each database type.
            List<string> dbNames = new List<string>();
            databaseTypes.ForEach(item =>
            {
                if (typeof(ICollectableObject).GetTypeInfo().IsAssignableFrom(item))
                {
                    var info = GetType().GetMethod("GetDatabaseItemInfo", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(item).Invoke(this, null);
                    var prop = info.GetType().GetProperty("DatabaseType");
                    if (!prop.IsNull())
                    {
                        var propValue = prop.GetValue(info);
                        if (propValue is Type)
                        {
                            dbNames.Add((propValue as Type).Name);
                        }
                    }
                }
                else if (typeof(IDatabase).GetTypeInfo().IsAssignableFrom(item))
                {
                    dbNames.Add(item.Name);
                }
                else
                {
                    throw new Exception("The type " + item.FullName + " does not implement IDatabase and neither ICollectableObject");
                }
            });

            paths = dbNames.Select(name => workspace + name + ".xml");
        }
        /// <summary>
        /// Imports a zipped database file.
        /// </summary>
        /// <param name="fileToImport"></param>
        /// <param name="exportPath"></param>
        public void ImportDatabase(string fileToImport, string exportPath)
        {
            //This stores the path where the file should be unzipped to,
            //including any subfolders that the file was originally in.
            string fileUnzipFullPath = string.Empty;

            //This is the full name of the destination file including
            //the path
            string fileUnzipFullName = string.Empty;

            lock(lockObject)
            {
                //Opens the zip file up to be read
                using (ZipArchive archive = ZipFile.OpenRead(fileToImport))
                {
                    //Loops through each file in the zip file
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        //Identifies the destination file name and path
                        fileUnzipFullName = Path.Combine(exportPath, file.FullName);

                        //Extracts the files to the output folder in a safer manner
                        if (File.Exists(fileUnzipFullName))
                        {
                            File.Delete(fileUnzipFullName);
                        }

                        //Calculates what the new full path for the unzipped file should be
                        fileUnzipFullPath = Path.GetDirectoryName(fileUnzipFullName);

                        //Creates the directory (if it doesn't exist) for the new path
                        Directory.CreateDirectory(fileUnzipFullPath);

                        //Extracts the file to (potentially new) path
                        file.ExtractToFile(fileUnzipFullName);
                    }
                }
            }

            OnDatabaseImported?.Invoke(this, new OnDatabaseImportedEventArgs(fileUnzipFullName, DateTime.Now));
        }
        /// <summary>
        /// Exports a group of database files to a single zipped file.
        /// </summary>
        /// <param name="filename"></param>
        public void ExportDatabase(string pathToSave, string filename, string fileExtension = ".db")
        {
            //Checks if the path to save ends with a slash and if not, adds it.
            if (!pathToSave.Last().Equals('\\'))
            {
                pathToSave += '\\';
            }

            if(!Directory.Exists(pathToSave))
            {
                Directory.CreateDirectory(pathToSave);
            }

            string fullFilePath = Path.Combine(pathToSave, filename + fileExtension);

            lock(lockObject)
            {
                ZipFile.CreateFromDirectory(CurrentWorkspace, fullFilePath);
            }

            OnDatabaseExported?.Invoke(this, new OnDatabaseExportedEventArgs(fullFilePath, DateTime.Now));
        }
        /// <summary>
        /// Updates a selection of items in its respective database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        public void Update<T>(ICollection<T> items) where T : ICollectableObject
        {
            if (items.IsNull())
            {
                throw new ArgumentNullException("The parameter items cannot be null.");
            }

            DatabaseItemInfo<T> dbInfo = GetDatabaseItemInfo<T>();

            var db = GetType().GetMethod("GetDatabase", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(dbInfo.DatabaseType).Invoke(this, null);
            var dbCollection = db.GetType().GetProperty(dbInfo.CollectionName).GetValue(db) as ICollection<T>;

            List<T> newCollection = dbCollection.IsNull() ? new List<T>() : dbCollection.ToList();       
            items.ForEach(item => newCollection.RemoveAll(dbItem => dbItem.GUID == item.GUID));
            items.ForEach(item => newCollection.Add(item));

            Save<T>(newCollection);
        }

        #endregion
    }
}
