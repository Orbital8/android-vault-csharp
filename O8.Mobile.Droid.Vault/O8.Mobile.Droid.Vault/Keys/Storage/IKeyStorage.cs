// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="IKeyStorage.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Content;
using Javax.Crypto;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Storage
{
    /// <summary>
    ///     Storage interface for secret key material.
    /// </summary>
    public interface IKeyStorage
    {
        /// <summary>
        ///     Return the type of key storage used.
        /// </summary>
        /// <returns>The key storage type.</returns>
        KeyStorageType KeyStorageType { get; }

        /// <summary>
        ///     Load key from storage
        /// </summary>
        /// <returns>The key.</returns>
        /// <param name="context">Context.</param>
        ISecretKey LoadKey(Context context);

        /// <summary>
        ///     Save key to storage
        /// </summary>
        /// <returns><c>true</c>, if key was saved, <c>false</c> otherwise.</returns>
        /// <param name="context">Context.</param>
        /// <param name="secretKey">Secret key.</param>
        bool SaveKey(Context context, ISecretKey secretKey);

        /// <summary>
        ///     Remove key from storage
        /// </summary>
        /// <param name="context">Context.</param>
        void ClearKey(Context context);

        /// <summary>
        ///     Determine if key is available
        /// </summary>
        /// <returns><c>true</c>, if key was hased, <c>false</c> otherwise.</returns>
        /// <param name="context">Context.</param>
        bool HasKey(Context context);
    }
}