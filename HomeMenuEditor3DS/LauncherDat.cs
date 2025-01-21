// File: HomeMenuEditor3DS/LauncherDat.cs

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
    public ulong TitleID { get; set; }
    public int Position { get; set; } = -1; // Default to -1 to indicate uninitialized
    public TitleType Type { get; set; }
    public bool IsCardTitle { get; set; }
    public TitleFolder? Folder { get; set; }
    //add title hex property
    public string TitleHex => TitleID.ToString("X16");
    public Title(ulong titleID, int position, TitleType type)
    {
        TitleID = titleID;
        Position = position;
        Type = type;
    }

    public Title(ulong titleID, TitleType type)
    {
        TitleID = titleID;
        Type = type;
    }
}

public class TitleFolder
{
    public string Name { get; set; } = string.Empty; // Initialize to empty string
    public uint Rows { get; set; }
    public List<Title> Titles { get; set; } = new List<Title>();
    public uint FolderNumber { get; set; }
    public short Position { get; set; } = -1; // Default to -1 to indicate uninitialized

    public TitleFolder(string name, uint rows, uint folderNumber, short position)
    {
        Name = name;
        Rows = rows;
        FolderNumber = folderNumber;
        Position = position;
    }

    public TitleFolder()
    {
    }
}

public class DataParser
{
    // Constants
    private const int FormatVersionOffset = 0x0;
    private const int FolderNamesOffset = 0x1560;
    private const int NextFolderNumberOffset = 0xD80;
    private const int IconPositionArrayOffset = 0xD9A;
    private const int FolderPositionsOffset = 0x11DC;
    private const int FolderNumbersOffset = 0x1D58;
    private const int FolderNamesLength = 0x22; // 34 bytes (17 UTF-16 characters)
    private const int CardPositionOffset = 0x2;
    private const int CardPositionLength = 0x2;
    private const int TitleIDArrayOffset = 0x8;
    private const int TitleIDArrayLength = 0xB40;
    private const int IconPositionsInFoldersOffset = 0x106A;

    private const int SaveDataTitleIDArrayOffset = 0x8;
    private const int SaveDataIconPositionArrayOffset = 0xCB0;
    private const int SaveDataIconPositionsInFoldersOffset = 0xF80;

    // Private fields
    private byte[] originalLauncherData; // To store the original launcher data for comparison
    private byte[] originalSaveData;     // To store the original save data for comparison
    private ushort cardPosition;         // Store the card slot position separately
    public List<Title> SystemTitles { get; set; }
    public List<Title> SDTitles { get; private set; }
    public List<TitleFolder> Folders { get; private set; }

    public DataParser()
    {
        SDTitles = new List<Title>();
        SystemTitles = new List<Title>();
        Folders = new List<TitleFolder>();
        // Initialize Folders with 60 entries
        for (int i = 0; i < 60; i++)
        {
            Folders.Add(new TitleFolder());
        }
    }

    // Public methods

    public void ReadData(byte[] launcherData, byte[] saveData)
    {
        ReadLauncherData(launcherData);
        ReadSaveData(saveData);
    }


    public void SwapTitles(Title title1, Title title2)
    {
        if (title1 == null || title2 == null)
            throw new ArgumentNullException("One or both titles are null.");

        Console.WriteLine($"{title1.TitleID:X16} swapped with {title2.TitleID:X16}");

        // Remove titles from their current folders or home menu
        RemoveTitleFromFolder(title1);
        RemoveTitleFromFolder(title2);

        // Swap positions
        int tempPosition = title1.Position;
        title1.Position = title2.Position;
        title2.Position = tempPosition;

        // Swap folders
        var tempFolder = title1.Folder;
        title1.Folder = title2.Folder;
        title2.Folder = tempFolder;

        // Add titles to their new folders or home menu
        if (title1.Folder != null)
        {
            if (!title1.Folder.Titles.Contains(title1))
                title1.Folder.Titles.Add(title1);
        }
        else
        {
            EnsureHomeMenuPositionIsUnique(title1);
        }

        if (title2.Folder != null)
        {
            if (!title2.Folder.Titles.Contains(title2))
                title2.Folder.Titles.Add(title2);
        }
        else
        {
            EnsureHomeMenuPositionIsUnique(title2);
        }

        // Update cardPosition if one of the titles is the card title
        if (title1.IsCardTitle)
        {
            cardPosition = (ushort)title1.Position;
        }
        else if (title2.IsCardTitle)
        {
            cardPosition = (ushort)title2.Position;
        }
    }

    public void AddTitleToFolder(TitleFolder folder, Title title)
    {
        if (folder == null)
            throw new ArgumentNullException(nameof(folder));

        if (title == null)
            throw new ArgumentNullException(nameof(title));
        if(folder==title.Folder)
        {
            return;
        }
        // Remove title from its current folder or from the home menu
        RemoveTitleFromFolder(title);
        RemoveTitleFromHomeMenu(title);

        // Assign the new folder
        title.Folder = folder;

        // Find an unused position within the folder (0-59)
        int folderPosition = GetUnusedPositionInFolder(folder);
        if (folderPosition == -1)
            throw new Exception("No available positions in the folder.");

        title.Position = folderPosition;
        folder.Titles.Add(title);
    }

    public void RemoveTitleFromFolder(Title title)
    {
        if (title.Folder != null)
        {
            title.Folder.Titles.Remove(title);
            title.Folder = null;
        }
    }

    public void RemoveTitleFromHomeMenu(Title title)
    {
        if (title.Folder == null && title.Position >= 0)
        {
            // Remove the position assignment
            title.Position = -1;
        }
    }

    public void EnsureHomeMenuPositionIsUnique(Title title)
    {
        int position = title.Position;

        // Get positions of all titles on home menu excluding the current title
        var occupiedPositions = SystemTitles
            .Where(t => t.Folder == null && t.Position >= 0 && t != title)
            .Select(t => t.Position)
            .ToList();

        if (occupiedPositions.Contains(position) || position < 0)
        {
            // Find an unused position on the home menu
            int newPosition = GetUnusedHomeMenuPosition();
            if (newPosition == -1)
                throw new Exception("No available positions on the home menu.");

            title.Position = newPosition;
        }
    }

    public int GetUnusedHomeMenuPosition()
    {
        for (int i = 0; i < 300; i++) // Adjust the range as per valid positions on home menu
        {
            if (!SystemTitles.Any(t => t.Folder == null && t.Position == i))
            {
                return i;
            }
        }
        return -1;
    }

    public TitleFolder CreateFolder(string name, short position)
    {
        if (PositionExists(position))
        {
            position = GetUnusedPosition();
            if (position == -1)
                throw new Exception("No available position.");
        }

        int folderIndex = Folders.FindIndex(f => f.Position < 0);
        if (folderIndex == -1)
            throw new Exception("Cannot create more than 60 folders.");

        TitleFolder folder = new TitleFolder(name, 0, GetNextFolderNumber(), position);
        Folders[folderIndex] = folder;

        return folder;
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

    public void DisplayFoldersAndTitles()
    {
        // Display card position
        Console.WriteLine($"Card Position: {cardPosition}");

        // Display non-empty system titles not in folders
        Console.WriteLine("\nSystem Titles not in folders:");
        foreach (var title in SystemTitles)
        {
            if (title.Position >= 0 && title.Folder == null && !title.IsCardTitle && title.TitleID != ulong.MaxValue)
            {
                Console.WriteLine($"   Title ID: {title.TitleID:X16}, Position: {title.Position}");
            }
        }

        // Display non-empty SD titles not in folders
        Console.WriteLine("\nSD Titles not in folders:");
        foreach (var title in SDTitles)
        {
            if (title.Position >= 0 && title.Folder == null && title.TitleID != ulong.MaxValue)
            {
                Console.WriteLine($"   Title ID: {title.TitleID:X16}, Position: {title.Position}");
            }
        }

        // Display folders and titles inside them
        Console.WriteLine("\nFolders and Titles:");
        foreach (var folder in Folders)
        {
            if (folder.Position >= 0)
            {
                Console.WriteLine($"Folder Name: {folder.Name}, Position: {folder.Position}");
                if (folder.Titles != null && folder.Titles.Count > 0)
                {
                    foreach (var title in folder.Titles)
                    {
                        Console.WriteLine($"   Title ID: {title.TitleID:X16}, Position: {title.Position}");
                    }
                }
            }
        }
    }

    public void SaveLauncherData(byte[] data)
    {
        using MemoryStream stream = new MemoryStream(data);
        using BinaryWriter writer = new BinaryWriter(stream);

        // Write Title IDs
        writer.Seek(TitleIDArrayOffset, SeekOrigin.Begin);
        foreach (var title in SystemTitles)
        {
            writer.Write(title.TitleID);
        }

        // Write Card Slot Position
        writer.Seek(CardPositionOffset, SeekOrigin.Begin);
        writer.Write(cardPosition);

        // Write Icon Positions
        writer.Seek(IconPositionArrayOffset, SeekOrigin.Begin);
        foreach (var title in SystemTitles)
        {
            writer.Write((short)(title.Position));
        }

        // Write Folder Positions
        writer.Seek(FolderPositionsOffset, SeekOrigin.Begin);
        foreach (var folder in Folders)
        {
            writer.Write(folder.Position);
        }

        // Write Folder Names
        writer.Seek(FolderNamesOffset, SeekOrigin.Begin);
        foreach (var folder in Folders)
        {
            byte[] nameBytes = new byte[FolderNamesLength];
            byte[] folderNameBytes = Encoding.Unicode.GetBytes(folder.Name);
            Array.Copy(folderNameBytes, nameBytes, Math.Min(folderNameBytes.Length, nameBytes.Length));
            writer.Write(nameBytes);
        }

        // Write Folder Numbers
        writer.Seek(FolderNumbersOffset, SeekOrigin.Begin);
        foreach (var folder in Folders)
        {
            writer.Write(folder.FolderNumber);
        }

        // Write Icon Positions in Folders
        writer.Seek(IconPositionsInFoldersOffset, SeekOrigin.Begin);
        for (int i = 0; i < SystemTitles.Count; i++)
        {
            var title = SystemTitles[i];
            sbyte folderIndex = -1;
            if (title.Folder != null)
            {
                folderIndex = (sbyte)Folders.IndexOf(title.Folder);
            }
            writer.Write(folderIndex);
        }

        // Write Next Folder Number
        writer.Seek(NextFolderNumberOffset, SeekOrigin.Begin);
        uint nextFolderNumber = Folders.Where(f => f.FolderNumber > 0).Max(f => f.FolderNumber) + 1;
        writer.Write(nextFolderNumber);

        // Compare data after writing (optional)
        // CompareWithOriginal(originalLauncherData, data);
    }

    public void SaveSaveData(byte[] data)
    {
        using MemoryStream stream = new MemoryStream(data);
        using BinaryWriter writer = new BinaryWriter(stream);

        // Write Title IDs
        writer.Seek(SaveDataTitleIDArrayOffset, SeekOrigin.Begin);
        foreach (var title in SDTitles)
        {
            writer.Write(title.TitleID);
        }

        // Write Icon Positions
        writer.Seek(SaveDataIconPositionArrayOffset, SeekOrigin.Begin);
        foreach (var title in SDTitles)
        {
            writer.Write((short)(title.Position));
        }

        // Write Icon Positions in Folders
        writer.Seek(SaveDataIconPositionsInFoldersOffset, SeekOrigin.Begin);
        for (int i = 0; i < SDTitles.Count; i++)
        {
            var title = SDTitles[i];
            sbyte folderIndex = -1;
            if (title.Folder != null)
            {
                folderIndex = (sbyte)Folders.IndexOf(title.Folder);
            }
            writer.Write(folderIndex);
        }

        // Compare data after writing (optional)
        // CompareWithOriginal(originalSaveData, data);
    }

    public void ExtractSmallIconsFromSMDH(string smdhDirectoryPath)
    {
        foreach (Title title in SystemTitles.Concat(SDTitles))
        {
            if (title.TitleID == ulong.MaxValue)
                continue;

            string titleIDHex = title.TitleID.ToString("X16").ToLowerInvariant();
            int halfLength = titleIDHex.Length / 2;
            string hyphenatedTitleID = titleIDHex.Insert(halfLength, "-");

            string[] possibleNames = { titleIDHex, hyphenatedTitleID };
            string imagePath = null;

            foreach (string possibleName in possibleNames)
            {
                imagePath = Path.Combine(smdhDirectoryPath, possibleName + ".jpg");
                if (File.Exists(imagePath))
                    break;

                string smdhFilePath = Path.Combine(smdhDirectoryPath, possibleName + ".smdh");
                if (File.Exists(smdhFilePath))
                {
                    SMDH smdhParser = new SMDH();
                    smdhParser.Load(smdhFilePath);
                    smdhParser.BigIcon.Save(imagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    Console.WriteLine($"Icon extracted for Title ID: {titleIDHex}");
                    break;
                }
            }
        }
    }

    public bool CompareWithOriginal(byte[] originalData, byte[] modifiedData)
    {
        if (originalData == null || modifiedData == null || originalData.Length != modifiedData.Length)
        {
            throw new Exception("Original data or modified data is null, or they have different lengths.");
        }

        for (int i = 0; i < originalData.Length; i++)
        {
            if (originalData[i] != modifiedData[i])
            {
                int startOffset = Math.Max(0, i - 5);
                int endOffset = Math.Min(modifiedData.Length - 1, i + 5);

                string previousBytes = BitConverter.ToString(modifiedData, startOffset, i - startOffset);
                string nextBytes = BitConverter.ToString(modifiedData, i + 1, endOffset - i);
                string currentByte = modifiedData[i].ToString("X2");
                string originalByte = originalData[i].ToString("X2");

                throw new Exception($"Difference found at offset 0x{i:X}. Previous bytes: [{previousBytes}], Current byte: [{currentByte}], Original byte: [{originalByte}], Next bytes: [{nextBytes}]");
            }
        }
        return true;
    }

    public void ReadLauncherData(byte[] data)
    {
        // Store original data for comparison
        originalLauncherData = new byte[data.Length];
        Array.Copy(data, originalLauncherData, data.Length);

        SystemTitles.Clear();
        // Folders are already initialized in the constructor

        using MemoryStream stream = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(stream);

        GetSystemTitleIDs(reader);
        GetCartSlotPosition(reader);
        GetSystemTitleIDPositions(reader);
        GetFolders(reader);
        GetFolderNames(reader);
        GetFolderNumbers(reader);
        GetSystemTitlesFolderContentPositions(reader);
    }

    public void ReadSaveData(byte[] data)
    {
        // Store original data for comparison
        originalSaveData = new byte[data.Length];
        Array.Copy(data, originalSaveData, data.Length);

        SDTitles.Clear();

        using MemoryStream stream = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(stream);

        // Read Title IDs
        reader.BaseStream.Seek(SaveDataTitleIDArrayOffset, SeekOrigin.Begin);
        for (int i = 0; i < 360; i++)
        {
            ulong titleID = reader.ReadUInt64();
            Title title = new Title(titleID, TitleType.SD);
            SDTitles.Add(title);
        }

        // Read Icon Positions
        reader.BaseStream.Seek(SaveDataIconPositionArrayOffset, SeekOrigin.Begin);
        foreach (var title in SDTitles)
        {
            short position = reader.ReadInt16();
            title.Position = position;
        }

        // Read Icon Positions in Folders
        reader.BaseStream.Seek(SaveDataIconPositionsInFoldersOffset, SeekOrigin.Begin);
        sbyte[] folderIconPositions = reader.ReadBytes(360).Select(b => unchecked((sbyte)b)).ToArray();

        for (int i = 0; i < SDTitles.Count; i++)
        {
            sbyte folderIndex = folderIconPositions[i];
            if (folderIndex >= 0 && folderIndex < Folders.Count)
            {
                var folder = Folders[folderIndex];
                folder.Titles.Add(SDTitles[i]);
                SDTitles[i].Folder = folder;
            }
        }
    }

    public void GetSystemTitleIDs(BinaryReader reader)
    {
        reader.BaseStream.Seek(TitleIDArrayOffset, SeekOrigin.Begin);
        for (int i = 0; i < 360; i++)
        {
            ulong titleID = reader.ReadUInt64();
            Title title = new Title(titleID, TitleType.System);
            SystemTitles.Add(title);
        }
    }

    public void GetCartSlotPosition(BinaryReader reader)
    {
        reader.BaseStream.Seek(CardPositionOffset, SeekOrigin.Begin);
        cardPosition = reader.ReadUInt16();

        if (cardPosition < SystemTitles.Count)
        {
            Title cardTitle = SystemTitles[cardPosition];
            cardTitle.IsCardTitle = true;
        }
    }

    public void GetSystemTitleIDPositions(BinaryReader reader)
    {
        reader.BaseStream.Seek(IconPositionArrayOffset, SeekOrigin.Begin);
        for (int i = 0; i < SystemTitles.Count; i++)
        {
            short position = reader.ReadInt16();
            SystemTitles[i].Position = position;
        }
    }

    public void GetFolders(BinaryReader reader)
    {
        reader.BaseStream.Seek(FolderPositionsOffset, SeekOrigin.Begin);
        for (int i = 0; i < 60; i++)
        {
            Folders[i].Position = reader.ReadInt16();
        }
    }

    public void GetFolderNames(BinaryReader reader)
    {
        reader.BaseStream.Seek(FolderNamesOffset, SeekOrigin.Begin);
        for (int i = 0; i < 60; i++)
        {
            byte[] nameBytes = reader.ReadBytes(FolderNamesLength);
            string folderName = Encoding.Unicode.GetString(nameBytes).TrimEnd('\0');
            Folders[i].Name = folderName;
        }
    }

    public void GetFolderNumbers(BinaryReader reader)
    {
        reader.BaseStream.Seek(FolderNumbersOffset, SeekOrigin.Begin);
        for (int i = 0; i < 60; i++)
        {
            Folders[i].FolderNumber = reader.ReadUInt32();
        }
    }

    public void GetSystemTitlesFolderContentPositions(BinaryReader reader)
    {
        reader.BaseStream.Seek(IconPositionsInFoldersOffset, SeekOrigin.Begin);
        sbyte[] iconPositions = reader.ReadBytes(360).Select(b => unchecked((sbyte)b)).ToArray();

        for (int i = 0; i < SystemTitles.Count; i++)
        {
            sbyte folderIndex = iconPositions[i];
            if (folderIndex >= 0 && folderIndex < Folders.Count)
            {
                var folder = Folders[folderIndex];
                folder.Titles.Add(SystemTitles[i]);
                SystemTitles[i].Folder = folder;
            }
        }
    }

   

    public void UpdateFolderTitleList(Title title)
    {
        if (title.Folder != null && !title.Folder.Titles.Contains(title))
        {
            title.Folder.Titles.Add(title);
        }
    }

    public uint GetNextFolderNumber()
    {
        uint maxFolderNumber = Folders.Where(f => f.FolderNumber > 0).Select(f => f.FolderNumber).Max();
        return maxFolderNumber + 1;
    }

    public short GetUnusedPosition()
    {
        for (short i = 0; i < 360; i++)
        {
            if (!PositionExists(i))
            {
                return i;
            }
        }
        return -1;
    }

    public bool PositionExists(short position)
    {
        return SystemTitles.Any(t => t.Position == position) ||
               SDTitles.Any(t => t.Position == position) ||
               Folders.Any(f => f.Position == position);
    }

    public int GetUnusedPositionInFolder(TitleFolder folder)
    {
        for (int i = 0; i < 60; i++)
        {
            if (!folder.Titles.Any(t => t.Position == i))
            {
                return i;
            }
        }
        return -1;
    }

    public Title? GetCardTitle()
    {
        return SystemTitles.FirstOrDefault(t => t.IsCardTitle);
    }
}
