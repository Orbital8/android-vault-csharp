// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="EncryptionConstants.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Android.Security.Keystore;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     Constant values used by the various encryption methods.
    /// </summary>
    public static class EncryptionConstants
    {
        public const string EncryptionPaddingPkcs5 = "PKCS5Padding";

        /**
	      * While this specifies PKCS5Padding and a 256 bit key, a historical artifact in the Sun encryption
	      * implementation interprets PKCS5 to be PKCS7 for block sizes over 8 bytes. In Android M this
	      * appears to have been corrected so that PKCS7Padding will work when instantiating a Cipher object.
	      */
        public const string AesCbcPaddedTransform = KeyProperties.KeyAlgorithmAes + "/" + KeyProperties.BlockModeCbc + "/" + EncryptionPaddingPkcs5;

        public const string AesCbcPaddedTransformAndroidM =
            KeyProperties.KeyAlgorithmAes + "/" + KeyProperties.BlockModeCbc + "/" + KeyProperties.EncryptionPaddingPkcs7;

        public const int Aes256KeyLengthBits = 256;

        public const string AndroidKeyStore = "AndroidKeyStore";
    }
}