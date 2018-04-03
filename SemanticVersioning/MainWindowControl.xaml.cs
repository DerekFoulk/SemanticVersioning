namespace SemanticVersioning
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();

            DataContext = this;

            Version = SemanticVersioningManager.Instance.CurrentVersion;
        }

        public Version Version { get; set; }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Updated version to {Version.ToString()}", "Version Updated");
        }
    }
}