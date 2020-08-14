using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
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
            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
            {
                foreach (PlotPlanItem item in plan)
                {
                    if(item.IsCorrect)
                        try
                        {
                            Plot(item);
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

        private void Plot(PlotPlanItem item)
        {
            AcUIManager.FocusOnFile(item.Sheet.File);
            using (item.Sheet.File.Document.LockDocument())
            using (PlotProgressDialog dialog = new PlotProgressDialog(false, 1, true))
            using (PlotEngine plotEngine = PlotFactory.CreatePublishEngine())
            {
                dialog.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Печать");
                dialog.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Отменить всё");
                dialog.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Отменить лист");
                dialog.OnBeginPlot();
                dialog.IsVisible = true;
                
                plotEngine.BeginPlot(dialog, null);
                Extents2d window = item.Sheet.WindowFrom();

                PlotSettings plotSettingsForSheet = item.Settings.GetSettings();
                
                PlotSettingsValidator validator = PlotSettingsValidator.Current;
                validator.SetPlotWindowArea(plotSettingsForSheet, window);
                validator.SetPlotType(plotSettingsForSheet, PlotType.Window);
                validator.SetStdScaleType(plotSettingsForSheet, StdScaleType.StdScale1To1);
                validator.SetPlotCentered(plotSettingsForSheet, true);
                validator.SetPlotRotation(plotSettingsForSheet, AutoRotation(window));

                PlotInfo plotInfo = GetPlotInfo(item, plotSettingsForSheet);
                
                plotEngine.BeginDocument(
                    plotInfo,
                    item.Sheet.UniqId,
                    null,
                    1,
                    false,
                    null
                );
                
                PlotPageInfo plotPageInfo = new PlotPageInfo();
                
                plotEngine.BeginPage(plotPageInfo, plotInfo, true, null);
                plotEngine.BeginGenerateGraphics(null);
                plotEngine.EndGenerateGraphics(null);
                plotEngine.EndPage(null);
                dialog.OnEndSheet();
                plotEngine.EndDocument(null);
                plotEngine.EndPlot(null);
                dialog.OnEndPlot();
                System.Windows.Forms.Application.DoEvents();

            }
        }

        private PlotRotation AutoRotation(Extents2d window)
        {
            if (window.MaxPoint.X - window.MinPoint.X < window.MaxPoint.Y - window.MinPoint.Y)
                return PlotRotation.Degrees000;
            return PlotRotation.Degrees090;
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
                
                piv.Validate(plotInfo);

                return plotInfo;
            }
        }
        
        
    }
}