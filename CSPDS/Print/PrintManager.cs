using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Windows.Data;
using CSPDS.Views;
using log4net;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace CSPDS
{
    public class PrintManager
    {
        private SheetCollector sheetCollector;

        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PrintManager(SheetCollector sheetCollector)
        {
            this.sheetCollector = sheetCollector;
        }

        public void PrintSelected()
        {
            IEnumerable<FileDescriptor> files = sheetCollector.ByFiles;
            List<PlotPlanItem> plan = PreparePlan(files);
            PlotPlanPreview preview = new PlotPlanPreview(plan, this);
            Application.ShowModalWindow(preview);
        }

        private List<PlotPlanItem> PreparePlan(IEnumerable<FileDescriptor> files)
        {
            _log.Debug("Prepare Plan");
            List<PlotPlanItem> ret = new List<PlotPlanItem>();

            foreach (FileDescriptor file in files)
            {
                foreach (SheetDescriptor sheet in file.Sheets)
                {    
                    if (sheet.IsChecked)
                    {
                        PlotSettingsDescriptor settings = sheetCollector.DescriptorFor(sheet);
                        if (!(settings is null))
                        {
                            ret.Add(new PlotPlanItem(sheet, settings));
                            _log.DebugFormat("В план печати: {0}", sheet.UniqId);
                        }
                        else
                        {
                            ret.Add(new PlotPlanItem("Не заданы настройки печати"));
                            _log.ErrorFormat("Не заданы настройки печати {0}", sheet.UniqId);
                        }
                    }
                }
            }

            return ret;
        }

        public void PlotByPlan(List<PlotPlanItem> plan, string pathToPdf)
        {
            Plotter plotter = new Plotter();
            plotter.Plot(plan);
        }
    }

    
    public class PlotPlanItem
    {
        private string wrong;
        private SheetDescriptor sheet;
        private PlotSettingsDescriptor settings;

        private bool isCorrect;

        public bool IsCorrect => isCorrect;
        public bool IsWrong => !isCorrect;

        public SheetDescriptor Sheet => sheet;

        public PlotSettingsDescriptor Settings => settings;

        public string Wrong => wrong;

        public PlotPlanItem(SheetDescriptor sheet, PlotSettingsDescriptor settings)
        {
            this.sheet = sheet;
            this.settings = settings;
            this.wrong = "";
            this.isCorrect = true;
        }

        public PlotPlanItem(string whatWrong)
        {
            wrong = whatWrong;
            this.isCorrect = false;
        }
    }
}