using System;
using Autodesk.AutoCAD.ApplicationServices.Core;

namespace CSPDS
{
    public class SystemVariableShader : IDisposable
    {
        private readonly string name;
        private readonly object oldValue;

        public SystemVariableShader(string name, object value)
        {
            
            this.name = name;
            this.oldValue = Application.GetSystemVariable(name);
            Application.SetSystemVariable(name, value);
        }

        public void Dispose()
        {
            Application.SetSystemVariable(name, oldValue);
        }
    }
}