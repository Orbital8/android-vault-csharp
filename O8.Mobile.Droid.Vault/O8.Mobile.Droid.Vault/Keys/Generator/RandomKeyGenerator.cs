// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="RandomKeyGenerator.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using O8.Mobile.Droid.Vault.Salt;
using Javax.Crypto;
using Javax.Crypto.Spec;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Generator
{
    /// <summary>
    ///     Generate a random key using the supplied random source.
    /// </summary>
    public class RandomKeyGenerator
    {
        private readonly int _keyLengthBits;
        private readonly ISaltGenerator _saltGenerator;

        public RandomKeyGenerator(ISaltGenerator saltGenerator, int keyLengthBits)
        {
            _saltGenerator = saltGenerator;
            _keyLengthBits = keyLengthBits;
        }

        public ISecretKey GenerateKey(string cipher)
        {
            return new SecretKeySpec(_saltGenerator.CreateSaltBytes(_keyLengthBits / 8), cipher);
        }
    }
}