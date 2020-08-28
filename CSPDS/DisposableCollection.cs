using System;
using System.Collections.Generic;

namespace CSPDS
{
    public class DisposableCollection: IDisposable
    {
        private HashSet<IDisposable> dgts = new HashSet<IDisposable>();
        
        public void Add(IDisposable dgt)
        {
            dgts.Add(dgt);
        }
        public void Dispose()
        {
            foreach (var dgt in dgts)
            {
                try
                {
                    dgt.Dispose();
                }
                catch (Exception ignored)
                {
                    //ignored
                }
            }
        }
    }
}