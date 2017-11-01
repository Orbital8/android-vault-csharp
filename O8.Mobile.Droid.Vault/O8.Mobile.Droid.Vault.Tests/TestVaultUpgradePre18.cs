// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="TestVaultUpgradePre18.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------

#region Namespaces

using Android.App;
using Android.Content;
using Android.OS;
using Android.Security.Keystore;
using O8.Mobile.Droid.Vault.Keys.Generator;
using O8.Mobile.Droid.Vault.Keys.Storage;
using O8.Mobile.Droid.Vault.Salt;
using NUnit.Framework;

#endregion

namespace O8.Mobile.Droid.Vault.Tests
{
    /// <summary>
    ///     Test transition to version 18 from a pre-18 device if it receives an OS upgrade.
    /// </summary>
    [TestFixture]
    public class TestVaultUpgradePre18
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

        private const string KeyFileName = "upgradeKeyFile";
        private const string KeyAlias1 = "upgradeKeyAlias";
        private const int KeyIndex1 = 1232234;
        private const string PresharedSecret1 = "a;sdlfkja;asdfae211;s122222e;l2ihjl9jl9dj9";

        private Context _context;

        private IKeyStorage GetKeyStorage(BuildVersionCodes sdkInt)
        {
            return CompatSharedPrefKeyStorageFactory.CreateKeyStorage(
                _context, sdkInt, KeyFileName, KeyAlias1, KeyIndex1, KeyProperties.KeyAlgorithmAes, PresharedSecret1, new PrngSaltGenerator());
        }

        [Test]
        public void TestUpgrade()
        {
            var originalKey = Aes256RandomKeyFactory.CreateKey();
            var keyStorageOld = GetKeyStorage(BuildVersionCodes.JellyBean);
            Assert.That(keyStorageOld.KeyStorageType, Is.EqualTo(KeyStorageType.Obfuscated), "Incorrect KeyStorageType");

            keyStorageOld.ClearKey(_context);
            keyStorageOld.SaveKey(_context, originalKey);

            var originalReadKey = keyStorageOld.LoadKey(_context);
            Assert.That(originalReadKey, Is.Not.Null, "Key was null after creation and read from old storage.");
            Assert.That(originalReadKey.GetEncoded(), Is.EqualTo(originalKey.GetEncoded()), "Keys were not identical after creation and read from old storage");

            var keyStorageNew = GetKeyStorage(BuildVersionCodes.JellyBeanMr2);
            Assert.That(keyStorageNew.KeyStorageType, Is.EqualTo(KeyStorageType.AndroidKeystore), "Incorrect KeyStorageType");

            var upgradedKey = keyStorageNew.LoadKey(_context);
            Assert.That(upgradedKey, Is.Not.Null, "Key was null after upgrade.");
            Assert.That(upgradedKey.GetEncoded(), Is.EqualTo(originalKey.GetEncoded()), "Keys were not identical after upgrade");

            var keyStorageRead = GetKeyStorage(BuildVersionCodes.JellyBeanMr2);
            Assert.That(keyStorageRead.KeyStorageType, Is.EqualTo(KeyStorageType.AndroidKeystore), "Incorrect KeyStorageType");

            var upgradedReadKey = keyStorageRead.LoadKey(_context);
            Assert.That(upgradedReadKey, Is.Not.Null, "Key was null after upgrade and read from storage.");
            Assert.That(upgradedReadKey.GetEncoded(), Is.EqualTo(originalKey.GetEncoded()), "Keys were not identical after upgrade and read from storage");
        }
    }
}