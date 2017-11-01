// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="PrngSaltGenerator.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using Java.Security;

#endregion

namespace O8.Mobile.Droid.Vault.Salt
{
    public class PrngSaltGenerator : ISaltGenerator
    {
        private readonly SecureRandom _secureRandom;

        public PrngSaltGenerator()
        {
            _secureRandom = new SecureRandom();
        }

        public byte[] CreateSaltBytes(int size)
        {
            var result = new byte[size];
            _secureRandom.NextBytes(result);
            return result;
        }
    }
}