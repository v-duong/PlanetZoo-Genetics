using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace PlanetZooGeneHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<AnimalData> AnimalList { get; set; } = new ObservableCollection<AnimalData>();
        public ObservableCollection<string> SpeciesList { get; set; } = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ReadZooFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            AnimalList.Clear();
            SpeciesList.Clear();

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

            foreach (AnimalData a in AnimalList)
            {
                if (!SpeciesList.Contains(a.Species))
                    SpeciesList.Add(a.Species);
            }
            Lister.ItemsSource = AnimalList;
            SpeciesSelection.ItemsSource = SpeciesList;
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

        private void FindOffspring_Click(object sender, RoutedEventArgs e)
        {
            if (SpeciesSelection.SelectedItem == null)
                return;
            string selectedSpecies = SpeciesSelection.SelectedItem.ToString();
            List<AnimalData> maleList = new List<AnimalData>();
            List<AnimalData> femaleList = new List<AnimalData>();
            List<PairingData> pairingsList = new List<PairingData>();

            foreach (AnimalData a in AnimalList)
            {
                if (a.Species == selectedSpecies && a.IsFertile)
                {
                    if (a.Sex == 0x00)
                    {
                        maleList.Add(a);
                    }
                    else
                    {
                        femaleList.Add(a);
                    }
                }
            }

            foreach (AnimalData female in femaleList)
            {
                foreach (AnimalData male in maleList)
                {
                    PairingData pairingData = new PairingData(female.Name, male.Name);
                    pairingsList.Add(pairingData);

                    Dictionary<int, List<Gene>> sizeOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.SIZE);
                    Dictionary<int, List<Gene>> longevityOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.LONGEVITY);
                    Dictionary<int, List<Gene>> fertilityOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.FERTILITY);
                    Dictionary<int, List<Gene>> immunityOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.IMMUNITY);

                    foreach (var sPair in sizeOccurences)
                        foreach (var lPair in longevityOccurences)
                            foreach (var fPair in fertilityOccurences)
                                foreach (var iPair in immunityOccurences)
                                {
                                    pairingData.AddOffspringData(sPair.Key, lPair.Key, fPair.Key, iPair.Key, sPair.Value.Count
                                                                                                             * lPair.Value.Count
                                                                                                             * fPair.Value.Count
                                                                                                             * iPair.Value.Count);
                                }

                    pairingData.CalculateProbabilities();
                }
            }

            if (pairingsList.Count == 0)
                return;

            OffspringGridView window = new OffspringGridView();

            for (int i = 0; i < pairingsList.Count; i++)
            {
                PairingData pair = pairingsList[i];
                var newStackPanel = new StackPanel
                {
                    Name = "SP" + i
                };
                var expander = new Expander
                {
                    Name = "Expander" + i,
                    Header = pair.MotherName + " & " + pair.FatherName,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left
                };
                var dg = new OffspringGrid();
                dg.dataGrid.ItemsSource = pair.publicView;
                newStackPanel.Children.Add(dg);
                expander.Content = newStackPanel;
                window.PanelName.Children.Add(expander);
            }
            window.PairingsGrid.dataGrid.ItemsSource = pairingsList;
            window.Show();
        }
    }
}