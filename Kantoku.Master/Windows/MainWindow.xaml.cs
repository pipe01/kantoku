using Kantoku.Master.ViewModels;
using Kantoku.Master.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Kantoku.Master
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Func<AddNewDeviceWindow>? AddNewDeviceWindow;

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(DashboardViewModel viewModel, Func<AddNewDeviceWindow> addNewDeviceWindow) : this()
        {
            this.DataContext = viewModel;
            this.AddNewDeviceWindow = addNewDeviceWindow;
        }

        private void AddNewDevice_Click(object sender, RoutedEventArgs e)
        {
            if (AddNewDeviceWindow == null)
                return;

            var dialog = AddNewDeviceWindow();
            dialog.Owner = this;
            dialog.ShowDialog();
        }
    }
}
