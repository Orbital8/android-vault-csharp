// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="ISharedPreferenceVault.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Content;
using O8.Mobile.Droid.Vault.Keys.Storage;
using Javax.Crypto;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     Shared Preferences backed vault for storing sensitive information.
    /// </summary>
    public interface ISharedPreferenceVault : ISharedPreferences
    {
        /// <summary>
        ///     Determine if this instance of storage currently has a valid key with which to encrypt values.
        /// </summary>
        /// <value><c>true</c> if is key available; otherwise, <c>false</c>.</value>
        bool IsKeyAvailable { get; }

        /// <summary>
        ///     Gets the expected security level of KeyStorage implementation being used.
        /// </summary>
        /// <returns>The key storage type.</returns>
        KeyStorageType KeyStorageType { get; }

        /// <summary>
        ///     Remove all stored values and destroy cryptographic keys associated with the vault instance.
        /// </summary>
        void ClearStorage();

        /// <summary>
        ///     Remove all stored values and destroy cryptographic keys associated with the vault instance.
        ///     Configure the vault to use the newly provided key for future data.
        /// </summary>
        /// <param name="secretKey">Secret key.</param>
        void RekeyStorage(ISecretKey secretKey);

        /// <summary>
        ///     Arbitrarily set the secret key to a specific value without removing any stored values. This is primarily
        ///     designed for {@link com.bottlerocketstudios.vault.keys.storage.MemoryOnlyKeyStorage} and typical
        ///     usage would be through the {@link #rekeyStorage(SecretKey)} method.
        /// </summary>
        /// <param name="secretKey">Secret key.</param>
        void SetKey(ISecretKey secretKey);
    }
}