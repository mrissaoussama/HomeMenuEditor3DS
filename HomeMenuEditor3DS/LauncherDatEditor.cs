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
        launcherDat.ReadLauncherData(launcerbytes);

        launcherDat.ReadSaveData(savedatabytes);
        launcherDat.DisplaySystemTitlesNotInFolders();
        launcherDat.DisplaySDTitlesNotInFolders();
        launcherDat.DisplayFoldersAndTitles();

        launcherDat.DisplayCardTitle();
        launcherDat.SwapTitles(launcherDat.SystemTitles.First(), launcherDat.SDTitles.First());
        launcherDat.SwapTitles(launcherDat.SystemTitles.First(), launcherDat.SDTitles.First());
    }
}
