// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="MemoryOnlyKeyStorage.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Content;
using Java.Lang;
using Javax.Crypto;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Storage
{
    /// <summary>
    ///     Key storage that only stores the key in memory. No key is persisted to storage. This is really only
    ///     useful if the key is provided by the user via PBKDF or from some other secure source.
    /// </summary>
    public class MemoryOnlyKeyStorage : Object, IKeyStorage
    {
        private ISecretKey _secretKey;

        public ISecretKey LoadKey(Context context)
        {
            return _secretKey;
        }

        public bool SaveKey(Context context, ISecretKey secretKey)
        {
            _secretKey = secretKey;
            return true;
        }

        public void ClearKey(Context context)
        {
            _secretKey = null;
        }

        public bool HasKey(Context context)
        {
            return _secretKey != null;
        }

        public KeyStorageType KeyStorageType
        {
            get { return KeyStorageType.NotPersistent; }
        }
    }
}