using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Windows;
using log4net;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Window = System.Windows.Window;


namespace CSPDS.Views
{
    public partial class PlotPlanPreview : Window
    {
        private readonly List<PlotPlanItem> _plan;
        private readonly PrintManager _printManager;

        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PlotPlanPreview(List<PlotPlanItem> plan, PrintManager printManager)
        {
            _plan = plan;
            _printManager = printManager;
            InitializeComponent();
            this.plan.ItemsSource = plan;
        }

        private void PlotPlan(object sender, RoutedEventArgs e)
        {
            _printManager.PlotByPlan(_plan, null);
        }
        private void PlotPlanToFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog("Печать в файл", "default.pdf","pdf", "Печать в файл", SaveFileDialog.SaveFileDialogFlags.AllowAnyExtension);
            var dr = sd.ShowDialog();
            _printManager.PlotByPlan(_plan, sd.Filename);
        }

    }
}