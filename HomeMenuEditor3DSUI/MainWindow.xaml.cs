// File: HomeMenuEditor3DSUI/MainWindow.xaml.cs

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows; 
using System.Reflection;
using System.Text.Json;   // For exe path

namespace HomeMenuEditor3DSUI
{
    public class AppConfig
    {
        public string LauncherPath { get; set; } = string.Empty;
        public string SaveDataPath { get; set; } = string.Empty;
        public string IconDataPath { get; set; } = string.Empty;
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<SlotViewModel> Slots { get; set; }
        public List<ObservableCollection<SlotViewModel>> SlotGroups { get; set; }
        public ObservableCollection<SlotViewModel> CurrentFolderSlots { get; set; }
        public TitleFolder CurrentFolder { get; set; }
        private SlotViewModel selectedSlot;
        public SlotViewModel SelectedSlot
        {
            get => selectedSlot;
            set
            {
                if (selectedSlot != value)
                {
                    selectedSlot = value;
                    OnPropertyChanged(nameof(SelectedSlot));
                }
            }
        }

        // File Paths
        public static string launcherDatFilePath { get; set; }
        public static string saveDataFilePath { get; set; }
        public static string iconDataFolderPath { get; set; }

        public static string DefaultIconDataFolderPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icondata");

        private DataParser dataParser;
        byte[] launcherbytes;
        byte[] savedatabytes;
        private AppConfig appConfig;
        private readonly string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            dataParser = new DataParser();

            // Initialize paths with default values
            launcherDatFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Launcher.dat");
            saveDataFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData.dat");
            iconDataFolderPath = DefaultIconDataFolderPath;

            LoadConfig();
            if (File.Exists(launcherDatFilePath) && File.Exists(saveDataFilePath))
            {
                LoadTitles();
            }
        
        }
        private void LoadConfig()
        {
            if (File.Exists(configFilePath))
            {
                try
                {
                    string json = File.ReadAllText(configFilePath);
                    appConfig = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error loading config: {ex.Message}", "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    appConfig = new AppConfig();
                }
            }
            else
            {
                appConfig = new AppConfig();
                SaveConfig(); // Create default config file
            }

            // Set paths from config or defaults
            launcherDatFilePath = string.IsNullOrEmpty(appConfig.LauncherPath) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Launcher.dat") : appConfig.LauncherPath;
            saveDataFilePath = string.IsNullOrEmpty(appConfig.SaveDataPath) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SaveData.dat") : appConfig.SaveDataPath;
            iconDataFolderPath = string.IsNullOrEmpty(appConfig.IconDataPath) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icondata") : appConfig.IconDataPath;

            // Ensure IconData directory exists
            if (!Directory.Exists(iconDataFolderPath))
            {
                Directory.CreateDirectory(iconDataFolderPath);
            }
        }

        private void SaveConfig()
        {
            appConfig.LauncherPath = launcherDatFilePath;
            appConfig.SaveDataPath = saveDataFilePath;
            appConfig.IconDataPath = iconDataFolderPath;

            try
            {
                string json = JsonSerializer.Serialize(appConfig, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving config: {ex.Message}", "Config Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SortAllTitlesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataParser.SortAllTitlesByTitleID();
              SaveButton_Click(null, null);
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
            }
        }
        private void SortFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentFolder != null)
                {
                    dataParser.SortFolderTitlesByTitleID(CurrentFolder);
                    SaveButton_Click(null, null);
                    LoadFolderContents(CurrentFolder);
               
                }
            }
            catch (Exception ex)
            {
            }
        }
        private void SortAllTitlesByTitleIDButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataParser.SortAllTitlesBy(t => t.TitleID);
                SaveButton_Click(null, null);
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortAllTitlesByNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataParser.SortAllTitlesBy(t => t.Name);
                SaveButton_Click(null, null);
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortAllTitlesBySizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataParser.SortAllTitlesBy(t => t.Size);
                SaveButton_Click(null, null);
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortAllTitlesByPublisherButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataParser.SortAllTitlesBy(t => t.Publisher);
                SaveButton_Click(null, null);
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortAllTitlesByGenreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dataParser.SortAllTitlesBy(t => t.Genre);
                SaveButton_Click(null, null);
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }
        private void SortFolderByTitleIDButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentFolder != null)
                {
                    dataParser.SortFolderTitlesBy(CurrentFolder, t => t.TitleID);
                    SaveButton_Click(null, null);
                    LoadFolderContents(CurrentFolder);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortFolderByNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentFolder != null)
                {
                    dataParser.SortFolderTitlesBy(CurrentFolder, t => t.Name);
                    SaveButton_Click(null, null);
                    LoadFolderContents(CurrentFolder);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortFolderBySizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentFolder != null)
                {
                    dataParser.SortFolderTitlesBy(CurrentFolder, t => t.Size);
                    SaveButton_Click(null, null);
                    LoadFolderContents(CurrentFolder);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortFolderByPublisherButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentFolder != null)
                {
                    dataParser.SortFolderTitlesBy(CurrentFolder, t => t.Publisher);
                    SaveButton_Click(null, null);
                    LoadFolderContents(CurrentFolder);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void SortFolderByGenreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentFolder != null)
                {
                    dataParser.SortFolderTitlesBy(CurrentFolder, t => t.Genre);
                    SaveButton_Click(null, null);
                    LoadFolderContents(CurrentFolder);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }
        }

        private void LoadTitles()
        {
            SaveConfig();
            if (Slots == null)
                Slots = new ObservableCollection<SlotViewModel>();
            else
                Slots.Clear();

            try
            {
        
                    launcherbytes = File.ReadAllBytes(launcherDatFilePath);
              

                    savedatabytes = File.ReadAllBytes(saveDataFilePath);
               

                dataParser.ReadData(launcherbytes, savedatabytes);
                dataParser.ExtractSmallIconsFromSMDH(iconDataFolderPath);

                // Fill Slots with empty placeholders
                for (int i = 0; i < 360; i++)
                {
                    Slots.Add(new SlotViewModel()); // Empty slot
                }

                // Place titles into their positions
                foreach (var title in dataParser.SystemTitles.Concat(dataParser.SDTitles))
                {
                    if (title.Position >= 0 && title.Position < Slots.Count && title.Folder is null)
                    {
                        var slot = Slots[title.Position];
                        if (slot.IsEmpty)
                        {
                            slot.Title = title;
                        }
                        else
                        {
                            Console.WriteLine($"Cannot assign title '{title.TitleHex}' to slot {title.Position} because it is already occupied.");
                            // Optionally, handle the conflict (e.g., find next available slot)
                        }
                    }
                }

                // Place folders into their positions
                foreach (var folder in dataParser.Folders)
                {
                    if (folder.Position >= 0 && folder.Position < Slots.Count)
                    {
                        var slot = Slots[folder.Position];
                        if (slot.IsEmpty)
                        {
                            slot.Folder = folder;
                        }
                        else
                        {
                            Console.WriteLine($"Cannot assign folder '{folder.Name}' to slot {folder.Position} because it is already occupied.");
                            // Optionally, assign to next available slot or notify the user
                            var nextEmptySlot = Slots.FirstOrDefault(s => s.IsEmpty);
                            if (nextEmptySlot != null)
                            {
                                nextEmptySlot.Folder = folder;
                                Console.WriteLine($"Folder '{folder.Name}' moved to slot {Slots.IndexOf(nextEmptySlot)}.");
                            }
                            else
                            {
                                System.Windows.MessageBox.Show($"No available slots to assign folder '{folder.Name}'.", "Assignment Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }

                // Split Slots into SlotGroups of 60 slots each
                SlotGroups = new List<ObservableCollection<SlotViewModel>>();
                for (int i = 0; i < 360; i += 60)
                {
                    var group = new ObservableCollection<SlotViewModel>();
                    for (int j = 0; j < 60; j++)
                    {
                        group.Add(Slots[i + j]);
                    }
                    SlotGroups.Add(group);
                }

                OnPropertyChanged(nameof(Slots));
                OnPropertyChanged(nameof(SlotGroups));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading titles: {ex.Message}", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                File.WriteAllBytes(saveDataFilePath, savedatabytes);

                // Reload the byte arrays from the files
                launcherbytes = File.ReadAllBytes(launcherDatFilePath);
                savedatabytes = File.ReadAllBytes(saveDataFilePath);

                // Refresh the UI
                ReloadButton_Click(null, null);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            Slots.Clear();
            dataParser = new DataParser();
            LoadTitles();
            CurrentFolder = null;
            CurrentFolderSlots = null;
            OnPropertyChanged(nameof(CurrentFolder));
            OnPropertyChanged(nameof(CurrentFolderSlots));
            OnPropertyChanged(nameof(IsFolderContentVisible));
        }

        // Title swapping logic
        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                var clickedSlot = button.DataContext as SlotViewModel;
                if (clickedSlot != null)
                {
                    if (SelectedSlot == null)
                    {
                        SelectedSlot = clickedSlot;
                    }
                    else
                    {
                        SwapSlots(SelectedSlot, clickedSlot);
                        SelectedSlot = null;
                    }
                }
            }
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedSlot = null;
        }

        private void SwapSlots(SlotViewModel slot1, SlotViewModel slot2)
        {
            if (slot1 == slot2)
                return;

            var title1 = slot1.Title;
            var title2 = slot2.Title;

            var folder1 = GetSlotFolder(slot1);
            var folder2 = GetSlotFolder(slot2);

            int position1 = GetSlotPosition(slot1);
            int position2 = GetSlotPosition(slot2);

            if (title1 != null && title2 != null)
            {
                // Swap titles
                dataParser.SwapTitles(title1, title2);

                // Update UI Slots
                slot1.Title = title2;
                slot1.Folder = title2.Folder;
                slot2.Title = title1;
                slot2.Folder = title1.Folder;
            }
            else if (title1 != null && title2 == null)
            {
                // Move title1 to slot2
                dataParser.MoveTitle(title1, position2, folder2);

                // Update UI Slots
                slot2.Title = title1;
                slot2.Folder = title1.Folder;
                slot1.Title = null;
                slot1.Folder = null;
            }
            else if (title1 == null && title2 != null)
            {
                // Move title2 to slot1
                dataParser.MoveTitle(title2, position1, folder1);

                // Update UI Slots
                slot1.Title = title2;
                slot1.Folder = title2.Folder;
                slot2.Title = null;
                slot2.Folder = null;
            }
            else
            {
                // Both slots are empty; do nothing
                return;
            }

            OnPropertyChanged(nameof(SelectedSlot));
        }

        private int GetSlotPosition(SlotViewModel slot)
        {
            if (CurrentFolderSlots != null && CurrentFolderSlots.Contains(slot))
            {
                return CurrentFolderSlots.IndexOf(slot);
            }
            else
            {
                return Slots.IndexOf(slot);
            }
        }

        private TitleFolder? GetSlotFolder(SlotViewModel slot)
        {
            if (CurrentFolderSlots != null && CurrentFolderSlots.Contains(slot))
            {
                return CurrentFolder;
            }
            else
            {
                return null;
            }
        }

        private void RefreshUI()
        {
            if (CurrentFolder != null)
            {
                LoadFolderContents(CurrentFolder);
            }
            else
            {
                LoadTitles();
            }
            OnPropertyChanged(nameof(SelectedSlot));
        }

        // Folder click logic
        private void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                var clickedSlot = button.DataContext as SlotViewModel;
                if (clickedSlot != null && clickedSlot.IsFolder)
                {
                    CurrentFolder = clickedSlot.Folder;
                    LoadFolderContents(CurrentFolder);
                }
            }
        }

        private void LoadFolderContents(TitleFolder folder)
        {
            if (folder != null)
            {
                if (CurrentFolderSlots == null)
                    CurrentFolderSlots = new ObservableCollection<SlotViewModel>();
                else
                    CurrentFolderSlots.Clear();

                // Fill folder slots with empty placeholders
                for (int i = 0; i < 60; i++)
                {
                    CurrentFolderSlots.Add(new SlotViewModel()); // Empty slot
                }

                // Place titles into their positions within the folder
                foreach (var title in folder.Titles)
                {
                    if (title.Position >= 0 && title.Position < CurrentFolderSlots.Count)
                    {
                        var slot = CurrentFolderSlots[title.Position];
                        if (slot.IsEmpty)
                        {
                            slot.Title = title;
                        }
                        else
                        {
                            Console.WriteLine($"Cannot assign title '{title.TitleHex}' to folder slot {title.Position} because it is already occupied.");
                            // Optionally, handle the conflict
                        }
                    }
                }

                OnPropertyChanged(nameof(CurrentFolder));
                OnPropertyChanged(nameof(CurrentFolderSlots));
                OnPropertyChanged(nameof(IsFolderContentVisible));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide the folder content
            CurrentFolder = null;
            CurrentFolderSlots = null;

            OnPropertyChanged(nameof(CurrentFolder));
            OnPropertyChanged(nameof(CurrentFolderSlots));
            OnPropertyChanged(nameof(IsFolderContentVisible));
        }

        public bool IsFolderContentVisible => CurrentFolder != null;

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // New Button Click Handlers

        private void SetLauncherPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Launcher.dat",
                Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*",
                InitialDirectory = Path.GetDirectoryName(launcherDatFilePath)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                launcherDatFilePath = openFileDialog.FileName;
            }
        }

        private void SetSaveDataPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select SaveData.dat",
                Filter = "DAT files (*.dat)|*.dat|All files (*.*)|*.*",
                InitialDirectory = Path.GetDirectoryName(saveDataFilePath)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                saveDataFilePath = openFileDialog.FileName;
            }
        }

        private void SetIconDataPath_Click(object sender, RoutedEventArgs e)
        {
                  OpenFolderDialog openFolderDialog = new OpenFolderDialog
                  {
                      Title = "Select SaveData.dat",
                      
                     
                      InitialDirectory = System.Environment.ProcessPath
                  };


            if (openFolderDialog.ShowDialog() == true)
            {
                    iconDataFolderPath = openFolderDialog.FolderName;
                }
           
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTitles();
        }
    }
}