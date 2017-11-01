// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="SpecificSaltGenerator.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Java.Lang;
using Java.Util;

#endregion

namespace O8.Mobile.Droid.Vault.Salt
{
    /// <summary>
    ///     Provides a predefined salt. Primarily useful for using a pre-calculated salt for a PBKDF.
    /// </summary>
    public class SpecificSaltGenerator : ISaltGenerator
    {
        private readonly byte[] _saltBytes;

        public SpecificSaltGenerator(byte[] saltBytes)
        {
            _saltBytes = saltBytes;
        }

        public byte[] CreateSaltBytes(int size)
        {
            if (size > _saltBytes.Length)
            {
                throw new IndexOutOfBoundsException("Requested salt size exceeds amount available");
            }

            return Arrays.CopyOf(_saltBytes, size);
        }
    }
}