using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    [Serializable]
    public class SettingsManager
    {
        Dictionary<string, object> configs;

        public SettingsManager()
        {
            configs = new Dictionary<string, object>();
        }

        public T GetConfig<T>(string configureName, T def)
        {
            T ret = def;
            try
            {
                if (configs.TryGetValue(configureName, out object value))
                {
                    ret = (T)value;
                }
                else
                {
                    configs[configureName] = ret;
                }
            }
            catch (Exception)
            {
                configs[configureName] = ret;
            }

            return ret;
        }

        public void SetConfig<T>(string configureName, T value)
        {
            configs[configureName] = value;
        }

        public bool isValid()
        {
            return configs != null;
        }
    }
}
