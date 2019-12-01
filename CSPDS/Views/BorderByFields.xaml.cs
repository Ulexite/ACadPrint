using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace CSPDS.Views
{
    public partial class BorderByFiles : UserControl
    {
        public BorderByFiles(ObservableCollection<FileDescriptor> files)
        {
            InitializeComponent();
            tvrObjects.ItemsSource = files;
        }
    }
}