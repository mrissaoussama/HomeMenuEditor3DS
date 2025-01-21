using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace HomeMenuEditor3DSUI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<SlotViewModel> Slots { get; set; }
        public string launcherDatFilePath = "C:\\Users\\oussama\\Desktop\\Launcher.dat";
        public string savedataFilePath = "C:\\Users\\oussama\\Desktop\\SaveData.dat";
        public static string SMDH_Directory_Path = "C:\\Users\\oussama\\Desktop\\icondata";
        private DataParser dataParser;
        byte[] launcherbytes ;
        byte[] savedatabytes;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            dataParser = new DataParser();
            LoadTitles();
        }

        private void LoadTitles()
        {
            Slots = new ObservableCollection<SlotViewModel>();

            // Load the data
             launcherbytes = File.ReadAllBytes(launcherDatFilePath);
             savedatabytes = File.ReadAllBytes(savedataFilePath);
            dataParser.ReadData(launcherbytes, savedatabytes);
            dataParser.ExtractSmallIconsFromSMDH(SMDH_Directory_Path);

            // Combine all titles and folders into a single list with positions
            var allItems = new List<(int Position, SlotViewModel Slot)>();

            // Add system titles
            foreach (var title in dataParser.SystemTitles)
            {
                if (title.Position >= 0 && title.Position < 60)
                {
                    allItems.Add((title.Position, new SlotViewModel { Title = title }));
                }
            }

            // Add SD titles
            foreach (var title in dataParser.SDTitles)
            {
                if (title.Position >= 0 && title.Position < 60)
                {
                    allItems.Add((title.Position, new SlotViewModel { Title = title }));
                }
            }

            // Add folders
            foreach (var folder in dataParser.Folders)
            {
                if (folder.Position >= 0 && folder.Position < 60)
                {
                    allItems.Add((folder.Position, new SlotViewModel { Folder = folder }));
                }
            }

            // Fill Slots with empty placeholders
            for (int i = 0; i < 60; i++)
            {
                Slots.Add(new SlotViewModel()); // Empty slot
            }

            // Place titles and folders into their positions
            foreach (var item in allItems)
            {
                if (item.Position >= 0 && item.Position < Slots.Count)
                {
                    Slots[item.Position] = item.Slot;
                }
            }

            OnPropertyChanged(nameof(Slots));
        }

        // Save and Reload methods...
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update the launcherbytes and savedatabytes arrays with the changes
                dataParser.SaveLauncherData(launcherbytes);
                dataParser.SaveSaveData(savedatabytes);

                // Write the updated byte arrays back to the files
                File.WriteAllBytes(launcherDatFilePath, launcherbytes);
                File.WriteAllBytes(savedataFilePath, savedatabytes);

                // Reload the byte arrays from the files
                launcherbytes = File.ReadAllBytes(launcherDatFilePath);
                savedatabytes = File.ReadAllBytes(savedataFilePath);

                // Refresh the UI
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            Slots.Clear();
            dataParser = new DataParser();
            LoadTitles();
        }


        // Title swapping logic
        private SlotViewModel selectedTitle;
        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            if (button != null)
            {
                var clickedSlot = button.DataContext as SlotViewModel;
                if (clickedSlot != null)
                {
                    if (selectedTitle == null)
                    {
                        selectedTitle = clickedSlot;
                    }
                    else
                    {
                        SwapSlots(selectedTitle, clickedSlot);
                        selectedTitle = null;
                    }
                }
            }
        }

        private void SwapSlots(SlotViewModel slot1, SlotViewModel slot2)
        {
            // Implement swapping logic using dataParser
            Title title1 = slot1.Title;
            Title title2 = slot2.Title;

            dataParser.SwapTitles(title1, title2);

            // Swap the slots in the collection
            int index1 = Slots.IndexOf(slot1);
            int index2 = Slots.IndexOf(slot2);
            if (index1 >= 0 && index2 >= 0)
            {
                Slots[index1] = slot2;
                Slots[index2] = slot1;
            }
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
