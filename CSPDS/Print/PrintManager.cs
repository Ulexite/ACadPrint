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
            short bgPlot = (short) Application.GetSystemVariable("BACKGROUNDPLOT");
            Application.SetSystemVariable("BACKGROUNDPLOT", 1);

            try
            {
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
            }
            finally
            {
                Application.SetSystemVariable("BACKGROUNDPLOT", bgPlot);
            }

            return ret;
        }

        public void PlotByPlan(List<PlotPlanItem> plan, string pathToPdf)
        {
            Plotter plotter = new Plotter();
            plotter.Plot(plan);
        }

        private void BeginDocument(PlotEngine plotEngine, PlotInfo plotInfo, PlotPlanItem planItem, string pathToPdf)
        {
            plotEngine.BeginDocument(
                plotInfo,
                planItem.Sheet.File.Name,
                null,
                1,
                false,
                pathToPdf
            );
        }

        private PlotInfo GetPlotInfo(PlotPlanItem planItem, PlotSettings plotSettingsForSheet)
        {
            Database db = planItem.Sheet.Db;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr =
                    (BlockTableRecord) tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                Layout layout = (Layout) tr.GetObject(btr.LayoutId, OpenMode.ForRead);
                PlotInfo plotInfo = new PlotInfo();
                plotInfo.Layout = btr.LayoutId;
                PlotInfoValidator piv = new PlotInfoValidator();
                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;

                plotInfo.OverrideSettings = plotSettingsForSheet;
                try
                {
                    piv.Validate(plotInfo);
                }
                catch (Exception e)
                {
                    _log.Error(e);
                    throw e;
                }

                return plotInfo;
            }
        }


        private PlotSettings GetPlotSettings(PlotPlanItem planItem)
        {
            _log.Debug("GetPlotSettings");
            //Настройки могут быть не в том файле, в котором лист
            FileDescriptor file = planItem.Settings.File;
            Database db = file.Document.Database;
            PlotSettings plotSettingsForSheet = new PlotSettings(true);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary settingsDict =
                    (DBDictionary) tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead);
                plotSettingsForSheet.CopyFrom(tr.GetObject((ObjectId) settingsDict.GetAt(planItem.Settings.Name),
                    OpenMode.ForRead));
                return plotSettingsForSheet;
            }
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