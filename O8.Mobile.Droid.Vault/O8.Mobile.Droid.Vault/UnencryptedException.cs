// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="UnencryptedException.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System;

#endregion

namespace O8.Mobile.Droid.Vault
{
    /// <summary>
    ///     Exception thrown when content that was provided for decryption was not encrypted.
    /// </summary>
    public class UnencryptedException : Exception
    {
        public UnencryptedException()
        {
        }

        public UnencryptedException(string message) : base(message)
        {
        }

        public UnencryptedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnencryptedException(Exception ex) : base("Content is not encrypted", ex)
        {
        }
    }
}