using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using CSPDS;
using CSPDS.Views;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

[assembly: ExtensionApplication(typeof(Initializator))]

namespace CSPDS
{

    
    public class Initializator : IExtensionApplication
    {
        public void Initialize()
        {
        }

        public void Terminate()
        {
        }
    }
}