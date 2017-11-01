// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="SharedPrefKeyStorage.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Content;
using Android.Util;
using O8.Mobile.Droid.Vault.Keys.Wrapper;
using Java.IO;
using Java.Lang;
using Java.Security;
using Javax.Crypto;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Storage
{
    /// <summary>
    ///     Storage system using SharedPreference file to retain SecretKeys.
    /// </summary>
    public class SharedPrefKeyStorage : IKeyStorage
    {
        private const string Tag = "O8.Mobile.Droid.Vault.Keys.Storage.SharedPrefKeyStorage";

        private const string PrefRoot = "vaultedBlobV2.";
        private readonly string _cipherAlgorithm;
        private readonly object _keyLock = new object();
        private readonly string _keystoreAlias;
        private readonly string _prefFileName;

        private readonly ISecretKeyWrapper _secretKeyWrapper;
        private ISecretKey _cachedSecretKey;

        public SharedPrefKeyStorage(ISecretKeyWrapper secretKeyWrapper, string prefFileName, string keystoreAlias, string cipherAlgorithm)
        {
            _secretKeyWrapper = secretKeyWrapper;
            _prefFileName = prefFileName;
            _keystoreAlias = keystoreAlias;
            _cipherAlgorithm = cipherAlgorithm;
        }

        public ISecretKey LoadKey(Context context)
        {
            if (_cachedSecretKey == null)
            {
                //Only allow one thread at a time load the key.
                lock (_keyLock)
                {
                    //If the other thread updated the key, don't re-load it.
                    if (_cachedSecretKey == null)
                    {
                        _cachedSecretKey = LoadSecretKey(context, _keystoreAlias, _cipherAlgorithm);
                    }
                }
            }

            return _cachedSecretKey;
        }

        public bool SaveKey(Context context, ISecretKey secretKey)
        {
            bool success;

            lock (_keyLock)
            {
                success = StoreSecretKey(context, _keystoreAlias, secretKey);

                //Clear the cached key upon failure to save.
                _cachedSecretKey = success ? secretKey : null;
            }

            return success;
        }

        public void ClearKey(Context context)
        {
            _cachedSecretKey = null;
            StoreSecretKey(context, _keystoreAlias, null);

            try
            {
                _secretKeyWrapper.ClearKey(context);
            }
            catch (GeneralSecurityException e)
            {
                Log.Error(Tag, "Failed to clearKey in wrapper", e);
            }
            catch (IOException e)
            {
                Log.Error(Tag, "Failed to clearKey in wrapper", e);
            }
        }

        public bool HasKey(Context context)
        {
            var secretKey = LoadKey(context);
            return secretKey != null;
        }

        public KeyStorageType KeyStorageType
        {
            get { return _secretKeyWrapper.KeyStorageType; }
        }

        /// <summary>
        ///     Return shared preference file to use for encrypted key storage
        /// </summary>
        /// <returns>The shared preferences.</returns>
        /// <param name="context">Context.</param>
        protected ISharedPreferences GetSharedPreferences(Context context)
        {
            return context.GetSharedPreferences(_prefFileName, FileCreationMode.Private);
        }

        /// <summary>
        ///     Return key used to store in shared preferences.
        /// </summary>
        /// <returns>The shared preference key.</returns>
        /// <param name="keystoreAlias">Keystore alias.</param>
        protected string GetSharedPreferenceKey(string keystoreAlias)
        {
            return PrefRoot + keystoreAlias;
        }

        /// <summary>
        ///     Use the SecretKeyWrapper secure storage to read the key in a securely wrapped format
        /// </summary>
        /// <returns>The secret key.</returns>
        /// <param name="context">Context.</param>
        /// <param name="keystoreAlias">Keystore alias.</param>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        protected ISecretKey LoadSecretKey(Context context, string keystoreAlias, string cipherAlgorithm)
        {
            var sharedPreferences = GetSharedPreferences(context);
            var encrypted = sharedPreferences.GetString(GetSharedPreferenceKey(keystoreAlias), null);

            if (encrypted != null)
            {
                try
                {
                    var enc = Base64.Decode(encrypted, Base64Flags.Default);
                    return _secretKeyWrapper.Unwrap(enc, cipherAlgorithm);
                }
                catch (GeneralSecurityException e)
                {
                    Log.Error(Tag, "load failed", e);
                }
                catch (RuntimeException e)
                {
                    Log.Error(Tag, "load failed", e);
                }
                catch (IOException e)
                {
                    Log.Error(Tag, "load failed", e);
                }
            }
            return null;
        }

        /// <summary>
        ///     Use the SecretKeyWrapper secure storage to write the key in a securely wrapped format
        /// </summary>
        /// <returns>The secret key.</returns>
        /// <param name="context">Context.</param>
        /// <param name="keystoreAlias">Keystore alias.</param>
        /// <param name="secretKey">Secret key.</param>
        protected bool StoreSecretKey(Context context, string keystoreAlias, ISecretKey secretKey)
        {
            var editor = GetSharedPreferences(context).Edit();
            if (secretKey == null)
            {
                editor.Remove(GetSharedPreferenceKey(keystoreAlias));
                editor.Apply();
                return true;
            }
            try
            {
                var wrappedKey = _secretKeyWrapper.Wrap(secretKey);
                var encoded = Base64.EncodeToString(wrappedKey, Base64Flags.Default);
                editor.PutString(GetSharedPreferenceKey(keystoreAlias), encoded);
                editor.Apply();
                return true;
            }
            catch (GeneralSecurityException e)
            {
                Log.Error(Tag, "save failed", e);
            }
            catch (RuntimeException e)
            {
                Log.Error(Tag, "save failed", e);
            }
            catch (IOException e)
            {
                Log.Error(Tag, "save failed", e);
            }
            return false;
        }
    }
}