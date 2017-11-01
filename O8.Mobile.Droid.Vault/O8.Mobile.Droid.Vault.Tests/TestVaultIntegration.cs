// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="TestVaultIntegration.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------

#region Namespaces

using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using O8.Mobile.Droid.Vault.Keys.Generator;
using O8.Mobile.Droid.Vault.Keys.Storage;
using NUnit.Framework;

#endregion

namespace O8.Mobile.Droid.Vault.Tests
{
    /// <summary>
    ///     Integration test of normal operation.
    /// </summary>
    [TestFixture]
    public class TestVaultIntegration
    {
        private const string KeyFileName = "integrationKeyFile";

        private const string PrefFileName = "integrationPrefFile";
        private const string KeyAlias1 = "integrationKeyAlias";
        private const int KeyIndex1 = 1;
        private const string PresharedSecret1 = "a;sdlfkja;5585585;shdluifhe;l2ihjl9jl9dj9";

        private const string TestStringKey = "testKey";
        private const string TestStringValue = " This is a test. ";
        private const string TestBooleanKey = "testBooleanKey";
        private const bool TestBooleanValue = true;
        private const string TestIntKey = "testIntegerKey";
        private const int TestIntValue = -230;
        private const string TestLongKey = "testLongKey";
        private const long TestLongValue = long.MaxValue;
        private const string TestFloatKey = "testFloatKey";
        private const float TestFloatValue = -2.3f;
        private const string TestStringSetKey = "testStringSetKey";
        private static readonly ICollection<string> TestStringSetValue = new[] { "Test String One", "Test String Two", "Test String Three", "Test String Four" };
        private const int LargeStringSize = 8192;
        private const string TestLargeStringKey = "testLongStringKey";

        private string CreateRandomString(int size)
        {
            var stringBuilder = new StringBuilder();
            const string validCharacters = "0123456789abcdefghijlmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWYXZ\n\t ";
            var validCharacterLength = validCharacters.Length;

            var random = new Random();
            for (var i = 0; i < size; i++)
            {
                stringBuilder.Append(validCharacters[random.Next(validCharacterLength)]);
            }

            return stringBuilder.ToString();
        }


        [Test]
        public void TestVaultRetention()
        {
            var context = Application.Context;
            var sharedPreferenceVault1 = SharedPreferenceVaultFactory.GetAppKeyedCompatAes256Vault(
                context, PrefFileName, KeyFileName, KeyAlias1, KeyIndex1, PresharedSecret1);

            Assert.That(sharedPreferenceVault1, Is.Not.Null, "Unable to create initial vault");
            Assert.That(
                sharedPreferenceVault1.KeyStorageType, Is.EqualTo(KeyStorageType.AndroidKeystore),
                "This test must be run on a device running Android API 18 or later, with working Android Keystore support");

            //Ensure no leftover data is restored
            sharedPreferenceVault1.RekeyStorage(Aes256RandomKeyFactory.CreateKey());
            Assert.That(sharedPreferenceVault1.GetString(TestStringKey, null), Is.Null, "Rekey of storage did not clear existing value");

            //Store some data and verify it.
            sharedPreferenceVault1.Edit().PutString(TestStringKey, TestStringValue).Apply();
            Assert.That(sharedPreferenceVault1.GetString(TestStringKey, null), Is.EqualTo(TestStringValue), "Storage in initial vault did not work properly");

            sharedPreferenceVault1.Edit().PutBoolean(TestBooleanKey, TestBooleanValue).Apply();
            Assert.That(
                sharedPreferenceVault1.GetBoolean(TestBooleanKey, !TestBooleanValue), Is.EqualTo(TestBooleanValue),
                "Storage in initial vault did not work properly");

            sharedPreferenceVault1.Edit().PutInt(TestIntKey, TestIntValue).Apply();
            Assert.That(sharedPreferenceVault1.GetInt(TestIntKey, 0), Is.EqualTo(TestIntValue), "Storage in initial vault did not work properly");

            sharedPreferenceVault1.Edit().PutLong(TestLongKey, TestLongValue).Apply();
            Assert.That(sharedPreferenceVault1.GetLong(TestLongKey, 0), Is.EqualTo(TestLongValue), "Storage in initial vault did not work properly");

            sharedPreferenceVault1.Edit().PutFloat(TestFloatKey, TestFloatValue).Apply();
            Assert.That(sharedPreferenceVault1.GetFloat(TestFloatKey, 0f), Is.EqualTo(TestFloatValue), "Storage in initial vault did not work properly");

            sharedPreferenceVault1.Edit().PutStringSet(TestStringSetKey, TestStringSetValue).Apply();
            Assert.That(
                sharedPreferenceVault1.GetStringSet(TestStringSetKey, null), Is.EqualTo(TestStringSetValue), "Storage in initial vault did not work properly");

            //Test getAll type checking operation.
            var fullSet = sharedPreferenceVault1.All;
            Assert.That(fullSet[TestStringKey], Is.TypeOf(typeof(string)), "String was not correct type");
            Assert.That(fullSet[TestBooleanKey], Is.TypeOf(typeof(bool)), "Boolean was not correct type");
            Assert.That(fullSet[TestIntKey], Is.TypeOf(typeof(int)), "Integer was not correct type");
            Assert.That(fullSet[TestLongKey], Is.TypeOf(typeof(long)), "Long was not correct type");
            Assert.That(fullSet[TestFloatKey], Is.TypeOf(typeof(float)), "Float was not correct type");
            Assert.That(fullSet[TestStringSetKey], Is.AssignableTo(typeof(ICollection<string>)), "Set was not correct type");

            //Clear data except for the test string key.
            Assert.That(sharedPreferenceVault1.Contains(TestBooleanKey), Is.True, "Contains test did not work");

            sharedPreferenceVault1.Edit().Clear().PutString(TestStringKey, TestStringValue).Commit();
            Assert.That(sharedPreferenceVault1.Contains(TestBooleanKey), Is.False, "Clear operation did not work");

            //Create a secondary instance of the sharedPreferenceVault to ensure separate data reading works and instantiation doesn't clobber old key/data.
            var sharedPreferenceVault2 = SharedPreferenceVaultFactory.GetAppKeyedCompatAes256Vault(
                context, PrefFileName, KeyFileName, KeyAlias1, KeyIndex1, PresharedSecret1);
            Assert.That(sharedPreferenceVault2, Is.Not.Null, "Unable to create second instance of vault");
            Assert.That(sharedPreferenceVault2.GetString(TestStringKey, null), Is.EqualTo(TestStringValue), "Retrieval in second vault did not work properly");

            //Test very large string
            var veryLargeString = CreateRandomString(LargeStringSize);
            sharedPreferenceVault1.Edit().PutString(TestLargeStringKey, veryLargeString).Commit();
            Assert.That(sharedPreferenceVault1.GetString(TestLargeStringKey, null), Is.EqualTo(veryLargeString), "Very long string mismatch");

            //Test data clearing
            sharedPreferenceVault1.ClearStorage();
            Assert.That(sharedPreferenceVault1.IsKeyAvailable, Is.False, "Key was not removed");
            Assert.That(sharedPreferenceVault1.GetString(TestStringKey, null), Is.Null, "Clear storage failed to delete data");
        }
    }
}