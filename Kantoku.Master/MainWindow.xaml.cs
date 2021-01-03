using Kantoku.Master.ViewModels;
using System.Windows;

namespace Kantoku.Master
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DashboardViewModel? ViewModel;

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
    }
}
