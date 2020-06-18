using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReflectXMLDB.Test.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ReflectXMLDB.Test
{
    [TestClass]
    public class BasicTests
    {

        public BasicTests()
        {

        }

        [TestMethod]
        public void BasicTest()
        {
            try
            {
                //Initializes ReflectXMLDB database handling class.
                DatabaseHandler dh = new DatabaseHandler();

                //Creates the workspace.
                string workspace = Path.Combine(Directory.GetCurrentDirectory(), @"DBSample");
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
                var querySomeSamples = dh.Get<Sample>("SomeData", "Data5");

                //Removes some of the items in the database.
                dh.Remove<Sample>(querySomeSamples);

                //Exports the database to a .db file.
                dh.ExportDatabase(Path.Combine(Directory.GetCurrentDirectory(), @"Place"), "copyOfSampleDatabase1");

                //Deletes the database
                dh.DeleteDatabase<SampleDatabase>();

                //Deletes the workspace and clears the internal resources.
                dh.ClearHandler();
                Directory.Delete(workspace, true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Assert.Fail();
            }
        }
    }
}
