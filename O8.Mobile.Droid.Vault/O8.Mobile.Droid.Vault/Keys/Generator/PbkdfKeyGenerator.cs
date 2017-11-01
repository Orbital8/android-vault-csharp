// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="PbkdfKeyGenerator.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Util;
using O8.Mobile.Droid.Vault.Salt;
using Java.Security;
using Java.Security.Spec;
using Javax.Crypto;
using Javax.Crypto.Spec;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Generator
{
    /// <summary>
    ///     Generate a SecretKey given a user supplied password string.
    /// </summary>
    public class PbkdfKeyGenerator
    {
        private const string Tag = "O8.Mobile.Droid.Vault.Keys.Generator.PbkdfKeyGenerator";
        private const string PbeAlgorithm = "PBKDF2WithHmacSHA1";
        private readonly int _keyLengthBits;

        private readonly int _pbkdf2Iterations;
        private readonly ISaltGenerator _saltGenerator;
        private readonly int _saltSize;

        public PbkdfKeyGenerator(int pbkdf2Iterations, int keyLengthBits, ISaltGenerator saltGenerator, int saltSizeBytes)
        {
            _pbkdf2Iterations = pbkdf2Iterations;
            _saltGenerator = saltGenerator;
            _saltSize = saltSizeBytes;
            _keyLengthBits = keyLengthBits;
        }

        public ISecretKey GenerateKey(string keySource)
        {
            return CreateKeyWithPassword(keySource);
        }

        private ISecretKey CreateKeyWithPassword(string password)
        {
            var passwordSalt = _saltGenerator.CreateSaltBytes(_saltSize);
            var spec = new PBEKeySpec(password.ToCharArray(), passwordSalt, _pbkdf2Iterations, _keyLengthBits);

            try
            {
                var skf = SecretKeyFactory.GetInstance(PbeAlgorithm);
                return skf.GenerateSecret(spec);
            }
            catch (InvalidKeySpecException ex)
            {
                Log.Error(Tag, "Failed to process key", ex);
            }
            catch (NoSuchAlgorithmException ex)
            {
                Log.Error(Tag, "Failed to process key", ex);
            }

            return null;
        }
    }
}