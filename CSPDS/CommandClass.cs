using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using CSPDS;
using log4net;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;

//Autodesk

[assembly: CommandClass(typeof(CommandClass))]

namespace CSPDS
{
    /// <summary>
    /// Данный класс содержит методы для непосредственной работы с AutoCAD
    /// </summary>
    public class CommandClass
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static Module module;

        [CommandMethod("ShowCPP")]
        public void ShowPalette()
        {
            try
            {
                if (module is null)
                    module = new Module();

                module.ShowUI();
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }
    }
}