public class TitleSlot
{
    public string TitleID { get; set; }
    public int Position { get; set; }
    public TitleFolder? Folder { get; set; }
    public TitleSlot(string titleID, int position)
    {
        TitleID = titleID;
        Position = position;
     
    }
}


