// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="TestMemoryOnlyKey.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------

#region Namespaces

using Android.App;
using O8.Mobile.Droid.Vault.Keys.Generator;
using O8.Mobile.Droid.Vault.Keys.Storage;
using NUnit.Framework;

#endregion

namespace O8.Mobile.Droid.Vault.Tests
{
    /// <summary>
    ///     Test usage with a key that is only stored in memory.
    /// </summary>
    [TestFixture]
    public class TestMemoryOnlyKey
    {
        private const string PrefFileName = "memoryOnlyPrefFile";

        private const string TestStringKey = "testKey";
        private const string TestStringValue = " This is a test. ";

        [Test]
        public void TestVaultRetention()
        {
            var context = Application.Context;
            var sharedPreferenceVault1 = SharedPreferenceVaultFactory.GetMemoryOnlyKeyAes256Vault(context, PrefFileName, false);

            Assert.That(sharedPreferenceVault1, Is.Not.Null, "Unable to create initial vault");
            Assert.That(sharedPreferenceVault1.IsKeyAvailable, Is.False, "Key was present before setting it");
            Assert.That(sharedPreferenceVault1.GetString(TestStringKey, null), Is.Null, "Reading data without setting key worked");

            //Set a new random key
            var testKey1 = Aes256RandomKeyFactory.CreateKey();
            sharedPreferenceVault1.SetKey(testKey1);

            Assert.That(sharedPreferenceVault1.IsKeyAvailable, Is.True, "Key was not present after setting it");
            Assert.That(sharedPreferenceVault1.GetString(TestStringKey, null), Is.Null, "Rekey of storage did not clear existing value");
            Assert.That(sharedPreferenceVault1.KeyStorageType, Is.EqualTo(KeyStorageType.NotPersistent), "Wrong type of storage");

            //Store some data and verify it.
            sharedPreferenceVault1.Edit().PutString(TestStringKey, TestStringValue).Apply();
            Assert.That(sharedPreferenceVault1.GetString(TestStringKey, null), Is.EqualTo(TestStringValue), "Storage in initial vault did not work properly");

            //Create a secondary instance of the sharedPreferenceVault to ensure in-memory key is not shared implicitly.
            var sharedPreferenceVault2 = SharedPreferenceVaultFactory.GetMemoryOnlyKeyAes256Vault(context, PrefFileName, false);
            Assert.That(sharedPreferenceVault2, Is.Not.Null, "Unable to create second instance of vault");
            Assert.That(sharedPreferenceVault2.GetString(TestStringKey, null), Is.Null, "Retrieval in second vault worked without key.");

            //Apply key and test again
            sharedPreferenceVault2.SetKey(testKey1);
            Assert.That(
                sharedPreferenceVault2.GetString(TestStringKey, null), Is.EqualTo(TestStringValue),
                "Retrieval in second instance of vault did not work properly");

            //Clear key and verify failure
            sharedPreferenceVault2.SetKey(null);
            Assert.That(sharedPreferenceVault2.IsKeyAvailable, Is.False, "Key was not cleared");
            Assert.That(sharedPreferenceVault2.GetString(TestStringKey, null), Is.Null, "Retrieval in second vault still worked after clearing key");

            //Test incorrect key
            sharedPreferenceVault2.SetKey(Aes256RandomKeyFactory.CreateKey());
            Assert.That(sharedPreferenceVault2.IsKeyAvailable, Is.True, "Rekey did not work");
            Assert.That(sharedPreferenceVault2.GetString(TestStringKey, null), Is.Null, "Retrieval in second vault still worked after clearing key");

            //Test data clearing in initial vault
            sharedPreferenceVault1.ClearStorage();
            Assert.That(sharedPreferenceVault1.IsKeyAvailable, Is.False, "Key was not removed after clearing storage.");
            sharedPreferenceVault1.RekeyStorage(testKey1);
            Assert.That(sharedPreferenceVault1.GetString(TestStringKey, null), Is.Null, "Clear storage failed to delete data");
        }
    }
}