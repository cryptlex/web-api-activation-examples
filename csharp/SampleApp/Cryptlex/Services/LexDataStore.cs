using System;
using System.Collections.Generic;

using Cryptlex.Services;

namespace Cryptlex
{
    public class LexDataStore
    {
        public static string AppVersion;
        public static string ClientVersion = "3.0.0-unity";
        public static Dictionary<string, string> MemoryStore = new Dictionary<string, string>();

        public static string GetDataKey(string productId, string key)
        {
            return LexEncryptionService.Sha256(productId + key);
        }

        public static void SaveValue(string productId, string key, string value)
        {
            string dataKey = GetDataKey(productId, key);
            MemoryStore[dataKey] = value;

            // TODO: implement persistant storage
        }

        public static string GetValue(string productId, string key)
        {
            string dataKey = GetDataKey(productId, key);
            string cachedValue;
            MemoryStore.TryGetValue(dataKey, out cachedValue);
            if (!String.IsNullOrEmpty(cachedValue))
            {
                return cachedValue;
            }

            // TODO: implement persistant storage
            return "";

        }

        public static void ResetValue(string productId, string key)
        {
            SaveValue(productId, key, String.Empty);
        }

        public static void Reset(string productId)
        {
            ResetValue(productId, LexConstants.KEY_LICENSE_KEY);
            ResetValue(productId, LexConstants.KEY_ACTIVATION_JWT);
        }
    }
}