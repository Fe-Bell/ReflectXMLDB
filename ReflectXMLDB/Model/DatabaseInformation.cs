namespace ReflectXMLDB.Model
{
    /// <summary>
    /// Returns database information.
    /// </summary>
    public class DatabaseInformation<T>
    {
        /// <summary>
        /// Returns the database qualified name.
        /// </summary>
        public string DatabaseQualifiedName { get; private set; }

        public DatabaseInformation()
        {
            DatabaseQualifiedName = typeof(T).AssemblyQualifiedName;
        }
    }
}
