<p align="center">
  <img src="rxdblogo.png">
</p>

# ReflectXMLDB
Database framework based on object serialization to XML.

This is a project that allows C#/.NET developers to implement simple XML database functionality in their applications. The project is written for [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) 2.0, which is compatible with .NET Core 2.0+ and .NET Framework 4.6.1+.

ReflectXMLDB is cross platform and can run in Windows, Linux and MacOS operating systems that support the .Net Core initiative.

# Features
- Cross everything. ReflectXMLDB is thread and multi-process protected. Also it can run on Linux, MacOS and Windows.
- Work with friendly XML database format that can be read by any regular text editor.
- Use simple methods to write/read data to the databases. Forget complex query actions and focus on your actual code.
- It is completely open source!

More to come!

# Get started
Download ReflectXMLDB from our [nuget](https://www.nuget.org/packages/ReflectXMLDB/) links.

ReflectXMLDB offers the traditional Get, Insert, Remove and Update item(s) capabilities commonly present in other database frameworks. Also, it provides a more low level interface with the database files generated, such as the ability to Export and Import the database files to .db files, which are compressed versions of the workspace.

There are a few rules that need to be understood when using ReflectXMLDB.
1. ReflectXMLDB creates a database XML file for each database initialized and/or instantiated.
2. Every database class must inherit from ReflectXMLDB.Interface.IDatabase and end its name with "Database". Also must provide a single ICollection (such as Arrays, Lists, ObservableCollections, etc.) of the item that it will store.
3. Every database object must inherit from ReflectXMLDB.Interface.ICollectableObject.
4. Database classes and objects must be in the same namespace.
5. That is it!

Example:
```csharp

    //Initializes ReflectXMLDB database handling class.
    DatabaseHandler dh = new DatabaseHandler();

    //Creates the workspace.
    string workspace = "\\DBSample";
    Type[] databaseTypes = new Type[] { typeof(SampleDatabase) };
    dh.SetWorkspace(workspace, databaseTypes);
    
    //Creates a database file.
    //SampleDatabase inherits from IDatabase.
    dh.CreateDatabase<SampleDatabase>();

    //Creates a list of objects to be inserted.
    //Sample inherits from ICollectableObject and is in the same namespace of SampleDatabase
    List<Sample> samples = new List<Sample>();
    for (int i = 0; i < 10; i++)
    {
        samples.Add(new Sample() { SomeData = string.Format("Data{0}", i) });
    }

    //Inserts the items in the database.
    dh.Insert(samples);

    //Gets all samples in the database.
    var queryAllSamples = dh.Get<Sample>();
    //Gets all samples that have Data5 as the value of the SomeData property.
    var querySomeSamples = dh.Get<Sample>("SomeData","Data5");

    //Removes some of the items in the database.
    dh.Remove<Sample>(querySomeSamples);

    //Exports the database to a .db file.
    dh.ExportDatabase("\\Place\\", "copyOfSampleDatabase1");

    //Deletes the database
    dh.DeleteDatabase<SampleDatabase>();

    //Deletes the workspace and clears the internal resources.
    dh.ClearHandler();
```
Where Sample is:
```csharp
    public class Sample : ICollectableObject
    {
        [XmlAttribute]
        public string GUID { get; set; }
        [XmlAttribute]
        public uint EID { get; set; }
        public string SomeData { get; set; }
    }
```
And SampleDatabase is:
```csharp
    public class SampleDatabase : IDatabase
    {
        [XmlAttribute]
        public string GUID { get; set; }
        public Sample[] Samples { get; set; }
    }
```

Happy coding!

# License
Licensed under [MIT License](https://github.com/Fe-Bell/ReflectXMLDB/blob/master/LICENSE).
