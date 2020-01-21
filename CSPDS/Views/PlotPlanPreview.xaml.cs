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
    public partial class PlotPlanPreview : Window
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PlotPlanPreview(List<PlotPlanItem> plan)
        {
            InitializeComponent();
            this.plan.ItemsSource = plan;
        }

        private void PlotPlan(object sender, RoutedEventArgs e)
        {
        
        }
        private void PlotPlanToFile(object sender, RoutedEventArgs e)
        {
        }

    }
}