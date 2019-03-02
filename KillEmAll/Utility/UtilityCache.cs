using KillEmAll.Utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Utility
{
    public class UtilityCache : IUtilityCache
    {
        private const int ROUNDING_PRECICION = 3;

        private Dictionary<string, object> _cache = new Dictionary<string, object>();

        public void Add<T>(string key, T value)
        {
            if (key == null)
                return;

            _cache.Add(key, value);

            //Console.WriteLine($"{GetType().FullName}: STORED IN CACHE: {key} -> {value}");
        }

        public void Clear()
        {
            _cache.Clear();
            //Console.WriteLine($"{GetType().FullName}: CACHE CLEARED");
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (key == null)
                Console.WriteLine($"{GetType().FullName}: Cache key cannot be null!");

            if (_cache.TryGetValue(key, out object result))
            {
                if (result is T)
                {
                    value = (T)result;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public string GenerateKey(Type classType, string methodName, params float[] inputs)
        {
            var key =  $"{classType.FullName}.{methodName}::";

            var sb = new StringBuilder(key);
            for (var i = 0; i < inputs.Length; i++)
            {
                sb.Append(',');
                sb.Append(inputs[i]);
            }

            return sb.ToString();
        }
    }
}
