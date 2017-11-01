// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="StandardSharedPreferenceVault.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Android.Content;
using O8.Mobile.Droid.Vault.Keys.Storage;
using Java.IO;
using Java.Lang;
using Java.Security;
using Javax.Crypto;
using Exception = System.Exception;
using Object = Java.Lang.Object;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     SecureVault backed by a SharedPreference file.
    /// </summary>
    public class StandardSharedPreferenceVault : Object, ISharedPreferenceVault
    {
        private const string Tag = "O8.Mobile.Droid.Vault.StandardSharedPreferenceVault";
        private const string StringSetSeparator = "1eRHtJaybutdAsFp2DkfrT1FqMJlLfT7DdgCpQtTaoQWheoeFBZRqt5pgFDH7Cf";

        private readonly Context _context;
        private readonly bool _enableExceptions;
        private readonly IKeyStorage _keyStorage;

        private readonly List<ISharedPreferencesOnSharedPreferenceChangeListener> _sharedPreferenceChangeListenerList =
            new List<ISharedPreferencesOnSharedPreferenceChangeListener>();

        private readonly string _sharedPreferenceName;
        private readonly string _transform;
        private ISharedPreferences _sharedPreferences;

        public StandardSharedPreferenceVault(Context context, IKeyStorage keyStorage, string prefFileName, string transform, bool enableExceptions)
        {
            _context = context.ApplicationContext;
            _keyStorage = keyStorage;
            _sharedPreferenceName = prefFileName;
            _transform = transform;
            _enableExceptions = enableExceptions;
        }

        public bool IsDebugEnabled { get; set; }

        public bool IsKeyAvailable
        {
            get { return _keyStorage.HasKey(_context); }
        }

        public KeyStorageType KeyStorageType
        {
            get { return _keyStorage.KeyStorageType; }
        }

        public IDictionary<string, object> All
        {
            get
            {
                var resultMap = new Dictionary<string, object>();
                var secretKey = _keyStorage.LoadKey(_context);

                if (secretKey != null)
                {
                    try
                    {
                        var sharedPreferences = GetSharedPreferences();
                        var sourceMap = sharedPreferences.All;

                        foreach (var key in sourceMap.Keys)
                        {
                            var value = GetString(key, null, secretKey);
                            if (value != null)
                            {
                                float floatValue;
                                long longValue;
                                bool boolValue;

                                if (value.Contains(".") && float.TryParse(value, out floatValue))
                                {
                                    resultMap.Add(key, floatValue);
                                }
                                else if (long.TryParse(value, out longValue))
                                {
                                    if (longValue <= int.MaxValue && longValue >= int.MinValue)
                                    {
                                        resultMap.Add(key, Convert.ToInt32(longValue));
                                    }
                                    else
                                    {
                                        resultMap.Add(key, longValue);
                                    }
                                }
                                else if (bool.TryParse(value, out boolValue))
                                {
                                    resultMap.Add(key, boolValue);
                                }
                                else if (value.Contains(StringSetSeparator))
                                {
                                    var split = value.Split(new[] { StringSetSeparator }, StringSplitOptions.None);
                                    var collection = new List<string>(split);
                                    resultMap.Add(key, collection);
                                }
                                else
                                {
                                    resultMap.Add(key, value);
                                }
                            }
                        }
                    }
                    catch (UnsupportedEncodingException ex)
                    {
                        Log("Exception in getAll()", ex);
                        if (_enableExceptions)
                        {
                            throw new RuntimeException(ex);
                        }
                    }
                    catch (UnencryptedException ex)
                    {
                        Log("Exception in getAll()", ex);
                        if (_enableExceptions)
                        {
                            throw;
                        }
                    }
                    catch (GeneralSecurityException ex)
                    {
                        Log("Exception in getAll()", ex);
                        if (_enableExceptions)
                        {
                            throw new RuntimeException(ex);
                        }
                    }
                }

                return resultMap;
            }
        }

        public void ClearStorage()
        {
            GetSharedPreferences().Edit().Clear().Apply();
            _keyStorage.ClearKey(_context);
        }

        public void RekeyStorage(ISecretKey secretKey)
        {
            ClearStorage();
            SetKey(secretKey);
        }

        public void SetKey(ISecretKey secretKey)
        {
            _keyStorage.SaveKey(_context, secretKey);
        }

        public bool Contains(string key)
        {
            return GetSharedPreferences().Contains(key);
        }

        public ISharedPreferencesEditor Edit()
        {
            return new StandardSharedPreferenceVaultEditor(this);
        }

        public bool GetBoolean(string key, bool defValue)
        {
            var result = defValue;
            var stringValue = GetString(key, null);

            bool parsed;
            if (stringValue != null && bool.TryParse(stringValue, out parsed))
            {
                result = parsed;
            }

            return result;
        }

        public float GetFloat(string key, float defValue)
        {
            var result = defValue;
            var stringValue = GetString(key, null);

            float parsed;
            if (stringValue != null && float.TryParse(stringValue, out parsed))
            {
                result = parsed;
            }

            return result;
        }

        public int GetInt(string key, int defValue)
        {
            var result = defValue;
            var stringValue = GetString(key, null);

            int parsed;
            if (stringValue != null && int.TryParse(stringValue, out parsed))
            {
                result = parsed;
            }

            return result;
        }

        public long GetLong(string key, long defValue)
        {
            var result = defValue;
            var stringValue = GetString(key, null);

            long parsed;
            if (stringValue != null && long.TryParse(stringValue, out parsed))
            {
                result = parsed;
            }

            return result;
        }

        public string GetString(string key, string defaultValue)
        {
            try
            {
                return GetString(key, defaultValue, _keyStorage.LoadKey(_context));
            }
            catch (UnsupportedEncodingException ex)
            {
                Log("Exception in getString()", ex);
                if (_enableExceptions)
                {
                    throw new RuntimeException(ex);
                }
            }
            catch (UnencryptedException ex)
            {
                Log("Exception in getString()", ex);
                if (_enableExceptions)
                {
                    throw;
                }
            }
            catch (GeneralSecurityException ex)
            {
                Log("Exception in getString()", ex);
                if (_enableExceptions)
                {
                    throw new RuntimeException(ex);
                }
            }

            return defaultValue;
        }

        public ICollection<string> GetStringSet(string key, ICollection<string> defValues)
        {
            var result = defValues;
            var joinedString = GetString(key, null);

            if (joinedString != null)
            {
                result = SplitStringSet(joinedString);
            }

            return result;
        }

        public void RegisterOnSharedPreferenceChangeListener(ISharedPreferencesOnSharedPreferenceChangeListener listener)
        {
            _sharedPreferenceChangeListenerList.Add(listener);
        }

        public void UnregisterOnSharedPreferenceChangeListener(ISharedPreferencesOnSharedPreferenceChangeListener listener)
        {
            _sharedPreferenceChangeListenerList.Remove(listener);
        }

        internal bool WriteValues(bool commit, bool wasCleared, IList<string> removalSet, StronglyTypedBundle stronglyTypedBundle)
        {
            var editor = GetSharedPreferences().Edit();
            var preferenceKeySet = new List<string>();

            if (wasCleared)
            {
                editor.Clear();
            }

            if (removalSet != null)
            {
                foreach (var key in removalSet)
                {
                    editor.Remove(key);
                }

                preferenceKeySet.AddRange(removalSet);
            }

            if (stronglyTypedBundle != null)
            {
                // Secret key is kept in memory only long enough to use it.
                var secretKey = _keyStorage.LoadKey(_context);
                if (secretKey != null)
                {
                    try
                    {
                        foreach (var key in stronglyTypedBundle.KeySet)
                        {
                            var type = stronglyTypedBundle.TypeForValue(key);

                            if (type == typeof(string))
                            {
                                WriteString(editor, key, secretKey, stronglyTypedBundle.Value<string>(key));
                            }
                            else if (type == typeof(long))
                            {
                                WriteLong(editor, key, secretKey, stronglyTypedBundle.Value<long>(key));
                            }
                            else if (type == typeof(int))
                            {
                                WriteInteger(editor, key, secretKey, stronglyTypedBundle.Value<int>(key));
                            }
                            else if (type == typeof(float))
                            {
                                WriteFloat(editor, key, secretKey, stronglyTypedBundle.Value<float>(key));
                            }
                            else if (type == typeof(bool))
                            {
                                WriteBoolean(editor, key, secretKey, stronglyTypedBundle.Value<bool>(key));
                            }
                            else if (typeof(IEnumerable<string>).IsAssignableFrom(type))
                            {
                                try
                                {
                                    //noinspection unchecked
                                    WriteStringSet(editor, key, secretKey, stronglyTypedBundle.Value<IEnumerable<string>>(key));
                                }
                                catch (InvalidCastException ex)
                                {
                                    Log("Unexpected type of set provided", ex);
                                    return false;
                                }
                            }
                            else
                            {
                                Log("Unexpected data type encountered " + type);
                                return false;
                            }
                        }
                    }
                    catch (UnsupportedEncodingException ex)
                    {
                        Log("Exception in writeValues()", ex);
                        if (_enableExceptions)
                        {
                            throw new RuntimeException(ex);
                        }

                        return false;
                    }
                    catch (GeneralSecurityException ex)
                    {
                        Log("Exception in writeValues()", ex);
                        if (_enableExceptions)
                        {
                            throw new RuntimeException(ex);
                        }

                        return false;
                    }
                }
                else
                {
                    return false;
                }

                preferenceKeySet.AddRange(stronglyTypedBundle.KeySet);
            }
            else
            {
                return false;
            }

            var commitSuccess = true;
            if (commit)
            {
                commitSuccess = editor.Commit();
            }
            else
            {
                editor.Apply();
            }

            if (commitSuccess)
            {
                NotifyListeners(preferenceKeySet);
            }

            return commitSuccess;
        }

        private string GetString(string key, string defaultValue, ISecretKey secretKey)
        {
            var result = defaultValue;
            var rawValue = GetSharedPreferences().GetString(key, null);

            if (rawValue != null && secretKey != null)
            {
                result = StringEncryptionUtils.Decrypt(secretKey, rawValue, Encoding.UTF8, _transform);
            }

            return result;
        }

        private void WriteStringSet(ISharedPreferencesEditor editor, string key, ISecretKey secretKey, IEnumerable<string> value)
        {
            WriteString(editor, key, secretKey, string.Join(StringSetSeparator, value));
        }

        private void WriteBoolean(ISharedPreferencesEditor editor, string key, ISecretKey secretKey, bool value)
        {
            WriteString(editor, key, secretKey, Convert.ToString(value));
        }

        private void WriteFloat(ISharedPreferencesEditor editor, string key, ISecretKey secretKey, float value)
        {
            WriteString(editor, key, secretKey, Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        private void WriteInteger(ISharedPreferencesEditor editor, string key, ISecretKey secretKey, int value)
        {
            WriteString(editor, key, secretKey, Convert.ToString(value));
        }

        private void WriteLong(ISharedPreferencesEditor editor, string key, ISecretKey secretKey, long value)
        {
            WriteString(editor, key, secretKey, Convert.ToString(value));
        }

        private void WriteString(ISharedPreferencesEditor editor, string key, ISecretKey secretKey, string value)
        {
            editor.PutString(key, StringEncryptionUtils.Encrypt(secretKey, value, Encoding.UTF8, _transform));
        }

        private void NotifyListeners(IEnumerable<string> preferenceKeySet)
        {
            var preferenceKeys = preferenceKeySet as string[] ?? preferenceKeySet.ToArray();
            foreach (var listener in _sharedPreferenceChangeListenerList)
            {
                foreach (var preferenceKey in preferenceKeys)
                {
                    listener.OnSharedPreferenceChanged(this, preferenceKey);
                }
            }
        }

        private ISharedPreferences GetSharedPreferences()
        {
            if (_sharedPreferences == null)
            {
                if (string.IsNullOrEmpty(_sharedPreferenceName))
                {
                    throw new IllegalStateException("Cannot open preferences before setting SharedPreferenceFileName");
                }

                _sharedPreferences = _context.GetSharedPreferences(_sharedPreferenceName, FileCreationMode.Private);
            }

            return _sharedPreferences;
        }

        private ICollection<string> SplitStringSet(string joinedString)
        {
            var splits = joinedString.Split(new[] { StringSetSeparator }, StringSplitOptions.None);
            return splits;
        }

        private void Log(string message)
        {
            if (IsDebugEnabled)
            {
                Android.Util.Log.Error(Tag, message);
            }
        }

        private void Log(string message, Throwable e)
        {
            if (IsDebugEnabled)
            {
                Android.Util.Log.Error(Tag, message, e);
            }
        }

        private void Log(string message, Exception e)
        {
            if (IsDebugEnabled)
            {
                Android.Util.Log.Error(Tag, message, e);
            }
        }
    }
}