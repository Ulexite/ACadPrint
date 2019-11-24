using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CSPDS.Views
{
    public partial class BorderByFiles : UserControl
    {
        public BorderByFiles(List<FileDescriptor> files)
        {
            tvrObjects.ItemsSource = files;
            InitializeComponent();
        }
    }
}