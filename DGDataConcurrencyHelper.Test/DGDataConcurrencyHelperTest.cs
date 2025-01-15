using DG.DataConcurrencyHelper.Objects;
using NUnit.Framework;
using System.Collections.Generic;
using System.Configuration;
#if NETFRAMEWORK
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif
using System.IO;
using System.Linq;

namespace DG.DataConcurrencyHelper.Test
{
    [TestFixture]
    public class DGDataConcurrencyHelperTest
    {
        private string logUsername = "TestUser";
        private string application = "TestApp";
        private DGDataConcurrencyHelper dataConcurrencyHelper = null;

        public DGDataConcurrencyHelperTest()
        {
#if !NETFRAMEWORK
            File.Copy($"{System.Reflection.Assembly.GetExecutingAssembly().Location}.config", ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath, true);
#endif

            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

            dataConcurrencyHelper = new DGDataConcurrencyHelper(ConfigurationManager.AppSettings["dgdataconcurrencyhelperConnectionString"]);

            CleanTestData();
        }

        private void CleanTestData()
        {
            SqlConnection sqlConnection = new SqlConnection();
            sqlConnection.ConnectionString = ConfigurationManager.AppSettings["dgdataconcurrencyhelperConnectionString"];
            sqlConnection.Open();

            new SqlCommand(@"DELETE FROM dch_concurrencyrecords WHERE concurrencyrecords_logusername = '" + logUsername + "'", sqlConnection).ExecuteNonQuery();
        }

        [Test]
        public void SetStats1()
        {
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "1", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "1", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(false));
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "2", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));
        }

        [Test]
        public void GetStatus1()
        {
            Assert.That(dataConcurrencyHelper.GetStatus("DB1", "Table1", "3"), Is.EqualTo(null));
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "3", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));
            Assert.That(dataConcurrencyHelper.GetStatus("DB1", "Table1", "3"), Is.EqualTo(DGDataConcurrencyHelper.Status.Editing));
        }

        [Test]
        public void ResetStatus1()
        {
            Assert.That(dataConcurrencyHelper.ResetStatus("DB1", "Table1", "4"), Is.EqualTo(false));
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "4", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));
            Assert.That(dataConcurrencyHelper.ResetStatus("DB1", "Table1", "4"), Is.EqualTo(true));
        }

        [Test]
        public void List1()
        {
            List<ConcurrencyRecord> connectionsStatus = dataConcurrencyHelper.List().ToList();
            foreach (ConcurrencyRecord c in connectionsStatus.Where(r => r.Logusername == logUsername))
                Assert.That(dataConcurrencyHelper.Remove(c.Id), Is.EqualTo(true));

            connectionsStatus = dataConcurrencyHelper.List().ToList();
            Assert.That(connectionsStatus.Where(r => r.Logusername == logUsername).Count(), Is.EqualTo(0));

            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "7", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "8", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));

            connectionsStatus = dataConcurrencyHelper.List().ToList();
            Assert.That(connectionsStatus.Where(r => r.Logusername == logUsername).Count(), Is.EqualTo(2));
        }

        [Test]
        public void Remove1()
        {
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "5", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "6", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));

            List<ConcurrencyRecord> connectionsStatus = dataConcurrencyHelper.List().ToList();
            Assert.That(connectionsStatus.Where(r => r.Logusername == logUsername).Count(), Is.AtLeast(1));

            int lastIdremoved = -1;
            foreach (ConcurrencyRecord c in connectionsStatus.Where(r => r.Logusername == logUsername))
            {
                lastIdremoved = c.Id;
                Assert.That(dataConcurrencyHelper.Remove(c.Id), Is.EqualTo(true));
            }

            connectionsStatus = dataConcurrencyHelper.List().ToList();
            Assert.That(connectionsStatus.Where(r => r.Logusername == logUsername).Count(), Is.EqualTo(0));

            Assert.That(dataConcurrencyHelper.Remove(lastIdremoved), Is.EqualTo(false));
        }

        [Test]
        public void Purge1()
        {
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "9", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));

            Assert.That(dataConcurrencyHelper.PurgeConnectionsStatus(-1), Is.AtLeast(1));

            Assert.That(dataConcurrencyHelper.PurgeConnectionsStatus(100), Is.EqualTo(0));
        }

        [Test]
        public void Find1()
        {
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "10", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));

            List<ConcurrencyRecord> connectionsStatus = dataConcurrencyHelper.List().ToList();
            Assert.That(connectionsStatus.Where(r => r.Logusername == logUsername).Count(), Is.AtLeast(1));

            foreach (ConcurrencyRecord c in connectionsStatus.Where(r => r.Logusername == logUsername))
            {
                Assert.That(dataConcurrencyHelper.Find(c.Id), Is.Not.EqualTo(null));
            }
        }

        [Test]
        public void Find2()
        {
            Assert.That(dataConcurrencyHelper.SetStatus("DB1", "Table1", "11", application, logUsername, DGDataConcurrencyHelper.Status.Editing), Is.EqualTo(true));

            Assert.That(dataConcurrencyHelper.Find("DB1", "Table1", "11"), Is.Not.EqualTo(null));
            Assert.That(dataConcurrencyHelper.Find("DB1", "Table1xx", "1"), Is.EqualTo(null));
        }

        [Test]
        public void KeyPairsDictionaryToJson1()
        {
            Dictionary<string, object> dictionary = null;
            string actual = null;
            string expected = null;

            dictionary = new Dictionary<string, object>() {
                { "uno", "1" }
            };
            actual = DGDataConcurrencyHelper.KeyPairsDictionaryToJson(dictionary);
            expected = "{ \"uno\": 1 }";
            Assert.That(actual, Is.EqualTo(expected));

            dictionary = new Dictionary<string, object>() {
                { "uno", "1" },
                { "due", "aaa" }
            };
            actual = DGDataConcurrencyHelper.KeyPairsDictionaryToJson(dictionary);
            expected = "{ \"uno\": 1, \"due\": \"aaa\" }";
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
