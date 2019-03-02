using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Utility.Interfaces
{
    public interface IUtilityCache
    {
        void Add<T>(string key, T value);

        bool TryGet<T>(string key, out T value);

        void Clear();

        string GenerateKey(Type classType, string methodName, params float[] inputs);
    }
}
