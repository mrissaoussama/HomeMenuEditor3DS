using System;
using System.ComponentModel;
using System.IO;



namespace HomeMenuEditor3DSUI
{
    public class SlotViewModel : INotifyPropertyChanged
    {
        private Title? title;
        public Title? Title
        {
            get => title;
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(IsFolder));
                    OnPropertyChanged(nameof(IconPath));
                }
            }
        }

        private TitleFolder? folder;
        public TitleFolder? Folder
        {
            get => folder;
            set
            {
                if (folder != value)
                {
                    folder = value;
                    OnPropertyChanged(nameof(Folder));
                    OnPropertyChanged(nameof(IsEmpty));
                    OnPropertyChanged(nameof(IsFolder));
                    OnPropertyChanged(nameof(IconPath));
                }
            }
        }

        public override string ToString()
        {
            if (Folder != null)
                return Folder.ToString();
            else if (Title != null)
                return Title.ToString();
            else
                return base.ToString();
        }

        public string IconPath
        {
            get { return GetIconPath(); }
        }

        public bool IsEmpty => Title == null && Folder == null;
        public bool IsFolder => Folder != null;

        public string FolderInitial => Folder != null && !string.IsNullOrEmpty(Folder.Name) ? Folder.Name.Substring(0, 1).ToUpper() : "F";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string GetIconPath()
        {
            if (Title != null)
            {
                if (Title.IsCardTitle)
                {
                    return Path.Combine(MainWindow.iconDataFolderPath, "cart.jpg");
                }
                var filename = $"{Title.TitleHex}.jpg";
                var file = Path.Combine(MainWindow.iconDataFolderPath, filename);
                if (File.Exists(file))
                    return file;
            }
            if (Folder != null)
                return Path.Combine(MainWindow.iconDataFolderPath, "folder.png");
            if (Folder == null && Title == null)
                return Path.Combine(MainWindow.iconDataFolderPath, "notitle.jpg");
            else
                return Path.Combine(MainWindow.iconDataFolderPath, "noicon.jpg");
        }
    }
}
