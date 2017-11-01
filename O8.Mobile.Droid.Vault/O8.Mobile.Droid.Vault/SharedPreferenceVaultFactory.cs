// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="SharedPreferenceVaultFactory.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.App;
using Android.Content;
using Android.OS;
using Android.Security.Keystore;
using O8.Mobile.Droid.Vault.Keys.Generator;
using O8.Mobile.Droid.Vault.Keys.Storage;
using O8.Mobile.Droid.Vault.Salt;
using Java.Lang;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     Factory to generate SharedPreference backed secure storage vaults.
    /// </summary>
    public static class SharedPreferenceVaultFactory
    {
        /// <summary>
        ///     Create an unkeyed vault. Use this when you wish to set the key later based on a user provided password
        ///     or when using a specific key strategy.The vault will be unusable until the SecretKey is set using
        ///     {@link SharedPreferenceVault#rekeyStorage(javax.crypto.SecretKey)}. You must verify
        ///     that the vault is not already keyed using {@link SharedPreferenceVault#isKeyAvailable()}
        /// </summary>
        /// <returns>The compat aes256 vault.</returns>
        /// <param name="context">Context.</param>
        /// <param name="prefFileName">Preference file name.</param>
        /// <param name="keyFileName">Key file name.</param>
        /// <param name="keyAlias">Key alias.</param>
        /// <param name="keyIndex">Key index.</param>
        /// <param name="presharedSecret">Preshared secret.</param>
        /// <param name="enableExceptions">Enable exceptions.</param>
        /// <remarks>
        ///     For devices running Android below 18, an obfuscated storage system will be used to store the key.
        ///     For devices using Android 18+ the AndroidKeystore secure storage will be used.If an application
        ///     was running on a device that was below 18 and the device was upgraded to 18+ the key
        ///     storage will be upgraded automatically and migrate the key.
        /// </remarks>
        public static ISharedPreferenceVault GetCompatAes256Vault(
            Context context, string prefFileName, string keyFileName, string keyAlias, int keyIndex, string presharedSecret, bool enableExceptions)
        {
            if (prefFileName == keyFileName)
            {
                throw new IllegalArgumentException("Preference file and key file cannot be the same file.");
            }

            var keyStorage = CompatSharedPrefKeyStorageFactory.CreateKeyStorage(
                context, Build.VERSION.SdkInt, keyFileName, keyAlias, keyIndex, KeyProperties.KeyAlgorithmAes, presharedSecret, new PrngSaltGenerator());
            return new StandardSharedPreferenceVault(context, keyStorage, prefFileName, EncryptionConstants.AesCbcPaddedTransform, enableExceptions);
        }

        /// <summary>
        ///     @see SharedPreferenceVaultFactory#getCompatAes256Vault(Context, String, String, String, int, String, boolean)
        /// </summary>
        /// <returns>The compat aes256 vault.</returns>
        /// <param name="context">Context.</param>
        /// <param name="prefFileName">Preference file name.</param>
        /// <param name="keyFileName">Key file name.</param>
        /// <param name="keyAlias">Key alias.</param>
        /// <param name="keyIndex">Key index.</param>
        /// <param name="presharedSecret">Preshared secret.</param>
        public static ISharedPreferenceVault GetCompatAes256Vault(
            Context context, string prefFileName, string keyFileName, string keyAlias, int keyIndex, string presharedSecret)
        {
            return GetCompatAes256Vault(context, prefFileName, keyFileName, keyAlias, keyIndex, presharedSecret, false);
        }

        /// <summary>
        ///     Create an application keyed pseudo random vault for storage of secure information. Use this when
        ///     there is no ability to secure the information using the user's password e.g. API client tokens and sensitive app
        ///     configuration.
        ///     @see com.bottlerocketstudios.vault.SharedPreferenceVaultFactory#getCompatAes256Vault(android.content.Context,
        ///     String, String, String, int, String)
        /// </summary>
        /// <returns>The app keyed compat aes256 vault.</returns>
        /// <param name="context">Context.</param>
        /// <param name="prefFileName">Preference file name.</param>
        /// <param name="keyFileName">Key file name.</param>
        /// <param name="keyAlias">Key alias.</param>
        /// <param name="keyIndex">Key index.</param>
        /// <param name="presharedSecret">Preshared secret.</param>
        /// <param name="enableExceptions">Enable exceptions.</param>
        public static ISharedPreferenceVault GetAppKeyedCompatAes256Vault(
            Context context, string prefFileName, string keyFileName, string keyAlias, int keyIndex, string presharedSecret, bool enableExceptions)
        {
            var sharedPreferenceVault = GetCompatAes256Vault(context, prefFileName, keyFileName, keyAlias, keyIndex, presharedSecret, enableExceptions);
            if (!sharedPreferenceVault.IsKeyAvailable)
            {
                sharedPreferenceVault.RekeyStorage(Aes256RandomKeyFactory.CreateKey());
            }

            return sharedPreferenceVault;
        }

        /// <summary>
        ///     @see SharedPreferenceVaultFactory#getAppKeyedCompatAes256Vault(Context, String, String, String, int, String,
        ///     boolean)
        /// </summary>
        /// <returns>The app keyed compat aes256 vault.</returns>
        /// <param name="context">Context.</param>
        /// <param name="prefFileName">Preference file name.</param>
        /// <param name="keyFileName">Key file name.</param>
        /// <param name="keyAlias">Key alias.</param>
        /// <param name="keyIndex">Key index.</param>
        /// <param name="presharedSecret">Preshared secret.</param>
        public static ISharedPreferenceVault GetAppKeyedCompatAes256Vault(
            Context context, string prefFileName, string keyFileName, string keyAlias, int keyIndex, string presharedSecret)
        {
            return GetAppKeyedCompatAes256Vault(context, prefFileName, keyFileName, keyAlias, keyIndex, presharedSecret, false);
        }

        /// <summary>
        ///     Create a vault that uses the operating system's built in keystore locking mechanism. Whenever
        ///     the device has not been unlocked in a specified amount of time, reading from this vault will
        ///     throw a {@link android.security.keystore.KeyPermanentlyInvalidatedException}
        ///     or {@link android.security.keystore.UserNotAuthenticatedException}.
        /// </summary>
        /// <returns>The keychain authenticated aes256 vault.</returns>
        /// <param name="context">Context.</param>
        /// <param name="prefFileName">Preference file name.</param>
        /// <param name="keyAlias">Key alias.</param>
        /// <param name="authDurationSeconds">Auth duration seconds.</param>
        public static ISharedPreferenceVault GetKeychainAuthenticatedAes256Vault(Context context, string prefFileName, string keyAlias, int authDurationSeconds)
        {
            var keyStorage = new KeychainAuthenticatedKeyStorage(
                keyAlias, KeyProperties.KeyAlgorithmAes, KeyProperties.BlockModeCbc, KeyProperties.EncryptionPaddingPkcs7, authDurationSeconds);

            var sharedPreferenceVault = new StandardSharedPreferenceVault(
                context, keyStorage, prefFileName, EncryptionConstants.AesCbcPaddedTransformAndroidM, true);
            if (!sharedPreferenceVault.IsKeyAvailable)
            {
                sharedPreferenceVault.RekeyStorage(null);
            }
            return sharedPreferenceVault;
        }

        public static bool CanUseKeychainAuthentication(Context context)
        {
            var keyguardManager = (KeyguardManager)context.GetSystemService(Context.KeyguardService);
            return keyguardManager.IsKeyguardSecure;
        }

        /// <summary>
        ///     Create a vault that will not persist the key to any secure storage system. The key is kept in
        ///     memory only and can be unset with {@link SharedPreferenceVault#setKey(SecretKey)}  with a null SecretKey.
        ///     Check {@link SharedPreferenceVault#isKeyAvailable()} before attempting to read or write information.
        /// </summary>
        /// <returns>The memory only key aes256 vault.</returns>
        /// <param name="context">Context.</param>
        /// <param name="prefFileName">Preference file name.</param>
        /// <param name="enableExceptions">Enable exceptions.</param>
        public static ISharedPreferenceVault GetMemoryOnlyKeyAes256Vault(Context context, string prefFileName, bool enableExceptions)
        {
            var keyStorage = new MemoryOnlyKeyStorage();
            return new StandardSharedPreferenceVault(context, keyStorage, prefFileName, EncryptionConstants.AesCbcPaddedTransform, enableExceptions);
        }
    }
}