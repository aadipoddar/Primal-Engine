﻿using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

using PrimalEditor.Content;
using PrimalEditor.GameProject;

namespace PrimalEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public static string PrimalPath { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			Loaded += OnMainWindowLoaded;
			Closing += OnMainWindowClosing;
		}


		private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= OnMainWindowLoaded;
			GetEnginePath();
			OpenProjectBrowserDialogue();
		}

		private void GetEnginePath()
		{
			var primalPath = Environment.GetEnvironmentVariable("PRIMAL_ENGINE", EnvironmentVariableTarget.User);
			if (primalPath == null || !Directory.Exists(Path.Combine(primalPath, @"Engine\EngineAPI")))
			{
				var dlg = new EnginePathDialog();
				if (dlg.ShowDialog() == true)
				{
					PrimalPath = dlg.PrimalPath;
					Environment.SetEnvironmentVariable("PRIMAL_ENGINE", PrimalPath.ToUpper(), EnvironmentVariableTarget.User);
				}
				else
				{
					Application.Current.Shutdown();
				}
			}
			else
			{
				PrimalPath = primalPath;
			}
		}

		private void OnMainWindowClosing(object sender, CancelEventArgs e)
		{
			if (DataContext == null)
			{
				e.Cancel = true;
				Application.Current.MainWindow.Hide();
				OpenProjectBrowserDialogue();
				if (DataContext != null)
				{
					Application.Current.MainWindow.Show();
				}

			}
			else
			{
				Closing -= OnMainWindowClosing;
				Project.Current?.Unload();
				DataContext = null;
			}
		}


		private void OpenProjectBrowserDialogue()
		{
			var projectBrowser = new ProjectBrowserDialogue();
			if (projectBrowser.ShowDialog() == false || projectBrowser.DataContext == null)
			{
				Application.Current.Shutdown();
			}
			else
			{
				Project.Current?.Unload();
				var project = projectBrowser.DataContext as Project;
				Debug.Assert(project != null);
				ContentWatcher.Reset(project.ContentPath, project.Path);
				DataContext = project;
			}
		}
	}
}
