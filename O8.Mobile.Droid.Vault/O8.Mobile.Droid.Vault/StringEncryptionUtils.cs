// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="StringEncryptionUtils.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System;
using System.Text;
using Java.Lang;
using Java.Nio;
using Java.Security;
using Javax.Crypto;
using Javax.Crypto.Spec;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     Tools to perform cryptographic transformations on strings resulting in Base64 encoded strings.
    /// </summary>
    public class StringEncryptionUtils
    {
        private const byte HeaderMagicNumber = 121;
        private const byte HeaderVersion = 1;
        private const int HeaderIvOffset = 2;
        private const int IntegerSizeBytes = sizeof(int);
        private const int HeaderMetadataSize = HeaderIvOffset + IntegerSizeBytes;

        /// <summary>
        ///     Generate a Base64 encoded string containing an AES encrypted version of cleartext using the provided seed to
        ///     generate a key.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="clearText">Clear text.</param>
        /// <param name="charset">Charset.</param>
        /// <param name="transform">Transform.</param>
        public static string Encrypt(ISecretKey key, string clearText, Encoding charset, string transform)
        {
            if (clearText == null)
            {
                return null;
            }

            var stringBytes = charset.GetBytes(clearText);
            var result = Encrypt(key, stringBytes, transform);
            return Convert.ToBase64String(result);
        }

        /// <summary>
        ///     Decode a Base64 encoded string into a cleartext string using the provided key and charset.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="encrypted">Encrypted.</param>
        /// <param name="charset">Charset.</param>
        /// <param name="transform">Transform.</param>
        public static string Decrypt(ISecretKey key, string encrypted, Encoding charset, string transform)
        {
            if (encrypted == null)
            {
                return null;
            }

            try
            {
                var enc = Convert.FromBase64String(encrypted);
                var result = Decrypt(key, enc, transform);

                if (result != null)
                {
                    return charset.GetString(result);
                }
            }
            catch (IllegalArgumentException e)
            {
                throw new UnencryptedException("Encrypted String was not base64 encoded.", e);
            }

            return null;
        }

        private static byte[] CreateIvHeader(byte[] iv)
        {
            var byteBuffer = ByteBuffer.Allocate(iv.Length + HeaderMetadataSize);

            byteBuffer.Put(new[] { HeaderMagicNumber });
            byteBuffer.Put(new[] { HeaderVersion });
            byteBuffer.PutInt(iv.Length);
            byteBuffer.Put(iv);

            byteBuffer.Position(0);
            var result = new byte[byteBuffer.Remaining()];
            byteBuffer.Get(result);
            return result;
        }

        private static Tuple<byte[], byte[]> ReadIvFromHeader(byte[] encrypted)
        {
            if (encrypted == null)
            {
                return null;
            }

            var encryptedBuffer = ByteBuffer.Wrap(encrypted);
            if (encrypted.Length <= HeaderMetadataSize)
            {
                throw new GeneralSecurityException("Not enough data");
            }
            if (encryptedBuffer.Get() != HeaderMagicNumber)
            {
                throw new GeneralSecurityException("Invalid header");
            }
            if (encryptedBuffer.Get() != HeaderVersion)
            {
                throw new GeneralSecurityException("Incorrect header version");
            }

            var ivSize = encryptedBuffer.Int;
            byte[] iv = null;
            if (ivSize > 0)
            {
                iv = new byte[ivSize];
                encryptedBuffer.Get(iv, 0, ivSize);
            }

            var dataSize = encrypted.Length - (HeaderMetadataSize + ivSize);
            var data = new byte[dataSize];
            encryptedBuffer.Get(data, 0, dataSize);

            return new Tuple<byte[], byte[]>(iv, data);
        }


        private static byte[] Encrypt(ISecretKey key, byte[] clearText, string transform)
        {
            using (var cipher = Cipher.GetInstance(transform))
            {
                cipher.Init(CipherMode.EncryptMode, key);

                var data = cipher.DoFinal(clearText);
                var header = CreateIvHeader(cipher.GetIV());
                return ConcatByteArrays(header, data);
            }
        }

        private static byte[] Decrypt(ISecretKey key, byte[] encrypted, string transform)
        {
            using (var cipher = Cipher.GetInstance(transform))
            {
                var dataPair = ReadIvFromHeader(encrypted);

                if (dataPair != null)
                {
                    if (dataPair.Item1 == null)
                    {
                        cipher.Init(CipherMode.DecryptMode, key);
                    }
                    else
                    {
                        cipher.Init(CipherMode.DecryptMode, key, new IvParameterSpec(dataPair.Item1));
                    }

                    return cipher.DoFinal(dataPair.Item2);
                }
            }

            return null;
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