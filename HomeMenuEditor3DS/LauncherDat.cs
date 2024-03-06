using SMDH_Creator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
public enum TitleType
{
    System,
    SD
}

public class Title
{
    public string TitleID { get; set; }
    public int Position { get; set; }
    public TitleType Type { get; set; }
    public bool IsCardTitle { get; set; }
    public TitleFolder? Folder { get; set; }
    public Title(string titleID, int position, TitleType type)
    {
        TitleID = titleID;
        Position = position;
        Type = type;
    }
    public Title(string titleID, TitleType type)
    {
        TitleID = titleID;
        Type = type;
    }
}

public class TitleFolder
{
    public string Name { get; set; }
    public uint Rows { get; set; }
    public List<Title> Titles { get; set; }
    public uint FolderNumber { get; set; }
    public short Position { get; set; }

    public TitleFolder(string name, uint rows, uint folderNumber, short position)
    {
        Name = name;
        Rows = rows;
        FolderNumber = folderNumber;
        Position = position;
        Titles = new List<Title>();
    }

    public TitleFolder()
    {
    }
}

public class DataParser
{
    private const int FolderNamesOffset = 0x1560;
    public const int NextFolderNumberOffset = 0xd80;
    public const int NextFolderNumberLength = 0x4;
    public const int IconPositionArrayOffset = 0xD9A;
    public const int FolderPositionsOffset = 0x11DC;
    private const int FolderNumbersOffset = 0x1D58;
    private const int FolderNumbersLength = 0xF0;
    private const int FolderNamesLength = 0x22;
    private const int CardPositionOffset = 0x2;
    private const int TitleIDArrayOffset = 0x8;
    private const int IconPositionsInFoldersOffset = 0x106A;

    private const int SaveDataTitleIDArrayOffset = 0x8;
    private const int SaveDataIconPositionArrayOffset = 0xCB0;
    private const int SaveDataIconPositionsInFoldersOffset = 0xF80;
    public DataParser()
    {
        SDTitles = new();
        SystemTitles = new();
        Folders = new();
    }

    public List<Title> SystemTitles { get; set; }
    public List<Title> SDTitles { get; private set; }
    public List<TitleFolder> Folders { get; private set; }
    public Title? CardTitle { get; private set; }

    public void ReadData(byte[] launcherData, byte[] saveData)
    {
        ReadLauncherData(launcherData);
    }

    public void SwapTitles(Title title1, Title title2)
    {
        if (title1 == null || title2 == null)
            throw new ArgumentNullException("One or both titles are null.");

        Console.WriteLine($"{title1.TitleID} swapped with {title2.TitleID}");

        // Swap positions
        int tempPosition = title1.Position;
        title1.Position = title2.Position;
        title2.Position = tempPosition;

        if (title1.Folder != null && title2.Folder != null)
        {
            int tempIndex1 = title1.Folder.Titles.IndexOf(title1);
            int tempIndex2 = title2.Folder.Titles.IndexOf(title2);
            title1.Folder.Titles[tempIndex1] = title2;
            title2.Folder.Titles[tempIndex2] = title1;
        }
        else if (title1.Folder != null && title2.Folder == null)
        {
            title1.Folder.Titles[title1.Position] = title2;
            title2.Folder = title1.Folder;
            title1.Folder = null;
        }
        else if (title2.Folder != null && title1.Folder == null)
        {
            title2.Folder.Titles[title2.Position] = title1;
            title1.Folder = title2.Folder;
            title2.Folder = null;
        }
    }


    public int GetUnusedPosition()
    {
        HashSet<int> usedPositions = new HashSet<int>();
        foreach (var title in SystemTitles)
        {
            usedPositions.Add(title.Position);
        }
        foreach (var title in SDTitles)
        {
            usedPositions.Add(title.Position);
        }
        for (int i = 0; i < 360; i++)
        {
            if (!usedPositions.Contains(i))
            {
                return i;
            }
        }
        return -1;
    }

    public void ReadLauncherData(byte[] data)
    {
        SystemTitles = new List<Title>();

        using (MemoryStream stream = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.BaseStream.Seek(TitleIDArrayOffset, SeekOrigin.Begin);
            for (int i = 0; i < 360; i++)
            {
                ulong titleID = reader.ReadUInt64();
                if (titleID != ulong.MaxValue)
                {
                    Title title = new Title(titleID.ToString("X16"), TitleType.System);
                    SystemTitles.Add(title);
                }
                else
                {
                    if (!SDTitles.Any(t => t.Position == i))
                    {
                        Title dummyTitle = new Title("FFFFFFFFFFFFFFFF", TitleType.System);
                        SystemTitles.Add(dummyTitle);
                    }
                }
            }
            reader.BaseStream.Seek(CardPositionOffset, SeekOrigin.Begin);
            ushort cardPosition = reader.ReadUInt16();
            if (cardPosition < SystemTitles.Count)
            {
                Title cardTitle = SystemTitles[cardPosition];
                cardTitle.Type = TitleType.System;
                cardTitle.Position = cardPosition;
                cardTitle.IsCardTitle = true;
                CardTitle = cardTitle;
            }
            reader.BaseStream.Seek(IconPositionArrayOffset, SeekOrigin.Begin);
            foreach (var title in SystemTitles)
            {
                short position = reader.ReadInt16();
                title.Position = position;
            }
            reader.BaseStream.Seek(FolderPositionsOffset, SeekOrigin.Begin);

            for (int i = 0; i < 60; i++)
            {
                TitleFolder folder = new();
                folder.Position=reader.ReadInt16();
                Folders.Add(folder);
            }
            reader.BaseStream.Seek(FolderNamesOffset, SeekOrigin.Begin);

            foreach (var folder in Folders)
            {
                byte[] nameBytes = reader.ReadBytes(FolderNamesLength);
                string folderName = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');
                folder.Name=folderName;
            }
            reader.BaseStream.Seek(FolderNumbersOffset, SeekOrigin.Begin);

            foreach (var folder in Folders)
            {
uint  number = reader.ReadUInt32();
                folder.FolderNumber = number;
            }
            reader.BaseStream.Seek(IconPositionsInFoldersOffset, SeekOrigin.Begin);
            sbyte[] iconPositions = new sbyte[360];
            for (int i = 0; i < 360; i++)
            {
                iconPositions[i] = reader.ReadSByte();
            }
            for (int i = 0; i < SystemTitles.Count; i++)
            {
                var iconPositionIndex = iconPositions[i];
                if (iconPositionIndex != -1)
                {
                    var folder = Folders[iconPositionIndex];
                    if (folder.Titles is null) folder.Titles = new();
                    folder?.Titles.Add(SystemTitles[i]);
                    SystemTitles[i].Folder = folder;
                }
            }
        }
    }

    public void ReadSaveData(byte[] data)
    {
        SDTitles = new List<Title>();

        using (MemoryStream stream = new MemoryStream(data))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            reader.BaseStream.Seek(SaveDataTitleIDArrayOffset, SeekOrigin.Begin);

            for (int i = 0; i < 360; i++)
            {
                ulong titleID = reader.ReadUInt64();
                if (titleID != ulong.MaxValue)
                {

                    Title title = new Title(titleID.ToString("X16"), TitleType.SD);

                    SDTitles.Add(title);
                }
                else
                {
                   
                        Title dummyTitle = new Title("FFFFFFFFFFFFFFFF", TitleType.SD);
                        SDTitles.Add(dummyTitle);
                    
                }
            }

            reader.BaseStream.Seek(SaveDataIconPositionArrayOffset, SeekOrigin.Begin);

            foreach (var title in SDTitles)
            {
                short position = reader.ReadInt16();

                title.Position = position;
            }

            reader.BaseStream.Seek(SaveDataIconPositionsInFoldersOffset, SeekOrigin.Begin);
            sbyte[] foldericonPositions = new sbyte[360];
            for (int i = 0; i < 360; i++)
            {
                foldericonPositions[i] = reader.ReadSByte();
            }
            for (int i = 0; i < SDTitles.Count; i++)
            {

                var iconPositionIndex = foldericonPositions[i];
                if (iconPositionIndex != -1)
                {
                    var folder = Folders[iconPositionIndex];
                    if (folder.Name == "Sonic")
                        Console.Beep();
                    if (folder.Titles is null) folder.Titles = new();
                    Title title = SDTitles[i];
                    folder?.Titles.Add(title);
                    SDTitles[i].Folder = folder;
                }
            }

        }
    }
    public void DisplayCardTitle()
    {
        if (CardTitle != null)
        {
            Console.WriteLine("Card Title:");
            Console.WriteLine($"Title ID: {CardTitle.TitleID}");
        }
        else
        {
            Console.WriteLine("No card title found.");
        }
    }

    public void DisplaySystemTitlesNotInFolders()
    {
        Console.WriteLine("System Titles:");
        foreach (Title title in SystemTitles.Where(t=>t.Folder is null))
        {
            if (title.TitleID is not null && title.TitleID is not "FFFFFFFFFFFFFFFF")
                Console.WriteLine($"Title ID: {title.TitleID}, Position: {title.Position}");
        }
    }

    public void DisplaySDTitlesNotInFolders()
    {
        Console.WriteLine("SD Titles:");
        foreach (Title title in SDTitles)
        {
            if (title.TitleID is not null && title.TitleID !="FFFFFFFFFFFFFFFF")
                Console.WriteLine($"Title ID: {title.TitleID}, Position: {title.Position}");
        }
    }

 
    public void ExtractSmallIconsFromSMDH(string smdhDirectoryPath)
    {
        foreach (Title title in SystemTitles.Concat(SDTitles))
        {
            if (title.TitleID is null) continue;
            string titleID = title.TitleID.ToLowerInvariant();
            int halfLength = titleID.Length / 2;
            string hyphenatedTitleID = titleID.Insert(halfLength, "-");

            string[] imagenames = { titleID + ".jpg", hyphenatedTitleID + ".jpg" };
            string imagepath = null;

            foreach (string possibleTitle in imagenames)
            {
                imagepath = Path.Combine(smdhDirectoryPath, possibleTitle);
                if (!File.Exists(imagepath))
                {
                    if (!string.IsNullOrEmpty(titleID))
                    {
                        string smdhFileName = titleID + ".smdh";
                        string iconFilePath = Path.Combine(smdhDirectoryPath, smdhFileName);
                        string[] smdhnames = { titleID + ".smdh", hyphenatedTitleID + ".smdh" };
                        foreach (var smdhname in smdhnames)
                        {
                            var smdhpath = Path.Combine(smdhDirectoryPath, smdhname);

                            if (File.Exists(smdhpath))
                            {
                                SMDH smdhParser = new SMDH();
                                smdhParser.Load(smdhpath);
                                smdhParser.BigIcon.Save(imagepath, System.Drawing.Imaging.ImageFormat.Jpeg);

                                Console.WriteLine($" icon extracted for Title ID: {titleID}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"SMDH file not found for Title ID: {titleID}");
                    }
                    break;
                }
            }
        }
    }

  
    public void DisplayFoldersAndTitles()
    {
        foreach (var folder in Folders)
        {
            Console.WriteLine($"Folder Name: {folder.Name}, Position: {folder.Position}");
            if(folder.Titles is not null)
            foreach (var title in folder.Titles)
            {
                Console.WriteLine($"   Title ID: {title.TitleID}, Position: {title.Position}");
            }
        }
    }

    public void RenameFolder(uint folderNumber, string newName)
    {
        var folder = Folders.FirstOrDefault(f => f.FolderNumber == folderNumber);
        if (folder != null)
        {
            folder.Name = newName;
        }
        else
        {
            Console.WriteLine("Folder not found.");
        }
    }

}
