// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="SecretKeyGenerator.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System.Text;
using Java.Security;
using Javax.Crypto.Spec;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Generator
{
    /// <summary>
    ///     Generate a secret key spec by performing a digest on a supplied set of bytes.
    /// </summary>
    public class SecretKeySpecGenerator
    {
        /// <summary>
        ///     Perform digest on seed to get key that will always be the same for any given seed.
        /// </summary>
        /// <returns>The full key.</returns>
        /// <param name="seed">Seed.</param>
        /// <param name="digest">Digest.</param>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        public static SecretKeySpec GetFullKey(string seed, string digest, string cipherAlgorithm)
        {
            return GetFullKey(Encoding.UTF8.GetBytes(seed), digest, cipherAlgorithm);
        }

        /// <summary>
        ///     Perform digest on seed to get key that will always be the same for any given seed.
        /// </summary>
        /// <returns>The full key.</returns>
        /// <param name="seed">Seed.</param>
        /// <param name="digest">Digest.</param>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        public static SecretKeySpec GetFullKey(byte[] seed, string digest, string cipherAlgorithm)
        {
            var md = MessageDigest.GetInstance(digest);
            return new SecretKeySpec(md.Digest(seed), cipherAlgorithm);
        }

        /// <summary>
        ///     Combine two byte arrays into one and return it.
        /// </summary>
        /// <returns>The byte arrays.</returns>
        /// <param name="a">The alpha component.</param>
        /// <param name="b">The blue component.</param>
        public static byte[] ConcatByteArrays(byte[] a, byte[] b)
        {
            var c = new byte[a.Length + b.Length];

            a.CopyTo(c, 0);
            b.CopyTo(c, a.Length);

            return c;
        }
    }
}