using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

namespace PrimalEditor.GameDev
{
	/// <summary>
	/// Interaction logic for NewScriptDialog.xaml
	/// </summary>
	public partial class NewScriptDialog : Window
	{
		private static readonly string _cppCode = @"
#include ""{0}.h""

namespace {1} {{

	REGISTER_SCRIPT({0});

    void {0}::begin_play()
	{{
        
	}}

	void {0}::update(float dt)
	{{
        
	}}

}} // namespace {1}";

		private static readonly string _hCode = @"
#pragma once

namespace {1} {{

	class {0} : public primal::script::entity_script
	{{
	public:
		constexpr explicit {0}(primal::game_entity::entity entity)
			: primal::script::entity_script{{entity}} {{}}

		void begin_play() override;
		void update(float dt) override;

    private:
	}};

}} // namespace {1}";


		private static readonly string _namespace = getNamespaceFromProjectName();

		private static string getNamespaceFromProjectName()
		{
			var projectName = Project.Current.Name;
			if (string.IsNullOrEmpty(projectName)) return string.Empty;
			projectName = Regex.Replace(projectName, @"[^A-Za-z0-9_]", "");

			return projectName;
		}

		private bool Validate()
		{
			bool isValid = false;
			var name = scriptName.Text.Trim();
			var path = scriptPath.Text.Trim();
			string errorMessage = string.Empty;
			var nameRegex = new Regex(@"[^A-Za-z0-9_]");

			if (string.IsNullOrEmpty(name))
				errorMessage = "Type in a Script Name";
			else if (nameRegex.IsMatch(name))
				errorMessage = "Invalid Character(s) used is Script Name";
			else if (string.IsNullOrEmpty(path))
				errorMessage = "Type in a Script Path";
			else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
				errorMessage = "Invalid Character(s) used is Script Path";
			else if (!Path.GetFullPath(Path.Combine(Project.Current.Path, path)).Contains(Path.Combine(Project.Current.Path, @"GameCode\")))
				errorMessage = "Script Path must be added to (a sub-folder of) GameCode";
			else if (File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.cpp"))) ||
				File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h"))))
				errorMessage = $"Script {name} already exists in this folder";
			else
				isValid = true;

			if (!isValid)
				messageTextBlock.Foreground = FindResource("Editor.RedBrush") as Brush;
			else
				messageTextBlock.Foreground = FindResource("Editor.FontBrush") as Brush;

			messageTextBlock.Text = errorMessage;
			return isValid;
		}

		private void OnScriptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!Validate()) return;

			var name = scriptName.Text.Trim();
			messageTextBlock.Text = $"{name}.cpp and {name}.h will be added to {Project.Current.Name}";
		}

		private void OnScriptPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			Validate();
		}

		private async void OnCreate_ButtonClick(object sender, RoutedEventArgs e)
		{
			if (!Validate()) return;
			IsEnabled = false;

			busyAnimation.Opacity = 0;
			busyAnimation.Visibility = Visibility.Visible;
			DoubleAnimation fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
			busyAnimation.BeginAnimation(OpacityProperty, fadeIn);

			try
			{
				var name = scriptName.Text.Trim();
				var path = Path.GetFullPath(Path.Combine(Project.Current.Path, scriptPath.Text.Trim()));
				var solution = Project.Current.Solution;
				var projectName = Project.Current.Name;

				await Task.Run(() => CreateScript(name, path, solution, projectName));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				Logger.Log(MessageType.Error, $"Failed to Create Script {scriptName.Text}");
			}
			finally
			{
				DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));
				fadeOut.Completed += (s, e) =>
				{
					busyAnimation.Opacity = 0;
					busyAnimation.Visibility = Visibility.Hidden;
					Close();
				};
				busyAnimation.BeginAnimation(OpacityProperty, fadeOut);

			}
		}

		private void CreateScript(string name, string path, string solution, string projectName)
		{
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);

			var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
			var h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));

			using (var sw = File.CreateText(cpp))
			{
				sw.Write(string.Format(_cppCode, name, _namespace));
			}

			using (var sw = File.CreateText(h))
			{
				sw.Write(string.Format(_hCode, name, _namespace));
			}

			string[] files = new string[] { cpp, h };

			for (int i = 0; i < 3; ++i)
			{
				if (!VisualStudio.AddFilesToSolution(solution, projectName, files)) Thread.Sleep(1000);
				else break;
			}
		}

		public NewScriptDialog()
		{
			InitializeComponent();
			Owner = Application.Current.MainWindow;
			scriptPath.Text = @"GameCode\";
		}
	}
}
