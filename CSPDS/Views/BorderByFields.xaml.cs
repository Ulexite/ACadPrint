using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using log4net;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;


namespace CSPDS.Views
{
    public partial class BorderByFiles : UserControl
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private PrintManager printManager;
        private SheetCollector collector;

        public BorderByFiles(PrintManager printManager, SheetCollector collector)
        {
            InitializeComponent();
            tvrObjects.ItemsSource = collector.ByFiles;
            this.printManager = printManager;
            this.collector = collector;
        }

        private void Plot(object sender, RoutedEventArgs e)
        {
            try
            {
                printManager.PrintSelected();
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }
            
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            try
            {
                collector.Refresh(Application.DocumentManager);
            }
            catch (Exception exception)
            {
                _log.Error(exception);
            }
        }

        private void OnSelect(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            if (tvrObjects.SelectedItem != null && tvrObjects.SelectedItem is SheetDescriptor)
            {
                try
                {
                    SheetDescriptor sd = ((SheetDescriptor) tvrObjects.SelectedItem);
                    FileDescriptor fd = sd.File;
                    if (!fd.Document.IsDisposed)
                    {
                        if (!fd.Document.IsActive)
                        {
                            DocumentCollection docMgr = Application.DocumentManager;
                            if (!docMgr.DocumentActivationEnabled)
                                docMgr.DocumentActivationEnabled = true;

                            docMgr.MdiActiveDocument = fd.Document;
                        }

                        Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
                        ed.SetImpliedSelection(new[] {sd.BorderEntity});
                    }
                }
                catch (Exception exception)
                {
                    _log.Error(exception);
                }
            }
        }
    }
}