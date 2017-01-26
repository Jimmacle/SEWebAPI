using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEWA
{
    public class ValueBag
    {
        private readonly Dictionary<string, string> _bag = new Dictionary<string, string>();

        public void Add(string key, string value)
        {
            _bag.Add(key, value);
        }

        public string Get(string key)
        {
            return _bag.ContainsKey(key) ? _bag[key] : null;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (!_bag.ContainsKey(key))
            {
                value = default(T);
                return false;
            }

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                value = (T)converter.ConvertFromString(_bag[key]);
                return true;
            }
            catch (NotSupportedException)
            {
                value = default(T);
                return false;
            }
        }
    }
}
