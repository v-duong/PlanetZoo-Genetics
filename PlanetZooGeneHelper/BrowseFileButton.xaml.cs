using System.Windows;
using System.Windows.Forms;

namespace PlanetZooGeneHelper
{
    public partial class BrowseFileButton : System.Windows.Controls.UserControl
    {
        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(BrowseFileButton), new PropertyMetadata(""));



        public string Filter
        {
            get { return (string)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Filter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(string), typeof(BrowseFileButton), new PropertyMetadata("All Files (*.*)|*.*"));



        public BrowseFileButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                InitialDirectory = FilePath,
                AddExtension = true,
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = Filter,
            };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = fileDialog.FileName;
            }


            fileDialog.Dispose();
        }
    }
}
