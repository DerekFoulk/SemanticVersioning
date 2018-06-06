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
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionManagerToolWindowControl"/> class.
        /// </summary>
        public VersionManagerToolWindowControl()
        {
            this.InitializeComponent();

            DataContext = new VersionManagerToolWindowViewModel();
        }
    }
}