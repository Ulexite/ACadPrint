using System.Windows;
using System.Windows.Controls;
using CSPDS.ViewModel;

namespace CSPDS.Views
{
    public partial class FormatDestinationView : UserControl
    {
        private Formats formats;
        private readonly ModuleUI ui;
        public FormatDestinationView(Formats formats, ModuleUI ui)
        {
            this.formats = formats;
            this.ui = ui;
            InitializeComponent();
            dgFormats.ItemsSource = formats.FormatDestinations;
        }
        
        private void Plot(object sender, RoutedEventArgs e)
        {
            ui.PlotSelected();
        }
        
    }
}