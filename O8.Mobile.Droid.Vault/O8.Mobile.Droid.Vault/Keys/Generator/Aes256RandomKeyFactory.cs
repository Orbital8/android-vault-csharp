// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="Aes256RandomKeyFactory.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Security.Keystore;
using O8.Mobile.Droid.Vault.Salt;
using Javax.Crypto;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Generator
{
    /// <summary>
    ///     Create a new random AES256 key.
    /// </summary>
    public class Aes256RandomKeyFactory
    {
        /// <summary>
        ///     Create a randomly generated AES256 key.
        /// </summary>
        /// <returns>The key.</returns>
        public static ISecretKey CreateKey()
        {
            var keyGenerator = new RandomKeyGenerator(new PrngSaltGenerator(), EncryptionConstants.Aes256KeyLengthBits);
            return keyGenerator.GenerateKey(KeyProperties.KeyAlgorithmAes);
        }
    }
}