// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="SaltBox.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System;
using Android.Content;
using Android.Util;
using Java.Lang;

#endregion

namespace O8.Mobile.Droid.Vault.Salt
{
    /// <summary>
    ///     Disk and in-memory cache of random salt used to aid obfuscated storage on older devices.
    /// </summary>
    public class SaltBox
    {
        private const string Tag = "O8.Mobile.Droid.Vault.Salt.SaltBox";
        private const string SettingNameFormat = "NaCl-{0}";
        private const string DefaultSharedPrefFile = "NaCl";

        private static readonly SparseArray<byte[]> _storedBits = new SparseArray<byte[]>();

        /// <summary>
        ///     This method will use the default shared preference file. Not recommended for external use.
        /// </summary>
        /// <returns>The stored bits.</returns>
        /// <param name="context">Context.</param>
        /// <param name="saltIndex">Salt index.</param>
        /// <param name="requestedSize">Requested size.</param>
        public static byte[] GetStoredBits(Context context, int saltIndex, int requestedSize)
        {
            return GetStoredBits(context, saltIndex, requestedSize, DefaultSharedPrefFile);
        }

        /// <summary>
        ///     Return previously stored byte array. If the default constructor is used, care must be taken
        ///     to avoid collision with Vault/Key indices with the saltIndex value.In other words, if you
        ///     use this class directly external to the factories provided by the Vault library, use a
        ///     different constructor.
        /// </summary>
        /// <returns>The stored bits.</returns>
        /// <param name="context">Context.</param>
        /// <param name="saltIndex">Salt index.</param>
        /// <param name="requestedSize">Requested size.</param>
        /// <param name="sharedPreferenceFileName">Shared preference file name.</param>
        public static byte[] GetStoredBits(Context context, int saltIndex, int requestedSize, string sharedPreferenceFileName)
        {
            var settingName = GetSettingName(saltIndex);
            var storedBits = GetStoredBitsCache(saltIndex);

            if (IsByteArrayInvalid(storedBits, requestedSize))
            {
                //Try to load existing
                storedBits = LoadStoredBitsFromPreferences(context, settingName, requestedSize, sharedPreferenceFileName);
                SetStoredBitsCache(saltIndex, storedBits);
            }

            return storedBits;
        }

        private static string GetSettingName(int saltIndex)
        {
            return string.Format(SettingNameFormat, saltIndex);
        }

        /// <summary>
        ///     This method will use the default shared preference file. Not recommended for external use.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="saltIndex">Salt index.</param>
        /// <param name="storedBits">Stored bits.</param>
        /// <param name="requestedSize">Requested size.</param>
        public static void WriteStoredBits(Context context, int saltIndex, byte[] storedBits, int requestedSize)
        {
            WriteStoredBits(context, saltIndex, storedBits, requestedSize, DefaultSharedPrefFile);
        }

        /// <summary>
        ///     Write a byte array to storage. If the default constructor is used, care must be taken
        ///     to avoid collision with Vault/Key indices with the saltIndex value.In other words, if you
        ///     use this class directly external to the factories provided by the Vault library, use a
        ///     different sharedPrefFileName.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="saltIndex">Salt index.</param>
        /// <param name="storedBits">Stored bits.</param>
        /// <param name="requestedSize">Requested size.</param>
        /// <param name="sharedPrefFileName">Shared preference file name.</param>
        public static void WriteStoredBits(Context context, int saltIndex, byte[] storedBits, int requestedSize, string sharedPrefFileName)
        {
            SaveStoredBitsToPreferences(context, requestedSize, GetSettingName(saltIndex), storedBits, sharedPrefFileName);
            if (IsByteArrayInvalid(storedBits, requestedSize))
            {
                SetStoredBitsCache(saltIndex, null);
            }
            else
            {
                SetStoredBitsCache(saltIndex, storedBits);
            }
        }

        private static bool IsByteArrayInvalid(byte[] storedBits, int requestedSize)
        {
            return storedBits == null || storedBits.Length != requestedSize;
        }

        private static void SaveStoredBitsToPreferences(Context context, int requestedSize, string settingName, byte[] storedBits, string sharedPrefFileName)
        {
            var sharedPrefsEditor = GetSharedPreferences(context, sharedPrefFileName).Edit();
            if (IsByteArrayInvalid(storedBits, requestedSize))
            {
                sharedPrefsEditor.Remove(settingName);
            }
            else
            {
                var base64 = Convert.ToBase64String(storedBits);
                sharedPrefsEditor.PutString(settingName, base64);
            }

            sharedPrefsEditor.Apply();
        }

        private static byte[] LoadStoredBitsFromPreferences(Context context, string settingName, int requestedSize, string sharedPrefFileName)
        {
            var sharedPrefs = GetSharedPreferences(context, sharedPrefFileName);
            var base64 = sharedPrefs.GetString(settingName, null);
            if (base64 != null)
            {
                try
                {
                    var storedBits = Convert.FromBase64String(base64);
                    if (IsByteArrayInvalid(storedBits, requestedSize))
                    {
                        return null;
                    }
                    return storedBits;
                }
                catch (IllegalArgumentException ex)
                {
                    Log.Warn(Tag, "Stored bits were not properly encoded", ex);
                }
            }

            return null;
        }

        private static ISharedPreferences GetSharedPreferences(Context context, string sharedPrefFileName)
        {
            return context.GetSharedPreferences(sharedPrefFileName, FileCreationMode.Private);
        }

        private static byte[] GetStoredBitsCache(int saltIndex)
        {
            return _storedBits.Get(saltIndex);
        }

        private static void SetStoredBitsCache(int saltIndex, byte[] storedBits)
        {
            _storedBits.Put(saltIndex, storedBits);
        }
    }
}