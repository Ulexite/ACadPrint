using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.PlottingServices;
using Autodesk.AutoCAD.Windows.Data;
using log4net;
using PlotType = Autodesk.AutoCAD.DatabaseServices.PlotType;

namespace CSPDS
{
    public class PrintManager
    {
        [DllImport("accore.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTrans")]
        static extern int acedTrans(
            double[] point,
            IntPtr fromRb,
            IntPtr toRb,
            int disp,
            double[] result
        );

        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

/*        public void PdfAll(FileDescriptor file)
        {
            _log.DebugFormat("PdfAll {0}", file.Name);
            if (PlotFactory.ProcessPlotState == ProcessPlotState.NotPlotting)
            {
                Database db = file.Db;

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    using (PlotEngine plotEngine = PlotFactory.CreatePublishEngine())
                    {
                        using (PlotProgressDialog dialog = new PlotProgressDialog(false, file.Sheets.Count, true))
                        {
                            _log.Debug("Init Dialog");

                            dialog.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Печать " + file.Name);
                            dialog.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Отменить всё");
                            dialog.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Отменить лист");

                            dialog.OnBeginPlot();

                            dialog.IsVisible = true;

                            plotEngine.BeginPlot(dialog, null);


                            int count = file.Sheets.Count;
                            bool isFirst = true;
                            foreach (SheetDescriptor border in file.Sheets)
                            {
                                count--;
                                _log.DebugFormat("Border Num {0}", count);

                                _log.Debug("Get window");
                                Extents2d window = WindowFrom(border, db, tr);
                                _log.DebugFormat("window: {0}-{1}",window.MinPoint,window.MaxPoint);
                                
                                dialog.OnBeginSheet();

                                _log.DebugFormat("StartForBorder {0}", border.SheetNumber);
                                PlotSettings plotSettingsForSheet = PlotSettingsFor(border, db, tr);

                                _log.Debug("PlotSettingsValidator");
                                PlotSettingsValidator validator = PlotSettingsValidator.Current;

                                _log.Debug("SetPlotWindowArea");
                                validator.SetPlotWindowArea(plotSettingsForSheet, window);
                                _log.Debug("SetPlotType");
                                validator.SetPlotType(plotSettingsForSheet, PlotType.Window);
                                _log.Debug("SetPlotType");
                                validator.SetStdScaleType(plotSettingsForSheet, StdScaleType.ScaleToFit);
                                validator.SetPlotCentered(plotSettingsForSheet, true);

                                _log.Debug("Plotinfo for PDF");
                                BlockTableRecord btr =
                                    (BlockTableRecord) tr.GetObject(db.CurrentSpaceId, OpenMode.ForRead);
                                Layout layout = (Layout) tr.GetObject(btr.LayoutId, OpenMode.ForRead);
                                PlotInfo plotInfo = new PlotInfo();
                                plotInfo.Layout = btr.LayoutId;
                                PlotInfoValidator piv = new PlotInfoValidator();
                                piv.MediaMatchingPolicy = MatchingPolicy.MatchEnabled;
                                plotInfo.OverrideSettings = plotSettingsForSheet;

                                piv.Validate(plotInfo);

                                if (isFirst)
                                {
                                    isFirst = false;
                                    plotEngine.BeginDocument(
                                        plotInfo,
                                        file.Name,
                                        null,
                                        1,
                                        true,
                                        file.Name + ".pdf"
                                    );
                                }

                                PlotPageInfo plotPageInfo = new PlotPageInfo();
                                plotEngine.BeginPage(plotPageInfo, plotInfo, count <= 0, null);
                                plotEngine.BeginGenerateGraphics(null);
                                plotEngine.EndGenerateGraphics(null);
                                plotEngine.EndPage(null);
                                dialog.OnEndSheet();
                                System.Windows.Forms.Application.DoEvents();
                            }

                            plotEngine.EndDocument(null);
                            dialog.OnEndPlot();
                            plotEngine.EndPlot(null);
                        }
                    }
                }
            }
        }
*/
        private Extents2d WindowFrom(SheetDescriptor sheet, Database db, Transaction tr)
        {
            _log.Debug("Try to take window");
            Curve curve = (Curve) tr.GetObject(sheet.BorderEntity, OpenMode.ForRead);
            _log.Debug("Take bounds");
            Extents3d bounds = curve.Bounds.Value;
            _log.Debug("Take points");
            Point3d first = bounds.MinPoint;
            Point3d second = bounds.MaxPoint;
            _log.DebugFormat("First {0}", first);
            _log.DebugFormat("First {0}", second);

            ResultBuffer rbFrom = new ResultBuffer(new TypedValue(5003, 1));
            ResultBuffer rbTo = new ResultBuffer(new TypedValue(5003, 2));

            double[] firres = new double[] {0,0,0};
            double[] secres = new double[] {0,0,0};

            acedTrans(first.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, firres);
            acedTrans(second.ToArray(), rbFrom.UnmanagedObject, rbTo.UnmanagedObject, 0, secres);
            
            var ret = new Extents2d(
                firres[0],
                firres[1],
                secres[0],
                secres[1]
            );

            _log.DebugFormat("Extents2d", ret);
            return ret;
        }

        private PlotSettings PlotSettingsFor(SheetDescriptor sheet, Database db, Transaction tr)
        {
            _log.DebugFormat("Get plot settings for {0}", sheet.Format);

            DBDictionary  settingsDict =
            (DBDictionary) tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead);

            PlotSettings ret = new PlotSettings(true);
            ret.CopyFrom(tr.GetObject((ObjectId) settingsDict.GetAt("1_@PlotSPDSModel"), OpenMode.ForRead));
            return ret;
        }
    }
}