// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="Aes256KeyFromPasswordGenerator.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using O8.Mobile.Droid.Vault.Salt;
using Javax.Crypto;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Generator
{
    /// <summary>
    ///     Create an AES256 key from a user supplied password.
    /// </summary>
    public class Aes256KeyFromPasswordFactory
    {
        private const int SaltSizeBytes = 512;

        /// <summary>
        ///     This will execute the key generation for the number of supplied iterations and use a unique random salt.
        ///     This will block for a while depending on processor speed.
        /// </summary>
        /// <returns>The key.</returns>
        /// <param name="password">Password.</param>
        /// <param name="pbkdfIterations">Pbkdf iterations.</param>
        public static ISecretKey CreateKey(string password, int pbkdfIterations)
        {
            return CreateKey(password, pbkdfIterations, new PrngSaltGenerator());
        }

        /// <summary>
        ///     This will execute the key generation for the number of supplied iterations and use salt from the
        ///     supplied source.This will block for a while depending on processor speed.
        /// </summary>
        /// <returns>The key.</returns>
        /// <param name="password">Password.</param>
        /// <param name="pbkdfIterations">Pbkdf iterations.</param>
        /// <param name="saltGenerator">Salt generator.</param>
        public static ISecretKey CreateKey(string password, int pbkdfIterations, ISaltGenerator saltGenerator)
        {
            var keyGenerator = new PbkdfKeyGenerator(pbkdfIterations, EncryptionConstants.Aes256KeyLengthBits, saltGenerator, SaltSizeBytes);
            return keyGenerator.GenerateKey(password);
        }
    }
}