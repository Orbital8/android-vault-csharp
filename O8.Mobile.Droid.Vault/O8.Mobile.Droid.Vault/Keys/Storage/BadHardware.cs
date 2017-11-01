// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="BadHardware.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System.Collections.Generic;
using Android.OS;

#endregion

namespace O8.Mobile.Droid.Vault.Keys.Storage
{
    public class BadHardware
    {
        private static readonly List<string> BadHardwareModels = new List<string>();

        static BadHardware()
        {
            BadHardwareModels.Add("SGH-T889"); //Galaxy Note 2 nukes hardware keystore on PIN unlock.
        }

        public static bool IsBadHardware()
        {
            return BadHardwareModels.Contains(Build.Model);
        }
    }
}