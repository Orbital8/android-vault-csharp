// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="TestVaultRegistry.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------

#region Namespaces

using Android.App;
using Android.Content;
using Java.Lang;
using NUnit.Framework;

#endregion

namespace O8.Mobile.Droid.Vault.Tests
{
    /// <summary>
    ///     Ensure uniqueness in registry
    /// </summary>
    [TestFixture]
    public class TestVaultRegistry
    {
        [SetUp]
        public void Setup()
        {
            _context = Application.Context;
        }

        [TearDown]
        public void TearDown()
        {
            _context = null;
        }

        private const string KeyFileName = "registryKeyFile";

        private const string PrefFileName1 = "registryPrefFile1";
        private const string KeyAlias1 = "keyAlias1";
        private const int KeyIndex1 = 1;
        private const string PresharedSecret1 = "a;sdlfkja;lkeiunwiuha;shdluifhe;l2ihjl9jl9dj9";

        private const string PrefFileName2 = "registryPrefFile2";
        private const string KeyAlias2 = "keyAlias2";
        private const int KeyIndex2 = 2;
        private const string PresharedSecret2 = "a;sdlfkja;asdfae22df23f545554656453458382328dfadsf;l2ihjl9jl9dj9";

        private const string PrefFileName3 = "registryPrefFile3";
        private const string KeyAlias3 = "keyAlias3";
        private const int KeyIndex3 = 3;

        private Context _context;

        private void AddToVault(Context context, string prefFileName, string keyFileName, string keyAlias, int keyIndex, string presharedSecret)
        {
            var vault = SharedPreferenceVaultFactory.GetAppKeyedCompatAes256Vault(context, prefFileName, keyFileName, keyAlias, keyIndex, presharedSecret);
            SharedPreferenceVaultRegistry.Instance.AddVault(keyIndex, prefFileName, keyAlias, vault);
            Assert.That(vault, Is.Not.Null, "Error creating vault");
        }

        [Test]
        public void TestRegistryUniqueness()
        {
            SharedPreferenceVaultRegistry.Instance.Clear();

            AddToVault(_context, PrefFileName1, KeyFileName, KeyAlias1, KeyIndex1, PresharedSecret1);
            AddToVault(_context, PrefFileName2, KeyFileName, KeyAlias2, KeyIndex2, PresharedSecret2);
            Assert.That(SharedPreferenceVaultRegistry.Instance.GetVault(KeyIndex1), Is.Not.Null, "Shared preference vault was missing");

            var aliasRepetitionPrevented = false;

            try
            {
                AddToVault(_context, PrefFileName3, KeyFileName, KeyAlias2, KeyIndex3, PresharedSecret2);
            }
            catch (IllegalArgumentException)
            {
                aliasRepetitionPrevented = true;
            }

            Assert.That(aliasRepetitionPrevented, Is.True, "Registry allowed an alias collision");

            var indexRepetitionPrevented = false;
            try
            {
                AddToVault(_context, PrefFileName3, KeyFileName, KeyAlias3, KeyIndex2, PresharedSecret2);
            }
            catch (IllegalArgumentException)
            {
                indexRepetitionPrevented = true;
            }
            Assert.That(indexRepetitionPrevented, Is.True, "Registry allowed an index collision");

            var prefFileRepetitionPrevented = false;
            try
            {
                AddToVault(_context, PrefFileName2, KeyFileName, KeyAlias3, KeyIndex3, PresharedSecret2);
            }
            catch (IllegalArgumentException)
            {
                prefFileRepetitionPrevented = true;
            }
            Assert.That(prefFileRepetitionPrevented, Is.True, "Registry allowed a pref file collision");
        }
    }
}