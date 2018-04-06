namespace SemanticVersioning
{
    using PropertyChanged;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class MainWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl(MainWindow mainWindow)
        {
            this.InitializeComponent();

            DataContext = this;

            Version = SemanticVersioningManager.Instance.CurrentVersion;

            MainWindowCommand.Instance.WindowOpened += Refresh;
            mainWindow.DocumentSaved += Refresh;
        }

        public Version Version { get; set; }

        private void Refresh(object sender, EventArgs e)
        {
            Version = SemanticVersioningManager.Instance.CurrentVersion;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            SemanticVersioningManager.Instance.SetVersion(Version);
        }
    }
}
