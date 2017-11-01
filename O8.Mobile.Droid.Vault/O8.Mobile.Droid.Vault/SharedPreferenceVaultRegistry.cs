// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="SharedPreferenceVaultRegistry.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System.Collections.Generic;
using Android.Util;
using Java.Lang;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     Ensure app-wide uniqueness of vault indices and key alias and reducing memory churn on object instantiation
    ///     and intermediate steps.Also ensures that vaults are single instance to avoid issues when the repository
    ///     has a key change or clearing event on a separate thread.
    ///     This should be populated once in the Application Object's onCreate().
    ///     Indices do not need to be consecutive, but they must be unique across the application and consistent
    ///     across upgrades.
    /// </summary>
    public class SharedPreferenceVaultRegistry
    {
        private static SharedPreferenceVaultRegistry _instance;
        private readonly IList<string> _keyAliasSet;
        private readonly IList<string> _prefFileSet;

        private readonly SparseArray<ISharedPreferenceVault> _sharedPreferenceVaultArray;

        private SharedPreferenceVaultRegistry()
        {
            _sharedPreferenceVaultArray = new SparseArray<ISharedPreferenceVault>();
            _keyAliasSet = new List<string>();
            _prefFileSet = new List<string>();
        }

        /// <summary>
        ///     Get the instance or create it.
        /// </summary>
        /// <value>The instance.</value>
        public static SharedPreferenceVaultRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SharedPreferenceVaultRegistry();
                }

                return _instance;
            }
        }

        public void AddVault(int index, string prefFileName, string keyAlias, ISharedPreferenceVault vault)
        {
            if (_prefFileSet.Contains(prefFileName))
            {
                throw new IllegalArgumentException("Only one vault per application can use the same preference file.");
            }

            if (_keyAliasSet.Contains(keyAlias))
            {
                throw new IllegalArgumentException("Only one vault per application can use the same KeyAlias.");
            }

            if (_sharedPreferenceVaultArray.Get(index) != null)
            {
                throw new IllegalArgumentException("Only one vault per application can use the same index.");
            }

            ReplaceVault(index, prefFileName, keyAlias, vault);
        }

        public void ReplaceVault(int index, string prefFileName, string keyAlias, ISharedPreferenceVault vault)
        {
            _prefFileSet.Add(prefFileName);
            _keyAliasSet.Add(keyAlias);
            _sharedPreferenceVaultArray.Put(index, vault);
        }

        public ISharedPreferenceVault GetVault(int index)
        {
            return _sharedPreferenceVaultArray.Get(index);
        }

        public void Clear()
        {
            _prefFileSet.Clear();
            _keyAliasSet.Clear();
            _sharedPreferenceVaultArray.Clear();
        }
    }
}