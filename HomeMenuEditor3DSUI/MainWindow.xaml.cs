using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HomeMenuEditor3DSUI
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<TitleViewModel> titles { get; set; }
        public string launcherDatFilePath = "C:\\Users\\oussama\\Desktop\\Launcher.dat";
        public string savedataFilePath = "C:\\Users\\oussama\\Desktop\\SaveData.dat";
        public static string SMDH_Directory_Path = "C:\\Users\\oussama\\Desktop\\icondata";
        public ObservableCollection<TitleViewModel> Titles
        {
            
            get { return titles; }
            set { titles = value; OnPropertyChanged("Titles"); }
        }

        private int columnCount;
        public int ColumnCount
        {
            get { return columnCount; }
            set { columnCount = value; OnPropertyChanged("ColumnCount"); }
        }

        private int rowCount;
        public int RowCount
        {
            get { return rowCount; }
            set { rowCount = value; OnPropertyChanged("RowCount"); }
        }

        private double iconSize;
        public double IconSize
        {
            get { return iconSize; }
            set { iconSize = value; OnPropertyChanged("IconSize"); }
        }

        private TitleViewModel selectedTitle;
        private DataParser dataParser;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            dataParser = new DataParser();
            LoadTitles();
            CalculateGridSize();
        }
      

    private void LoadTitles()
        {
            Titles = new ObservableCollection<TitleViewModel>();

            byte[] launcerbytes = File.ReadAllBytes(launcherDatFilePath);
            byte[] savedatabytes = File.ReadAllBytes(savedataFilePath);
            dataParser.ReadData(launcerbytes, savedatabytes);


            foreach (Title title in dataParser.SystemTitles)
            {
                Titles.Add(new TitleViewModel { Title = title });
            }

            foreach (Title title in dataParser.SDTitles)
            {
                AddOrUpdateTitleViewModel(new TitleViewModel { Title = title });
            }
        }


        private void AddOrUpdateTitleViewModel(TitleViewModel newViewModel)
        {
            int index = Titles.IndexOf(Titles.FirstOrDefault(vm => vm.Title.Position == newViewModel.Title.Position));
            if (index != -1)
            {
                Titles[index] = newViewModel;
            }
            else
            {
                Titles.Add(newViewModel);
            }
        }


        private string GetIconPath(string titleID)
        {
            string iconFileName = $"{titleID}.jpg";
            string iconPath = Path.Combine(SMDH_Directory_Path, iconFileName);

            if (File.Exists(iconPath))
            {
                return iconPath;
            }
            else
            {
                return null;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var clickedTitle = button.DataContext as TitleViewModel;
                if (clickedTitle != null)
                {
                    if (selectedTitle == null)
                    {
                        selectedTitle = clickedTitle;
                    }
                    else
                    {
                        SwapTitles(clickedTitle);
                        selectedTitle = null;
                    }
                }
            }
        }

        private void SwapTitles(TitleViewModel targetTitle)
        {
            Title title1 = selectedTitle.Title;
            Title title2 = targetTitle.Title;

            dataParser.SwapTitles(title1, title2);
            int index1 = Titles.IndexOf(selectedTitle);
            int index2 = Titles.IndexOf(targetTitle);
            if (index1 >= 0 && index2 >= 0)
            {
                TitleViewModel temp = Titles[index1];
                Titles[index1] = Titles[index2];
                Titles[index2] = temp;
            }
        }

        private void CalculateGridSize()
        {
            for (int i = 0; i < 60; i++)
            {
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            dataParser.SaveSDTitlesToFile(savedataFilePath);
            dataParser.SaveSystemTitlesToFile(launcherDatFilePath);
        }
        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            Titles.Clear();
            dataParser = new();
            LoadTitles();
        }
    }
}
