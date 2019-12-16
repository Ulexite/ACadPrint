using Autodesk.AutoCAD.DatabaseServices;

namespace CSPDS
{
    public class PrintManager
    {
        public void PrintAll(FileDescriptor file) {
            PlotSettingsValidator.Current;
        } 
    }
}