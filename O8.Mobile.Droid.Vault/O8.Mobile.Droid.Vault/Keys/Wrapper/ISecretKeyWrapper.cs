// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="ISecretKeyWrapper.cs">
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

namespace O8.Mobile.Droid.Vault.Keys.Wrapper
{
    /// <summary>
    ///     Interface that implementations of secret key wrapping operations must implement.
    /// </summary>
    public interface ISecretKeyWrapper
    {
        KeyStorageType KeyStorageType { get; }

        /// <summary>
        ///     Wrap a {@link javax.crypto.SecretKey} using the public key assigned to this wrapper.
        ///     Use {@link #unwrap(byte[], String)} to later recover the original {@link javax.crypto.SecretKey}.
        /// </summary>
        /// <param name="key">Key.</param>
        byte[] Wrap(ISecretKey key);

        /// <summary>
        ///     Unwrap a {@link javax.crypto.SecretKey} using the private key assigned to this wrapper.
        /// </summary>
        /// <param name="blob">BLOB.</param>
        /// <param name="wrappedKeyAlgorithm">Wrapped key algorithm.</param>
        ISecretKey Unwrap(byte[] blob, string wrappedKeyAlgorithm);

        /// <summary>
        ///     Change key material so that next wrapping will use a different key pair.
        /// </summary>
        /// <param name="context">Context.</param>
        void ClearKey(Context context);
    }
}