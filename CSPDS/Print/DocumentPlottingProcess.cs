using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.PlottingServices;

namespace CSPDS
{
    public class DocumentPlottingProcess : IDisposable
    {
        //Disposable delegates:
        private readonly DocumentLock docLock;
        private readonly PlotEngine engine;
        private readonly PlotProgressDialog dialog;
        
        private readonly int pageCount;
        private readonly string plotName;
        
        private int plottedPagesCount = 0;

        public DocumentPlottingProcess(Document document, int pageCount, string plotName)
        {
            this.pageCount = pageCount;
            this.plotName = plotName;
            docLock = document.LockDocument();
            engine = PlotFactory.CreatePublishEngine();
            dialog = new PlotProgressDialog(false, pageCount, true);
            dialog.set_PlotMsgString(PlotMessageIndex.DialogTitle, "Печать "+plotName);
            dialog.set_PlotMsgString(PlotMessageIndex.CancelJobButtonMessage, "Отменить всё");
            dialog.set_PlotMsgString(PlotMessageIndex.CancelSheetButtonMessage, "Отменить лист");
        }

        public void BeginPlot(PlotInfo plotInfo)
        {
            dialog.OnBeginPlot();
            dialog.IsVisible = true;
            engine.BeginPlot(dialog, null);
            engine.BeginDocument(
                plotInfo,
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
            using (PlotPageInfo plotPageInfo = new PlotPageInfo())
            {
                dialog.OnBeginSheet();
                engine.BeginPage(plotPageInfo, info, plottedPagesCount >= pageCount, null);
                engine.BeginGenerateGraphics(null);
                engine.EndGenerateGraphics(null);
                engine.EndPage(null);
                dialog.OnEndSheet();
            }
            System.Windows.Forms.Application.DoEvents();
        }


        public void Dispose()
        {
            try
            {
                docLock.Dispose();
            }
            catch (Exception ignored)
            {
                // ignored
            }

            try
            {
                dialog.Dispose();
            }
            catch (Exception ignored)
            {
                // ignored
            }

            try
            {
                engine.Dispose();
            }
            catch (Exception ignored)
            {
                // ignored
            }
        }
    }
}