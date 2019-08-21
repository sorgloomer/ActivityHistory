using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityHistory
{
    public static class Refs
    {
        public static void Dispose(IDisposable obj)
        {
            if (obj != null)
                obj.Dispose();
        }

        public static void Set<T>(ref T refObj, T newValue) where T : IDisposable
        {
            Dispose(refObj);
            refObj = newValue;
        }
        public static void Fire(Action action)
        {
            if (action != null)
                action.Invoke();
        }
    }
}
