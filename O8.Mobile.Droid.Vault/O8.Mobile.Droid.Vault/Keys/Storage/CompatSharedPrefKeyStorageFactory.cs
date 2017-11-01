// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="CompatSharedPrefKeyStorageFactory.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Content;
using Android.OS;
using Android.Util;
using O8.Mobile.Droid.Vault.Keys.Wrapper;
using O8.Mobile.Droid.Vault.Salt;
using Java.Lang;
using Java.Security;
using Enum = System.Enum;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Storage
{
    /// <summary>
    ///     Create the appropriate version of key storage based on current API version and migrate any previous
    ///     version's keys to the new version.
    /// </summary>
    public class CompatSharedPrefKeyStorageFactory
    {
        private const string Tag = "O8.Mobile.Droid.Vault.Keys.Storage.CompatSharedPrefKeyStorageFactory";
        private const string PrefCompatFactorySdkIntRoot = "compatFactorySdkInt.";
        private const string PrefCompatFactoryAndroidKeystoreTestStateRoot = "androidKeystoreTestState.";

        /// <summary>
        ///     Provided with the SDK version, create or upgrade the best version for the device.
        /// </summary>
        /// <returns>The key storage.</returns>
        /// <param name="context">Context.</param>
        /// <param name="currentSdkInt">Current sdk int.</param>
        /// <param name="prefFileName">Preference file name.</param>
        /// <param name="keystoreAlias">Keystore alias.</param>
        /// <param name="saltIndex">Salt index.</param>
        /// <param name="cipherAlgorithm">Cipher algorithm.</param>
        /// <param name="presharedSecret">Preshared secret.</param>
        /// <param name="saltGenerator">Salt generator.</param>
        public static IKeyStorage CreateKeyStorage(
            Context context, BuildVersionCodes currentSdkInt, string prefFileName, string keystoreAlias, int saltIndex, string cipherAlgorithm,
            string presharedSecret, ISaltGenerator saltGenerator)
        {
            IKeyStorage result = null;
            var oldSdkInt = ReadOldSdkInt(context, prefFileName, keystoreAlias);

            //Check to see if we have crossed an upgrade boundary and attempt to upgrade if so.
            if (DoesRequireKeyUpgrade(oldSdkInt, currentSdkInt))
            {
                try
                {
                    result = UpgradeKeyStorage(
                        context, oldSdkInt, currentSdkInt, prefFileName, keystoreAlias, saltIndex, cipherAlgorithm, presharedSecret, saltGenerator);
                }
                catch (GeneralSecurityException e)
                {
                    Log.Error(Tag, "Upgrade resulted in an exception", e);
                    result = null;
                }
            }

            //Upgrade failed or was unnecessary, get the latest appropriate version of the KeyStorage.
            if (result == null)
            {
                result = CreateVersionAppropriateKeyStorage(
                    context, currentSdkInt, prefFileName, keystoreAlias, saltIndex, cipherAlgorithm, presharedSecret, saltGenerator);
            }

            if (result != null)
            {
                WriteCurrentSdkInt(context, currentSdkInt, prefFileName, keystoreAlias);
            }

            return result;
        }

        private static IKeyStorage UpgradeKeyStorage(
            Context context, BuildVersionCodes oldSdkInt, BuildVersionCodes currentSdkInt, string prefFileName, string keystoreAlias, int saltIndex,
            string cipherAlgorithm, string presharedSecret, ISaltGenerator saltGenerator)
        {
            var oldKeyStorage = CreateVersionAppropriateKeyStorage(
                context, oldSdkInt, prefFileName, keystoreAlias, saltIndex, cipherAlgorithm, presharedSecret, saltGenerator);
            var secretKey = oldKeyStorage.LoadKey(context);

            if (secretKey != null)
            {
                var newKeyStorage = CreateVersionAppropriateKeyStorage(
                    context, currentSdkInt, prefFileName, keystoreAlias, saltIndex, cipherAlgorithm, presharedSecret, saltGenerator);
                if (newKeyStorage.SaveKey(context, secretKey))
                {
                    return newKeyStorage;
                }
            }

            return null;
        }

        private static IKeyStorage CreateVersionAppropriateKeyStorage(
            Context context, BuildVersionCodes currentSdkInt, string prefFileName, string keystoreAlias, int saltIndex, string cipherAlgorithm,
            string presharedSecret, ISaltGenerator saltGenerator)
        {
            ISecretKeyWrapper secretKeyWrapper = null;

            if (currentSdkInt >= BuildVersionCodes.JellyBeanMr2 && !BadHardware.IsBadHardware() &&
                CanUseAndroidKeystore(context, prefFileName, keystoreAlias, currentSdkInt))
            {
                secretKeyWrapper = new AndroidKeystoreSecretKeyWrapper(context, keystoreAlias);
            }
            else
            {
                secretKeyWrapper = new ObfuscatingSecretKeyWrapper(context, saltIndex, saltGenerator, presharedSecret);
            }
            return new SharedPrefKeyStorage(secretKeyWrapper, prefFileName, keystoreAlias, cipherAlgorithm);
        }

        private static bool CanUseAndroidKeystore(Context context, string prefFileName, string keystoreAlias, BuildVersionCodes currentSdkInt)
        {
            var androidKeystoreTestState = ReadAndroidKeystoreTestState(context, prefFileName, keystoreAlias);
            if (AndroidKeystoreTestState.Untested.Equals(androidKeystoreTestState))
            {
                androidKeystoreTestState = PerformAndroidKeystoreTest(context, keystoreAlias, currentSdkInt);
                WriteAndroidKeystoreTestState(context, prefFileName, keystoreAlias, androidKeystoreTestState);
            }

            return AndroidKeystoreTestState.Pass.Equals(androidKeystoreTestState);
        }

        private static AndroidKeystoreTestState PerformAndroidKeystoreTest(Context context, string keystoreAlias, BuildVersionCodes currentSdkInt)
        {
            var androidKeystoreTestState = AndroidKeystoreTestState.Fail;

            if (currentSdkInt >= BuildVersionCodes.JellyBeanMr2)
            {
                try
                {
                    var androidKeystoreSecretKeyWrapper = new AndroidKeystoreSecretKeyWrapper(context, keystoreAlias);
                    androidKeystoreTestState = androidKeystoreSecretKeyWrapper.TestKey ? AndroidKeystoreTestState.Pass : AndroidKeystoreTestState.Fail;
                }
                catch (Throwable t)
                {
                    Log.Error(Tag, "Caught an exception while creating the AndroidKeystoreSecretKeyWrapper", t);
                    androidKeystoreTestState = AndroidKeystoreTestState.Fail;
                }
            }

            if (AndroidKeystoreTestState.Fail.Equals(androidKeystoreTestState))
            {
                Log.Warn(Tag, "This device failed the AndroidKeystoreSecretKeyWrapper test.");
            }

            return androidKeystoreTestState;
        }

        private static string GetAndroidKeystoreTestStateSharedPreferenceKey(string keystoreAlias)
        {
            return PrefCompatFactoryAndroidKeystoreTestStateRoot + keystoreAlias;
        }

        private static void WriteAndroidKeystoreTestState(
            Context context, string prefFileName, string keystoreAlias, AndroidKeystoreTestState androidKeystoreTestState)
        {
            var testState = Enum.GetName(typeof(AndroidKeystoreTestState), androidKeystoreTestState);

            GetSharedPreferences(context, prefFileName).Edit().PutString(GetAndroidKeystoreTestStateSharedPreferenceKey(keystoreAlias), testState).Apply();
        }

        private static AndroidKeystoreTestState ReadAndroidKeystoreTestState(Context context, string prefFileName, string keystoreAlias)
        {
            var testState = Enum.GetName(typeof(AndroidKeystoreTestState), AndroidKeystoreTestState.Untested);
            var prefValue = GetSharedPreferences(context, prefFileName).GetString(GetAndroidKeystoreTestStateSharedPreferenceKey(keystoreAlias), testState);

            AndroidKeystoreTestState androidKeystoreTestState;

            try
            {
                AndroidKeystoreTestState enumValue;
                if (Enum.TryParse(prefValue, out enumValue))
                {
                    androidKeystoreTestState = enumValue;
                }
                else
                {
                    androidKeystoreTestState = AndroidKeystoreTestState.Untested;
                }
            }
            catch (IllegalArgumentException e)
            {
                Log.Error(Tag, "Failed to parse previous test state", e);
                androidKeystoreTestState = AndroidKeystoreTestState.Untested;
            }

            return androidKeystoreTestState;
        }

        private static string GetCurrentSdkIntSharedPreferenceKey(string keystoreAlias)
        {
            return PrefCompatFactorySdkIntRoot + keystoreAlias;
        }

        private static void WriteCurrentSdkInt(Context context, BuildVersionCodes currentSdkInt, string prefFileName, string keystoreAlias)
        {
            var editor = GetSharedPreferences(context, prefFileName).Edit();
            editor.PutInt(GetCurrentSdkIntSharedPreferenceKey(keystoreAlias), (int)currentSdkInt);
            editor.Apply();
        }

        private static BuildVersionCodes ReadOldSdkInt(Context context, string prefFileName, string keystoreAlias)
        {
            var sharedPreferences = GetSharedPreferences(context, prefFileName);
            var intValue = sharedPreferences.GetInt(GetCurrentSdkIntSharedPreferenceKey(keystoreAlias), 0);
            return (BuildVersionCodes)intValue;
        }

        /// <summary>
        ///     Determine if the device has just had the OS upgraded across the JELLY_BEAN_MR2 barrier.
        /// </summary>
        /// <returns>The require key upgrade.</returns>
        /// <param name="oldSdkInt">Old sdk int.</param>
        /// <param name="currentSdkInt">Current sdk int.</param>
        private static bool DoesRequireKeyUpgrade(BuildVersionCodes oldSdkInt, BuildVersionCodes currentSdkInt)
        {
            return oldSdkInt > 0 && oldSdkInt < currentSdkInt && oldSdkInt < BuildVersionCodes.JellyBeanMr2 && currentSdkInt >= BuildVersionCodes.JellyBeanMr2 &&
                   !BadHardware.IsBadHardware();
        }

        /// <summary>
        ///     Return shared preference file to use for encrypted key storage
        /// </summary>
        /// <returns>The shared preferences.</returns>
        /// <param name="context">Context.</param>
        /// <param name="prefFileName">Preference file name.</param>
        protected static ISharedPreferences GetSharedPreferences(Context context, string prefFileName)
        {
            return context.GetSharedPreferences(prefFileName, FileCreationMode.Private);
        }
    }
}