namespace SemanticVersioning
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for VersioningWindowControl.
    /// </summary>
    public partial class VersioningWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersioningWindowControl"/> class.
        /// </summary>
        public VersioningWindowControl()
        {
            this.InitializeComponent();

            DataContext = this;

            Version = VersionManager.GetCurrentVersion();
        }

        public Version Version { get; set; }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Updated version to {Version.Major}.{Version.Minor}.{Version.Patch}", "Version Updated");
        }
    }
}