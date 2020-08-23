using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;
using log4net;

namespace CSPDS
{
    public class DocumentPlottingProcess : IDisposable
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //Disposable delegates:
        private readonly DocumentLock docLock;
        private readonly PlotEngine engine;
        private readonly PlotProgressDialog dialog;
        private readonly SystemVariableShader backgroundPlot;

        private readonly int pageCount;
        private readonly string plotName;

        private int plottedPagesCount = 0;

        public DocumentPlottingProcess(Document document, int pageCount, string plotName)
        {
            this.pageCount = pageCount;
            this.plotName = plotName;

            backgroundPlot = new SystemVariableShader("BACKGROUNDPLOT", 0);
            docLock = document.LockDocument();
            engine = PlotFactory.CreatePublishEngine();
            dialog = new PlotProgressDialog(false, pageCount, true);
        }

        public void BeginPlot(PlotInfo info)
        {
            _log.DebugFormat("Печать документа {0}, {1} листов", plotName, pageCount);

            dialog.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Печать " + plotName);
            dialog.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Отменить всё");
            dialog.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Отменить лист");
            dialog.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Set Progress");
            dialog.set_PlotMsgString(PlotMessageIndex.SheetSetProgressCaption, "Sheet Progress");
            dialog.LowerPlotProgressRange = 0;
            dialog.UpperPlotProgressRange = 100;
            dialog.PlotProgressPos = 0;
            dialog.OnBeginPlot();
            dialog.IsVisible = true;

            engine.BeginPlot(dialog, null);
            engine.BeginDocument(
                info,
                plotName,
                null,
                1,
                false,
                null
            );
            System.Windows.Forms.Application.DoEvents();
        }

        public void EndPlot()
        {
            engine.EndDocument(null);
            engine.EndPlot(null);
            dialog.OnEndPlot();
            System.Windows.Forms.Application.DoEvents();
        }

        public void PlotPage(PlotInfo info, PlotSettings settings)
        {
            plottedPagesCount++;
            dialog.PlotProgressPos = plottedPagesCount * 100 / pageCount;

            using (PlotPageInfo plotPageInfo = new PlotPageInfo())
            {
                dialog.StatusMsgString = String.Format("Печать {0}: страница {1} из {2}", plotName, plottedPagesCount,
                    pageCount);
                dialog.OnBeginSheet();
                dialog.SheetProgressPos = 0;
                dialog.LowerSheetProgressRange = 0;
                dialog.UpperSheetProgressRange = 100;
                engine.BeginPage(plotPageInfo, info, plottedPagesCount >= pageCount, null);
                engine.BeginGenerateGraphics(null);

                dialog.UpperSheetProgressRange = 50;
                System.Windows.Forms.Application.DoEvents();

                engine.EndGenerateGraphics(null);
                engine.EndPage(null);
                dialog.UpperSheetProgressRange = 100;
                dialog.OnEndSheet();
            }
        }


        public void Dispose()
        {
            try
            {
                docLock.Dispose();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }

            try
            {
                dialog.Dispose();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }

            try
            {
                engine.Dispose();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }
            try
            {
                backgroundPlot.Dispose();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }
        }
    }
}