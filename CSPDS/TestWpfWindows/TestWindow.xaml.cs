/*
 * User: aleksey.nakoryakov
 * Date: 01.08.12
 * Time: 13:37
 */

using System.Windows;

namespace CSPDS.TestWpfWindows
{
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
        }

        public string UserName => nameBox.Text;

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}