// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="StandardSharedPreferenceVaultEditor.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System.Collections.Generic;
using Android.Content;
using Java.Lang;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     Editor implementation for StandardSharedPreferenceVault
    /// </summary>
    public class StandardSharedPreferenceVaultEditor : Object, ISharedPreferencesEditor
    {
        private readonly StandardSharedPreferenceVault _standardSharedPreferenceVault;
        private bool _cleared;
        private readonly List<string> _removalSet = new List<string>();
        private readonly StronglyTypedBundle _stronglyTypedBundle = new StronglyTypedBundle();

        public StandardSharedPreferenceVaultEditor(StandardSharedPreferenceVault standardSharedPreferenceVault)
        {
            _standardSharedPreferenceVault = standardSharedPreferenceVault;
        }

        public void Apply()
        {
            _standardSharedPreferenceVault.WriteValues(false, _cleared, _removalSet, _stronglyTypedBundle);
        }

        public ISharedPreferencesEditor Clear()
        {
            _cleared = true;
            return this;
        }

        public bool Commit()
        {
            return _standardSharedPreferenceVault.WriteValues(true, _cleared, _removalSet, _stronglyTypedBundle);
        }

        public ISharedPreferencesEditor PutBoolean(string key, bool value)
        {
            _stronglyTypedBundle.PutValue(key, value);
            return this;
        }

        public ISharedPreferencesEditor PutFloat(string key, float value)
        {
            _stronglyTypedBundle.PutValue(key, value);
            return this;
        }

        public ISharedPreferencesEditor PutInt(string key, int value)
        {
            _stronglyTypedBundle.PutValue(key, value);
            return this;
        }

        public ISharedPreferencesEditor PutLong(string key, long value)
        {
            _stronglyTypedBundle.PutValue(key, value);
            return this;
        }

        public ISharedPreferencesEditor PutString(string key, string value)
        {
            _stronglyTypedBundle.PutValue(key, value);
            return this;
        }

        public ISharedPreferencesEditor PutStringSet(string key, ICollection<string> values)
        {
            _stronglyTypedBundle.PutValue(key, values);
            return this;
        }

        public ISharedPreferencesEditor Remove(string key)
        {
            _stronglyTypedBundle.Remove(key);
            _removalSet.Add(key);
            return this;
        }
    }
}