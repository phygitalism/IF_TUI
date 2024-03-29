﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarkerRegistratorGui.View;
using MarkerRegistratorGui.ViewModel;

namespace MarkerRegistratorGui
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly PointerInjector _pointerInjector;

		public MainWindow()
		{
			InitializeDataContext();
			InitializeComponent();

			_pointerInjector = new PointerInjector(this, ((MainViewModel)DataContext).Pointers);

			Closed += (_, __) => _pointerInjector.Dispose();
		}

		private void InitializeDataContext()
		{
			try
			{
				DataContext = new MainViewModel();
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, "Initialization error", MessageBoxButton.OK, MessageBoxImage.Error);
				throw;
			}
		}

		private void CallButtonCommand(object sender, TouchEventArgs e)
		{
			var button = sender as Button;
			button.Command.Execute(button.CommandParameter);
		}
	}
}
