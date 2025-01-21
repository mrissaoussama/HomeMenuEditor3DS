class Program
{
    static void Main(string[] args)
    {
        string launcherDatFilePath = "C:\\Users\\oussama\\Desktop\\Launcher.dat";
        string savedataFilePath = "C:\\Users\\oussama\\Desktop\\SaveData.dat";
        string SMDH_Directory_Path = "C:\\Users\\oussama\\Desktop\\icondata";
        byte[] launcerbytes = File.ReadAllBytes(launcherDatFilePath);
        byte[] savedatabytes = File.ReadAllBytes(savedataFilePath);
        DataParser launcherDat = new DataParser();
     
        //var firstfolder=launcherDat.Folders.First();
        //launcherDat.RenameFolder(firstfolder.FolderNumber, "testfolder");
        launcherDat.ReadLauncherData(launcerbytes);
        launcherDat.ReadSaveData(savedatabytes);
        launcherDat.DisplayFoldersAndTitles();
        // var x=launcherDat.GetUnusedPosition();
        var title = launcherDat.SDTitles.First();
        var title2 = launcherDat.SDTitles[1];
        var folder= launcherDat.CreateFolder("2es222lder", 31);
        launcherDat.AddTitleToFolder(folder, title); 
       // launcherDat.SwapTitles(title, title2);
        launcherDat.SaveLauncherData(launcerbytes);
        launcherDat.SaveSaveData(savedatabytes);
        launcherDat.ReadLauncherData(launcerbytes);
        launcherDat.ReadSaveData(savedatabytes);
        launcherDat.DisplayFoldersAndTitles();
       File.WriteAllBytes(savedataFilePath, savedatabytes);
     File.WriteAllBytes(launcherDatFilePath, launcerbytes);
    }
}
