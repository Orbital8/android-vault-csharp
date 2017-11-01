// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="ObfuscatingSecretKeyWrapper.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System.Text;
using Android.Content;
using Android.Security.Keystore;
using Android.Util;
using O8.Mobile.Droid.Vault.Keys.Generator;
using O8.Mobile.Droid.Vault.Keys.Storage;
using O8.Mobile.Droid.Vault.Salt;
using Java.IO;
using Java.Security;
using Javax.Crypto;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Wrapper
{
    /// <summary>
    ///     Creates an AES encrypted wrapped version of the key. This should only be used on less secure
    ///     devices with API &lt; 18. This combines some information built into the application(presharedSecret)
    ///     along with some data randomly generated upon first use.This will generate a unique key per
    ///     installation of the application.
    /// </summary>
    public class ObfuscatingSecretKeyWrapper : ISecretKeyWrapper
    {
        private const string Tag = "O8.Mobile.Droid.Vault.Keys.Wrapper.ObfuscatingSecretKeyWrapper";
        private const int SaltSizeBytes = 512;
        private const string WrappedKeyHash = KeyProperties.DigestSha256;
        private const string WrappedKeyAlgorithm = KeyProperties.KeyAlgorithmAes;
        private const string WrappedKeyTransformation = "AES/ECB/PKCS5Padding";

        private readonly Context _context;
        private readonly string _presharedSecret;
        private readonly ISaltGenerator _saltGenerator;
        private readonly int _saltIndex;
        private readonly Cipher _cipher;
        private byte[] _salt;

        private ISecretKey _wrappingKey;

        /// <summary>
        ///     Create a new instance of an Obfuscating SecretKey Wrapper.
        /// </summary>
        public ObfuscatingSecretKeyWrapper(Context context, int saltIndex, ISaltGenerator saltGenerator, string presharedSecret)
        {
            _context = context.ApplicationContext;
            _saltIndex = saltIndex;
            _saltGenerator = saltGenerator;
            _presharedSecret = presharedSecret;
            _cipher = Cipher.GetInstance(WrappedKeyTransformation);
        }


        public KeyStorageType KeyStorageType
        {
            get { return KeyStorageType.Obfuscated; }
        }

        public byte[] Wrap(ISecretKey key)
        {
            _cipher.Init(CipherMode.WrapMode, GetWrappingKey(_context));
            return _cipher.Wrap(key);
        }

        public ISecretKey Unwrap(byte[] blob, string wrappedKeyAlgorithm)
        {
            _cipher.Init(CipherMode.UnwrapMode, GetWrappingKey(_context));
            return (ISecretKey)_cipher.Unwrap(blob, wrappedKeyAlgorithm, KeyType.SecretKey);
        }

        public void ClearKey(Context context)
        {
            _wrappingKey = null;
            _salt = null;
            SaltBox.WriteStoredBits(context, _saltIndex, null, SaltSizeBytes);
        }

        private ISecretKey GetWrappingKey(Context context)
        {
            if (_wrappingKey == null)
            {
                var salt = GetSalt(context);

                try
                {
                    _wrappingKey = SecretKeySpecGenerator.GetFullKey(
                        SecretKeySpecGenerator.ConcatByteArrays(Encoding.UTF8.GetBytes(_presharedSecret), salt), WrappedKeyHash, WrappedKeyAlgorithm);
                }
                catch (UnsupportedEncodingException e)
                {
                    Log.Error(Tag, "Caught java.io.UnsupportedEncodingException", e);
                }
                catch (NoSuchAlgorithmException e)
                {
                    Log.Error(Tag, "Caught java.security.NoSuchAlgorithmException", e);
                }
            }

            return _wrappingKey;
        }

        private byte[] GetSalt(Context context)
        {
            if (_salt == null)
            {
                _salt = SaltBox.GetStoredBits(context, _saltIndex, SaltSizeBytes);
                if (_salt == null)
                {
                    _salt = CreateSalt(context);
                }
            }

            return _salt;
        }

        private byte[] CreateSalt(Context context)
        {
            var salt = _saltGenerator.CreateSaltBytes(SaltSizeBytes);
            SaltBox.WriteStoredBits(context, _saltIndex, salt, SaltSizeBytes);
            return salt;
        }
    }
}