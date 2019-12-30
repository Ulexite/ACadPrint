using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using CSPDS;
using CSPDS.Views;
using log4net;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
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

        private static PaletteSet mainPalette;

        private static SheetCollector sheetCollector = new SheetCollector();

        [CommandMethod("ShowCPP")]
        public void ShowPalette()
        {
            try
            {
                LogInitInformation();
                sheetCollector.Refresh(Application.DocumentManager);
                if (mainPalette is null)
                {
                    mainPalette = new PaletteSet("Менеджер печати ЦПП");
                    mainPalette.DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right);
                    
                    var byFiles = new BorderByFiles(sheetCollector.ByFiles);
                    //var byFormats = new BorderByFiles(sheetCollector.ByFormats);
                    var formatSettings = new FormatPlotSettings(sheetCollector.Formats);
                    var settingsList = new PlotSettingsList(sheetCollector.Settings);

                    ElementHost filesHost = new ElementHost();
                    filesHost.Dock = DockStyle.Fill;
                    filesHost.Child = byFiles;
                    filesHost.AutoSize = true;
                    
                    ElementHost formatsHost = new ElementHost();
                    formatsHost.Dock = DockStyle.Fill;
                    formatsHost.Child = formatSettings;
                    formatsHost.AutoSize = true;
                    
                    ElementHost settingsHost = new ElementHost();
                    settingsHost.Dock = DockStyle.Fill;
                    settingsHost.Child = settingsList;
                    settingsHost.AutoSize = true;
                    
                    mainPalette.Add("Листы", filesHost);
                    mainPalette.Add("Настройки по форматам", formatsHost);
                    mainPalette.Add("Настройки", settingsHost);
                }

                mainPalette.KeepFocus = true;
                mainPalette.Visible = true;
            }
            catch (Exception e)
            {
                _log.Error(e);
            }
        }

        private static void LogInitInformation()
        {
            _log.Debug(Application.Version.ToString());
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}