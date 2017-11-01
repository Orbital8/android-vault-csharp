// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="TestKeyGeneration.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------

#region Namespaces

using O8.Mobile.Droid.Vault.Keys.Generator;
using NUnit.Framework;

#endregion

namespace O8.Mobile.Droid.Vault.Tests
{
    /// <summary>
    ///     Test secret key creation.
    /// </summary>
    [TestFixture]
    public class TestKeyGeneration
    {
        [Test]
        public void TestPasswordKey()
        {
            var secretKey = Aes256KeyFromPasswordFactory.CreateKey("testPassword", 10000);
            Assert.That(secretKey, Is.Not.Null, "Secret key was not created");
            Assert.That(secretKey.GetEncoded().Length, Is.EqualTo(EncryptionConstants.Aes256KeyLengthBits / 8), "Secret key was incorrect length");
        }

        [Test]
        public void TestRandomKey()
        {
            var secretKey = Aes256RandomKeyFactory.CreateKey();

            Assert.That(secretKey, Is.Not.Null, "Secret key was not created");
            Assert.That(secretKey.GetEncoded().Length, Is.EqualTo(EncryptionConstants.Aes256KeyLengthBits / 8), "Secret key was incorrect length");
        }
    }
}