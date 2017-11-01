// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="MainActivity.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------

#region Namespaces

using Android.App;
using Android.OS;
using NUnit.Runner;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

#endregion

namespace O8.Mobile.Droid.Vault.Tests
{
    [Activity(Label = "Vault Tests", MainLauncher = true)]
    public class MainActivity : FormsApplicationActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Forms.Init(this, savedInstanceState);

            // This will load all tests within the current project
            var nunit = new App { AutoRun = true };

            // If you want to add tests in another assembly
            //nunit.AddTestAssembly(typeof(MyTests).Assembly);

            LoadApplication(nunit);
        }
    }
}