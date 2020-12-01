using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Forms;

namespace PlanetZooGeneHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<AnimalData> AnimalList { get; set; } = new ObservableCollection<AnimalData>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReadZooFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            AnimalList.Clear();

            using (ZipArchive zip = new ZipArchive(File.OpenRead(filePath), ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.FullName is "parkdata")
                        using (Stream fileStream = entry.Open())
                        {
                            using (MemoryStream memStream = new MemoryStream())
                            {
                                fileStream.CopyTo(memStream);
                                memStream.Seek(0, SeekOrigin.Begin);
                                PlanetZooParkData parkData = new PlanetZooParkData(memStream);
                                AnimalList = new ObservableCollection<AnimalData>(parkData.Animals);
                            }
                        }
                }
            }

            Lister.ItemsSource = AnimalList;
        }

        private void FileClick(object sender, RoutedEventArgs e)
        {
            string filePath = "";

            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                InitialDirectory = "",
                AddExtension = true,
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Planet Zoo Park Saves (*.zoo)|*.zoo",
            };

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath = fileDialog.FileName;
            }

            fileDialog.Dispose();

            ReadZooFile(filePath);
        }
    }
}