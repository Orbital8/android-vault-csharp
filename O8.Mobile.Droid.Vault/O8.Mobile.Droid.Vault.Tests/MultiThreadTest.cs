// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="MultiThreadTest.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------

#region Namespaces

using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using NUnit.Framework;

#endregion

namespace O8.Mobile.Droid.Vault.Tests
{
    /// <summary>
    ///     Test with a lot of concurrent threads.
    /// </summary>
    [TestFixture]
    public class MultiThreadTest
    {
        private const string KeyFileName = "multiThreadKeyFile";
        private const string PrefFileName = "multiThreadPrefFile";
        private const string KeyAlias1 = "multiThreadKeyAlias";
        private const int KeyIndex1 = 1;
        private const string PresharedSecret1 = "a;sdl564546a6s6w2828d4fsdfbsijd;saj;9dj9";

        private const string TestKey = "testKey";
        private const string TestValue = "testValue";
        private const int NumberOfSimultaneousThreads = 60;
        private const int NumberOfIterations = 10;

        [Test]
        public async Task TestWithManyThreadsAsync()
        {
            var context = Application.Context;
            var sharedPreferenceVault = SharedPreferenceVaultFactory.GetAppKeyedCompatAes256Vault(
                context, PrefFileName, KeyFileName, KeyAlias1, KeyIndex1, PresharedSecret1);

            for (var testIteration = 0; testIteration < NumberOfIterations; testIteration++)
            {
                sharedPreferenceVault.Edit().PutString(TestKey, TestValue).Apply();

                var resultFutureList = new List<Task<string>>(NumberOfSimultaneousThreads);
                for (var i = 0; i < NumberOfSimultaneousThreads; i++)
                {
                    resultFutureList.Add(Task.Run(() => sharedPreferenceVault.GetString(TestKey, null)));
                }

                await Task.WhenAll(resultFutureList);

                foreach (var task in resultFutureList)
                {
                    Assert.That(task.Result, Is.EqualTo(TestValue));
                }
            }
        }
    }
}