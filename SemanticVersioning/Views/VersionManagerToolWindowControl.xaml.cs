using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SemanticVersioning.ViewModels;

namespace SemanticVersioning.Views
{
    /// <summary>
    ///     Interaction logic for VersionManagerToolWindowControl.
    /// </summary>
    public partial class VersionManagerToolWindowControl : UserControl
    {
        private readonly VersionManagerToolWindowViewModel _versionManagerToolWindowViewModel;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VersionManagerToolWindowControl" /> class.
        /// </summary>
        public VersionManagerToolWindowControl(VersionManagerToolWindow versionManagerToolWindow)
        {
            InitializeComponent();

            DataContext = _versionManagerToolWindowViewModel = new VersionManagerToolWindowViewModel();

            IsVisibleChanged += OnIsVisibleChanged;
            versionManagerToolWindow.DocumentSaved += OnDocumentSaved;
        }

        private void OnDocumentSaved(object sender, DocumentSavedEventArgs e)
        {
            var matches = new List<string>
            {
                ".csproj",
                "AssemblyInfo.cs",
                "AndroidManifest.xml",
                "Info.plist",
                "Package.appxmanifest"
            };

            if (matches.Any(x => e.FullName.EndsWith(x)))
                _versionManagerToolWindowViewModel.Load();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool) e.NewValue;

            if (isVisible)
                _versionManagerToolWindowViewModel.Load();
        }

        private void OnRefreshButtonClick(object sender, RoutedEventArgs e)
        {
            _versionManagerToolWindowViewModel.Load();
        }

        private void OnUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            _versionManagerToolWindowViewModel.Update();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox_SelectAll(sender);
        }

        private void TextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TextBox_SelectAll(sender);
        }

        private void TextBox_SelectAll(object sender)
        {
            var textBox = sender as TextBox;

            textBox.SelectAll();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                // Update binding source (necessary because the binding source is not updated until the TextBox loses focus by default)
                var textBox = sender as TextBox;
                var bindingExpression = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
                bindingExpression?.UpdateSource();

                _versionManagerToolWindowViewModel.Update();
            }
        }
    }
}