// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="ISaltGenerator.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

namespace O8.Mobile.Droid.Vault.Salt
{
    public interface ISaltGenerator
    {
        byte[] CreateSaltBytes(int size);
    }
}