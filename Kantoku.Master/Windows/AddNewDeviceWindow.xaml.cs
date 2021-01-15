﻿using Kantoku.Master.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Kantoku.Master.Windows
{
    /// <summary>
    /// Interaction logic for AddNewDeviceWindow.xaml
    /// </summary>
    public partial class AddNewDeviceWindow : Window
    {
        public AddNewDeviceWindow()
        {
            InitializeComponent();

            this.DataContext = new AddDeviceViewModel();
        }
    }
}
