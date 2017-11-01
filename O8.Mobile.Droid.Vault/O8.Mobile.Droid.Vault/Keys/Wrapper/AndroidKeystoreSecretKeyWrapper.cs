// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="AndroidKeystoreSecretKeyWrapper.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Security;
using Android.Security.Keystore;
using O8.Mobile.Droid.Vault.Keys.Storage;
using Java.Math;
using Java.Security;
using Java.Security.Spec;
using Java.Util;
using Javax.Crypto;
using Javax.Security.Auth.X500;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Wrapper
{
    /// <summary>
    ///     Wraps {@link javax.crypto.SecretKey} instances using a public/private key pair stored in
    ///     the platform {@link java.security.KeyStore}. This allows us to protect symmetric keys with
    ///     hardware-backed crypto, if provided by the device.
    /// </summary>
    /// <remarks>
    ///     See <a href="http://en.wikipedia.org/wiki/Key_Wrap">key wrapping</a> for more details.
    /// </remarks>
    public class AndroidKeystoreSecretKeyWrapper : ISecretKeyWrapper
    {
        public const string Transformation = "RSA/ECB/PKCS1Padding";
        public const string Algorithm = "RSA";
        public const int CertificateLifeYears = 100;
        private readonly string _alias;

        private readonly Cipher _cipher;
        private readonly Context _context;
        private KeyPair _keyPair;

        /// <summary>
        ///     Create a wrapper using the public/private key pair with the given alias.
        ///     If no pair with that alias exists, it will be generated.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="alias">Alias.</param>
        public AndroidKeystoreSecretKeyWrapper(Context context, string alias)
        {
            _alias = alias;
            _cipher = Cipher.GetInstance(Transformation);
            _context = context.ApplicationContext;
        }

        public bool TestKey
        {
            get
            {
                var keyPair = GetKeyPair();
                return keyPair != null;
            }
        }

        public byte[] Wrap(ISecretKey key)
        {
            _cipher.Init(CipherMode.WrapMode, GetKeyPair().Public);
            return _cipher.Wrap(key);
        }

        public ISecretKey Unwrap(byte[] blob, string wrappedKeyAlgorithm)
        {
            _cipher.Init(CipherMode.UnwrapMode, GetKeyPair().Private);
            return _cipher.Unwrap(blob, wrappedKeyAlgorithm, KeyType.SecretKey).JavaCast<ISecretKey>();
        }

        public void ClearKey(Context context)
        {
            _keyPair = null;
            var keyStore = KeyStore.GetInstance(EncryptionConstants.AndroidKeyStore);
            keyStore.Load(null);
            keyStore.DeleteEntry(_alias);
        }

        public KeyStorageType KeyStorageType
        {
            get { return KeyStorageType.AndroidKeystore; }
        }

        public static IAlgorithmParameterSpec GetVersionAppropriateAlgorithmParameterSpec(
            Context context, string alias, Calendar start, Calendar end, BigInteger serial, X500Principal subject)
        {
            IAlgorithmParameterSpec algorithmParameterSpec;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                algorithmParameterSpec = BuildApi23AlgorithmParameterSpec(alias, start, end, serial, subject);
            }
            else
            {
                algorithmParameterSpec = BuildLegacyAlgorithmParameterSpec(context, alias, start, end, serial, subject);
            }

            return algorithmParameterSpec;
        }

        private KeyPair GetKeyPair()
        {
            lock (_alias)
            {
                if (_keyPair == null)
                {
                    var keyStore = KeyStore.GetInstance(EncryptionConstants.AndroidKeyStore);
                    keyStore.Load(null);

                    if (!keyStore.ContainsAlias(_alias))
                    {
                        GenerateKeyPair(_context, _alias);
                    }

                    // Even if we just generated the key, always read it back to ensure we
                    // can read it successfully.
                    var entry = (KeyStore.PrivateKeyEntry)keyStore.GetEntry(_alias, null);
                    _keyPair = new KeyPair(entry.Certificate.PublicKey, entry.PrivateKey);
                }
            }
            return _keyPair;
        }

        private static void GenerateKeyPair(Context context, string alias)
        {
            var start = new GregorianCalendar();
            var end = new GregorianCalendar();
            end.Add(CalendarField.Year, CertificateLifeYears);

            var algorithmParameterSpec = GetVersionAppropriateAlgorithmParameterSpec(
                context, alias, start, end, BigInteger.One, new X500Principal("CN=" + alias));
            var gen = KeyPairGenerator.GetInstance(Algorithm, EncryptionConstants.AndroidKeyStore);
            gen.Initialize(algorithmParameterSpec);
            gen.GenerateKeyPair();
        }

        private static IAlgorithmParameterSpec BuildLegacyAlgorithmParameterSpec(
            Context context, string alias, Calendar start, Calendar end, BigInteger serialNumber, X500Principal subject)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return
                new KeyPairGeneratorSpec.Builder(context).SetAlias(alias)
                    .SetSubject(subject)
                    .SetSerialNumber(serialNumber)
                    .SetStartDate(start.Time)
                    .SetEndDate(end.Time)
                    .Build();
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static IAlgorithmParameterSpec BuildApi23AlgorithmParameterSpec(
            string alias, Calendar start, Calendar end, BigInteger serialNumber, X500Principal subject)
        {
            return
                new KeyGenParameterSpec.Builder(alias, KeyStorePurpose.Decrypt | KeyStorePurpose.Encrypt | KeyStorePurpose.Sign).SetCertificateSubject(subject)
                    .SetCertificateSerialNumber(serialNumber)
                    .SetKeyValidityStart(start.Time)
                    .SetCertificateNotBefore(start.Time)
                    .SetKeyValidityEnd(end.Time)
                    .SetCertificateNotAfter(end.Time)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingRsaPkcs1)
                    .SetBlockModes(KeyProperties.BlockModeEcb)
                    .Build();
        }
    }
}