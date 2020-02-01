﻿using System;
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
    public partial class PlotSettingsList : UserControl
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PlotSettingsList(ObservableCollection<PlotSettingsDescriptor> settings)
        {
            InitializeComponent();
            dgSettings.ItemsSource = settings;
        }

   }
}