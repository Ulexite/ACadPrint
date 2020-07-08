using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace CSPDS
{
    public class Plotter
    {
        public void Plot(List<PlotPlanItem> plan)
        {
            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
            {
                foreach (PlotPlanItem item in plan)
                {
                    if(item.IsCorrect)
                     Plot(item);
                }
            }
        }

        private void Plot(PlotPlanItem item)
        {
            AcUIManager.FocusOnFile(item.Sheet.File);
            
            using (PlotEngine plotEngine = PlotFactory.CreatePublishEngine())
            {
                plotEngine.BeginPlot(null, null);
                Extents2d window = item.Sheet.WindowFrom();

                PlotSettings plotSettingsForSheet = item.Settings.GetSettings();
                
                PlotSettingsValidator validator = PlotSettingsValidator.Current;
                validator.SetPlotWindowArea(plotSettingsForSheet, window);
                validator.SetPlotType(plotSettingsForSheet, PlotType.Window);
                validator.SetStdScaleType(plotSettingsForSheet, StdScaleType.StdScale1To1);
                validator.SetPlotCentered(plotSettingsForSheet, true);

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
                plotEngine.EndDocument(null);
                
                System.Windows.Forms.Application.DoEvents();

            }
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