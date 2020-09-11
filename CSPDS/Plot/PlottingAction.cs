using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using CSPDS.Actors;
using CSPDS.Model;
using CSPDS.Utils;
using log4net;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace CSPDS
{
    public class PlottingAction
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ModuleUI ui;

        //TODO: Configuration?
        private readonly GroupsSorter<PlotTask> sorter = new GroupsSorter<PlotTask>("fileName");

        private readonly DestinationPlotSettingsExtractor
            plotSettingsExtractor = new DestinationPlotSettingsExtractor();

        public ModuleUI Ui
        {
            get => ui;
            set => ui = value;
        }

        public void Plot(PlotPlan plan)
        {
            var byDocuments = sorter.Group(plan.TaskList);
            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
            {
                foreach (var documentSheets in byDocuments)
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

        private void PlotDocument(KeyValuePair<string, List<PlotTask>> documentTasks)
        {
            if (documentTasks.Value.Count <= 0)
                return;

            Document document = documentTasks.Value[0].Sheet.Document;
            ui.FocusOnFile(document);
            using (DocumentPlottingProcess plotting =
                new DocumentPlottingProcess(document, documentTasks.Value.Count, documentTasks.Key))
            {
                PlotSettings firstSheetSettings =
                    PlotSettingsForSheet(documentTasks.Value[0].Sheet, documentTasks.Value[0].Destination);
                PlotInfo firstSheetPlotInfo = GetPlotInfo(document.Database, firstSheetSettings);

                plotting.BeginPlot(firstSheetPlotInfo);

                foreach (PlotTask task in documentTasks.Value)
                {
                    PlotSettings settings = PlotSettingsForSheet(task.Sheet, task.Destination);
                    PlotInfo info = GetPlotInfo(task.Sheet.Document.Database, settings);
                    plotting.PlotPage(info, settings);
                }

                plotting.EndPlot();
            }
        }

        private PlotSettings PlotSettingsForSheet(Sheet sheet, Destination destination)
        {
            PlotSettings plotSettingsForSheet = plotSettingsExtractor.SettingsFor(destination);
            Extents2d window = sheet.Bounds;

            PlotSettingsValidator validator = PlotSettingsValidator.Current;
            validator.SetPlotWindowArea(plotSettingsForSheet, window);
            validator.SetPlotType(plotSettingsForSheet, PlotType.Window);
            validator.SetStdScaleType(plotSettingsForSheet, StdScaleType.StdScale1To1);
            validator.SetPlotCentered(plotSettingsForSheet, true);
            //TODO: Вынести в Actor
            validator.SetPlotRotation(plotSettingsForSheet, AutoRotation(window));
            //TODO: validator.RefreshLists(plotSettingsForSheet);

            return plotSettingsForSheet;
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