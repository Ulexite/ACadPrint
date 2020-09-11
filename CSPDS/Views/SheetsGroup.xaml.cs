using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CSPDS.ViewModel;

namespace CSPDS.Views
{
    public partial class SheetsGroup : UserControl
    {
        private readonly ObservableCollection<SheetsGroupTreeNode> groups;
        private readonly ModuleUI ui;
        public SheetsGroup(ObservableCollection<SheetsGroupTreeNode> groups, ModuleUI ui)
        {
            InitializeComponent();
            tvrObjects.ItemsSource = groups;
            this.groups = groups;
            this.ui = ui;
        }
        
        private void Plot(object sender, RoutedEventArgs e)
        {
            ui.PlotSelected();
        }
        
    }
}