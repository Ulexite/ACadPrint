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

        public BorderByFiles(ObservableCollection<FileDescriptor> files)
        {
            InitializeComponent();
            tvrObjects.ItemsSource = files;
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