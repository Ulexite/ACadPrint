using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using log4net;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace CSPDS
{
    public class Plotter
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public void Plot(List<PlotPlanItem> plan)
        {
            List<List<PlotPlanItem>> byDocuments = SortByDocuments(plan);
            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
            {
                foreach (List<PlotPlanItem> documentSheets in byDocuments)
                {
                    try
                    {
                        PlotDocument(documentSheets);
                    }
                    catch (ThreadAbortException fatal)
                    {
                        _log.Fatal(fatal);
                    }
                    catch (Exception exceeption)
                    {
                        _log.Error(exceeption);
                    }
                }
            }
        }

        private void PlotDocument(List<PlotPlanItem> documentSheets)
        {
            if (documentSheets.Count <= 0)
                return;
            AcUIManager.FocusOnFile(documentSheets[0].Sheet.File);
            Document document = documentSheets[0].Sheet.File.Document;
            string plottingName = documentSheets[0].Sheet.File.Name;
            using (DocumentPlottingProcess plotting =
                new DocumentPlottingProcess(document, documentSheets.Count, plottingName))
            {
                PlotSettings firstSheetSettings = PlotSettingsForSheet(documentSheets[0].Sheet, documentSheets[0].Settings);
                PlotInfo firstSheetPlotInfo = GetPlotInfo(documentSheets[0].Sheet.Db, firstSheetSettings);
            
                plotting.BeginPlot(firstSheetPlotInfo);

                foreach (PlotPlanItem item in documentSheets)
                {
                    PlotSettings settings = PlotSettingsForSheet(item.Sheet, item.Settings);
                    PlotInfo info = GetPlotInfo(item.Sheet.Db, settings);
                    plotting.PlotPage(info,settings);
                }
                
                plotting.EndPlot();                
            }
        }

        private PlotSettings PlotSettingsForSheet(SheetDescriptor sheet, PlotSettingsDescriptor settings)
        {
            PlotSettings plotSettingsForSheet = settings.GetSettings();
            Extents2d window = sheet.WindowFrom();
            
            PlotSettingsValidator validator = PlotSettingsValidator.Current;
            validator.SetPlotWindowArea(plotSettingsForSheet, window);
            validator.SetPlotType(plotSettingsForSheet, PlotType.Window);
            validator.SetStdScaleType(plotSettingsForSheet, StdScaleType.StdScale1To1);
            validator.SetPlotCentered(plotSettingsForSheet, true);
            validator.SetPlotRotation(plotSettingsForSheet, AutoRotation(window));
            return plotSettingsForSheet;
        }

        private List<List<PlotPlanItem>> SortByDocuments(List<PlotPlanItem> plan)
        {
            Dictionary<string, List<PlotPlanItem>> byDocuments = new Dictionary<string, List<PlotPlanItem>>();
            foreach (PlotPlanItem item in plan)
            {
                if (item.IsCorrect)
                {
                    if (!byDocuments.ContainsKey(item.Sheet.File.Name))
                        byDocuments.Add(item.Sheet.File.Name, new List<PlotPlanItem>());

                    byDocuments[item.Sheet.File.Name].Add(item);
                }
            }

            return new List<List<PlotPlanItem>>(byDocuments.Values);
        }

        private PlotRotation AutoRotation(Extents2d window)
        {
            if (window.MaxPoint.X - window.MinPoint.X < window.MaxPoint.Y - window.MinPoint.Y)
                return PlotRotation.Degrees000;
            return PlotRotation.Degrees090;
        }

        private PlotInfo GetPlotInfo(Database db, PlotSettings plotSettingsForSheet)
        {
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

                piv.Validate(plotInfo);

                return plotInfo;
            }
        }
    }
}