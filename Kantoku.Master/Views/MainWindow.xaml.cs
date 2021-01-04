using Kantoku.Master.ViewModels;
using System.Windows;

namespace Kantoku.Master
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DashboardViewModel? ViewModel;
        private int SelectionCount;

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(DashboardViewModel viewModel) : this()
        {
            this.DataContext = ViewModel = viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
                await ViewModel.Start();
        }

        private void ContextMenu_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
            if (SelectionCount == 0)
                e.Handled = true;
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SelectionCount += e.AddedItems.Count;
            SelectionCount -= e.RemovedItems.Count;
        }
    }
}
