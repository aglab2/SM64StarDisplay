using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarDisplay
{
    public abstract class CachedManager
    {
        public bool isInvalidated;
        public virtual void InvalidateCache()
        {
            isInvalidated = true;
        }

        public virtual bool CheckInvalidated()
        {
            if (isInvalidated)
            {
                isInvalidated = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        bool checkChanged<T>(T a, T b)
        {
            return a.Equals(b);
        }
    }
}
