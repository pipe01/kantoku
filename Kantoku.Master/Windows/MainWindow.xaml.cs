using Kantoku.Master.ViewModels;
using Kantoku.Master.Windows;
using System.Windows;
using System.Windows.Controls;

namespace Kantoku.Master
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int SelectionCount;

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(DashboardViewModel viewModel) : this()
        {
            this.DataContext = viewModel;
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (SelectionCount == 0)
                e.Handled = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionCount += e.AddedItems.Count;
            SelectionCount -= e.RemovedItems.Count;
        }

        private void AddNewDevice_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNewDeviceWindow();
            dialog.Owner = this;
            dialog.ShowDialog();
        }
    }
}
