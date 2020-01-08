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
    public partial class FormatPlotSettings : UserControl
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private PrintManager printManager;
        private SheetCollector collector;

        public FormatPlotSettings(PrintManager printManager, SheetCollector collector)
        {
            InitializeComponent();
            this.printManager = printManager;
            this.collector = collector;            
            dgFormats.ItemsSource = collector.Formats;
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

   }
}