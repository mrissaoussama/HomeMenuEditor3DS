using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace SMDH_Creator
{//from smdh creator
    public class smdhHeader
    {
        public UInt32 magic;
        public UInt16 version, reserved;
    }
    public class smdhTitle
    {
        public UInt16[] shortDescription = new UInt16[0x40], longDescription = new UInt16[0x80], publisher = new UInt16[0x40];
    }
    class smdhSettings
    {
        public byte[] gameRatings = new byte[0x10];
        public UInt32 regionLock;
        public byte[] matchMakerId = new byte[0xC];
        public UInt32 flags;
        public UInt16 eulaVersion, reserved;
        public UInt32 defaultFrame, cecId;
    }
    class SMDH
    {
        private FileStream file;

        private smdhHeader header = new smdhHeader();
        private smdhTitle[] applicationTitles = new smdhTitle[16];
        private smdhSettings settings = new smdhSettings();
        private byte[] reserved = new byte[0x8];
        private UInt16[] bigIconData = new UInt16[0x900], smallIconData = new UInt16[0x240];

        private Bitmap smallIcon = new Bitmap(24, 24), bigIcon = new Bitmap(48, 48);

        public bool Valid
        {
            get
            {
                return header.magic == 0x48444D53;
            }
        }

        public UInt32 Version
        {
            get
            {
                return this.header.version;
            }
        }

        public String GetShortDescription(int appTitleID)
        {
            String text = "";
            foreach (char c in this.applicationTitles[appTitleID].shortDescription)
            {
                text += c;
            }
            return text;
        }

        public void SetShortDescription(int appTitleID, string description)
        {
            for (int i = 0; i < this.applicationTitles[appTitleID].shortDescription.Length; i += 1)
            {
                if (i < description.Length)
                {
                    this.applicationTitles[appTitleID].shortDescription[i] = description[i];
                }
                else
                {
                    this.applicationTitles[appTitleID].shortDescription[i] = 0;
                }
            }
        }

        public String GetLongDescription(int appTitleID)
        {
            String text = "";
            foreach (char c in this.applicationTitles[appTitleID].longDescription)
            {
                text += c;
            }
            return text;
        }

        public void SetLongDescription(int appTitleID, string description)
        {
            for (int i = 0; i < this.applicationTitles[appTitleID].longDescription.Length; i += 1)
            {
                if (i < description.Length)
                {
                    this.applicationTitles[appTitleID].longDescription[i] = description[i];
                }
                else
                {
                    this.applicationTitles[appTitleID].longDescription[i] = 0;
                }
            }
        }

        public String GetPublisher(int appTitleID)
        {
            String text = "";
            foreach (char c in this.applicationTitles[appTitleID].publisher)
            {
                text += c;
            }
            return text;
        }

        public void SetPublisher(int appTitleID, string publisher)
        {
            if (this.applicationTitles[appTitleID] == null)
            {
                this.applicationTitles[appTitleID] = new smdhTitle();
            }
            for (int i = 0; i < this.applicationTitles[appTitleID].publisher.Length; i += 1)
            {
                if (i < publisher.Length)
                {
                    this.applicationTitles[appTitleID].publisher[i] = publisher[i];
                }
                else
                {
                    this.applicationTitles[appTitleID].publisher[i] = 0;
                }
            }
        }

        public Bitmap SmallIcon
        {
            get
            {
                return this.smallIcon;
            }
            set
            {
                this.smallIcon = value;
                this.convertSmallIcon(false);
            }
        }
        public Bitmap BigIcon
        {
            get
            {
                return this.bigIcon;
            }
            set
            {
                this.bigIcon = value;
                this.convertBigIcon(false);
            }
        }

        private byte getU8()
        {
            byte[] temp = new byte[1];
            file.Read(temp, 0, 1);
            return temp[0];
        }

        private UInt16 getU16()
        {
            byte[] temp = new byte[2];
            file.Read(temp, 0, 2);
            return BitConverter.ToUInt16(temp, 0);
        }

        private UInt32 getU32()
        {
            byte[] temp = new byte[4];
            file.Read(temp, 0, 4);
            return BitConverter.ToUInt32(temp, 0);
        }

        private void readHeader()
        {
            this.header.magic = this.getU32();
            this.header.version = this.getU16();
            this.header.reserved = this.getU16();
        }

        private void readTitle(int titleId)
        {
            this.applicationTitles[titleId] = new smdhTitle();
            for (int i = 0; i < 0x40; i += 1)
            {
                this.applicationTitles[titleId].shortDescription[i] = this.getU16();
            }
            for (int i = 0; i < 0x80; i += 1)
            {
                this.applicationTitles[titleId].longDescription[i] = this.getU16();
            }
            for (int i = 0; i < 0x40; i += 1)
            {
                this.applicationTitles[titleId].publisher[i] = this.getU16();
            }
        }

        private void readTitles()
        {
            for (int i = 0; i < applicationTitles.Length; i += 1)
            {
                this.readTitle(i);
            }
        }

        private void readSettings()
        {
            for (int i = 0; i < 0x10; i += 1)
            {
                this.settings.gameRatings[i] = this.getU8();
            }
            this.settings.regionLock = this.getU32();
            for (int i = 0; i < 0xC; i += 1)
            {
                this.settings.matchMakerId[i] = this.getU8();
            }
            this.settings.flags = this.getU32();
            this.settings.eulaVersion = this.getU16();
            this.settings.reserved = this.getU16();
            this.settings.defaultFrame = this.getU32();
            this.settings.cecId = this.getU32();
        }

        private void readReserved()
        {
            for (int i = 0; i < 0x8; i += 1)
            {
                this.reserved[i] = this.getU8();
            }
        }

        private void readSmallIcon()
        {
            for (int i = 0; i < this.smallIconData.Length; i += 1)
            {
                this.smallIconData[i] = this.getU16();
            }
        }

        private void readBigIcon()
        {
            for (int i = 0; i < this.bigIconData.Length; i += 1)
            {
                this.bigIconData[i] = this.getU16();
            }
        }
        byte[] tileOrder = { 0, 1, 8, 9, 2, 3, 10, 11, 16, 17, 24, 25, 18, 19, 26, 27, 4, 5, 12, 13, 6, 7, 14, 15, 20, 21, 28, 29, 22, 23, 30, 31, 32, 33, 40, 41, 34, 35, 42, 43, 48, 49, 56, 57, 50, 51, 58, 59, 36, 37, 44, 45, 38, 39, 46, 47, 52, 53, 60, 61, 54, 55, 62, 63 };

        private void convertBigIcon(bool toBitmap)
        {
            if (toBitmap)
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 48; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 48; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;
                            int color = this.bigIconData[i];
                            i += 1;

                            int b = (color & 0x1f) << 3;
                            int g = ((color >> 5) & 0x3f) << 2;
                            int r = ((color >> 11) & 0x1f) << 3;

                            this.bigIcon.SetPixel(x + tile_x, y + tile_y, Color.FromArgb(r, g, b));
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 48; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 48; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;

                            int r = this.bigIcon.GetPixel(x + tile_x, y + tile_y).R >> 3;
                            int g = this.bigIcon.GetPixel(x + tile_x, y + tile_y).G >> 2;
                            int b = this.bigIcon.GetPixel(x + tile_x, y + tile_y).B >> 3;

                            this.bigIconData[i] = (UInt16)((r << 11) | (g << 5) | b);
                            i += 1;
                        }
                    }
                }
            }
        }
        private void convertSmallIcon(bool toBitmap)
        {
            if (toBitmap)
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 24; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 24; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;
                            int color = this.smallIconData[i];
                            i += 1;

                            int b = (color & 0x1f) << 3;
                            int g = ((color >> 5) & 0x3f) << 2;
                            int r = ((color >> 11) & 0x1f) << 3;

                            this.smallIcon.SetPixel(x + tile_x, y + tile_y, Color.FromArgb(r, g, b));
                        }
                    }
                }
            }
            else
            {
                int i = 0;
                for (int tile_y = 0; tile_y < 24; tile_y += 8)
                {
                    for (int tile_x = 0; tile_x < 24; tile_x += 8)
                    {
                        for (int k = 0; k < 8 * 8; k += 1)
                        {
                            int x = tileOrder[k] & 0x7;
                            int y = tileOrder[k] >> 3;

                            int r = this.smallIcon.GetPixel(x + tile_x, y + tile_y).R >> 3;
                            int g = this.smallIcon.GetPixel(x + tile_x, y + tile_y).G >> 2;
                            int b = this.smallIcon.GetPixel(x + tile_x, y + tile_y).B >> 3;

                            this.smallIconData[i] = (UInt16)((r << 11) | (g << 5) | b);
                            i += 1;
                        }
                    }
                }
            }
        }

        public void Load(String fileName)
        {
            file = File.OpenRead(fileName);
            byte[] temp = new byte[4];

            this.readHeader();
            if (this.Valid)
            {
                this.readTitles();
                this.readSettings();
                this.readReserved();
                this.readSmallIcon();
                this.readBigIcon();

                this.convertSmallIcon(true);
                this.convertBigIcon(true);
            }
            file.Close();
        }

        public void Save(String fileName)
        {
            file = File.OpenWrite(fileName);
            BinaryWriter writer = new BinaryWriter(file);

            this.header.magic = 0x48444D53;
            writer.Write(this.header.magic);
            writer.Write(this.header.version);
            writer.Write(this.header.reserved);

            for (int i = 0; i < applicationTitles.Length; i += 1)
            {
                if (this.applicationTitles[i] == null)
                {
                    this.applicationTitles[i] = new smdhTitle();
                }
                for (int j = 0; j < this.applicationTitles[i].shortDescription.Length; j += 1)
                {
                    writer.Write(this.applicationTitles[i].shortDescription[j]);
                }
                for (int j = 0; j < this.applicationTitles[i].longDescription.Length; j += 1)
                {
                    writer.Write(this.applicationTitles[i].longDescription[j]);
                }
                for (int j = 0; j < this.applicationTitles[i].publisher.Length; j += 1)
                {
                    writer.Write(this.applicationTitles[i].publisher[j]);
                }
            }

            for (int i = 0; i < this.settings.gameRatings.Length; i += 1)
            {
                writer.Write(this.settings.gameRatings[i]);
            }
            writer.Write(this.settings.regionLock);
            for (int i = 0; i < this.settings.matchMakerId.Length; i += 1)
            {
                writer.Write(this.settings.matchMakerId[i]);
            }
            writer.Write(this.settings.flags);
            writer.Write(this.settings.eulaVersion);
            writer.Write(this.settings.reserved);
            writer.Write(this.settings.defaultFrame);
            writer.Write(this.settings.cecId);

            for (int i = 0; i < this.reserved.Length; i += 1)
            {
                writer.Write(this.reserved[i]);
            }
            for (int i = 0; i < this.smallIconData.Length; i += 1)
            {
                writer.Write(this.smallIconData[i]);
            }
            for (int i = 0; i < this.bigIconData.Length; i += 1)
            {
                writer.Write(this.bigIconData[i]);
            }

            writer.Close();
            file.Close();
        }
    }
}