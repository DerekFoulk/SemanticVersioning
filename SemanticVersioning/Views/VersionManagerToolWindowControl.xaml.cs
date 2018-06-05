namespace SemanticVersioning.Views
{
    using SemanticVersioning.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for VersionManagerToolWindowControl.
    /// </summary>
    public partial class VersionManagerToolWindowControl : UserControl
    {
        private VersionManagerToolWindowViewModel _versionManagerToolWindowViewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionManagerToolWindowControl"/> class.
        /// </summary>
        public VersionManagerToolWindowControl(VersionManagerToolWindow versionManagerToolWindow)
        {
            this.InitializeComponent();

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
            var isVisible = (bool)e.NewValue;

            if (isVisible)
                _versionManagerToolWindowViewModel.Load();
        }
    }
}
