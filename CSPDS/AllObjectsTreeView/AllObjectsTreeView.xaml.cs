using System.Windows;

namespace CSPDS.AllObjectsTreeView
{
    public partial class AllObjectsView : Window
    {
        public AllObjectsView(ObjectDescriptionNode root)
        {
            InitializeComponent();
            tvrObjects.ItemsSource = root.InnerObjects;
        }
    }
}