// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Orbital 8 Pty Ltd" file="StronglyTypedBundle.cs">
//   Copyright (c) 2017 Orbital 8 Pty Ltd. All rights reserved.
// </copyright>
// <datecreated>2016-09-20</datecreated>
// --------------------------------------------------------------------------------------------------------------------
//
// Ported to Xamarin Android / C# from https://github.com/BottleRocketStudios/Android-Vault 
// developed by Bottle Rocket LLC (http://www.bottlerocketstudios.com/).

#region Namespaces

using System;
using System.Collections.Generic;

#endregion

namespace O8.Mobile.Droid.Vault
{
    public class StronglyTypedBundle
    {
        private readonly IDictionary<string, Type> _classMap = new Dictionary<string, Type>();
        private readonly IDictionary<string, object> _valueMap = new Dictionary<string, object>();

        public ICollection<string> KeySet => _valueMap.Keys;

        public Type TypeForValue(string key)
        {
            return _classMap[key];
        }

        public T Value<T>(string key)
        {
            return (T)_valueMap[key];
        }

        public void PutValue(string key, object value)
        {
            if (_valueMap.ContainsKey(key))
            {
                _valueMap[key] = value;
            }
            else
            {
                _valueMap.Add(key, value);
            }

            if (_classMap.ContainsKey(key))
            {
                _classMap[key] = value.GetType();
            }
            else
            {
                _classMap.Add(key, value.GetType());
            }
        }

        public void Remove(string key)
        {
            _valueMap.Remove(key);
            _classMap.Remove(key);
        }
    }
}