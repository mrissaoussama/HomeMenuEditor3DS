using System;
using System.ComponentModel;
using System.IO;

namespace HomeMenuEditor3DSUI
{
    public class TitleViewModel : INotifyPropertyChanged
    {
        private Title title;
        public Title Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    OnPropertyChanged("Title");
                    OnPropertyChanged("IconPath");
                }
            }
        }

        public string IconPath
        {
            get { return GetIconPath(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string GetIconPath()
        {
            if (Title != null)
            {
                var filename= $"{Title.TitleID}.jpg";
                var file = Path.Combine(MainWindow.SMDH_Directory_Path, filename);
                if(File.Exists(file)) 
                return file;
            }
            
                return Path.Combine(MainWindow.SMDH_Directory_Path, "noicon.jpg");
            
        }
    }
}
