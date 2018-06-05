namespace SemanticVersioning.Views
{
    using SemanticVersioning.ViewModels;
    using System.Diagnostics.CodeAnalysis;
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
        public VersionManagerToolWindowControl()
        {
            this.InitializeComponent();

            DataContext = _versionManagerToolWindowViewModel = new VersionManagerToolWindowViewModel();

            IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;

            if (isVisible)
                _versionManagerToolWindowViewModel.Load();
        }
    }
}
