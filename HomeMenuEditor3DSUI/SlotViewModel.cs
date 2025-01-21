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
        private TitleFolder? folder;
        public TitleFolder? Folder
        {
            get { return folder; }
            set
            {
                if (folder != value)
                {
                    folder = value;
                    OnPropertyChanged("TitleFolder");
                    OnPropertyChanged("IconPath");
                }
            }
        }

        public string IconPath
        {
            get { return GetIconPath(); }
        }

     
        public bool IsFolder => Folder != null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
        private string GetIconPath()
        {if(Folder!=null)
                return Path.Combine(MainWindow.SMDH_Directory_Path, "folder.png");
            if (Title != null)
            {if(Title.IsCardTitle)
                {
                    return Path.Combine(MainWindow.SMDH_Directory_Path, "cart.jpg");

                }
                var filename= $"{Title.TitleHex}.jpg";
                var file = Path.Combine(MainWindow.SMDH_Directory_Path, filename);
                if(File.Exists(file)) 
                return file;
            }
            
                return Path.Combine(MainWindow.SMDH_Directory_Path, "noicon.jpg");
            
        }
    }
}
