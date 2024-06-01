//Written for Ragnarok Online 2. https://store.steampowered.com/app/231060
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace Ragnarok_Online_2_Extractor
{
    class Program
    {
        static BinaryReader br;
        static string path;
        static void Main(string[] args)
        {
            br = new BinaryReader(File.OpenRead(args[0]));
            if (new string(br.ReadChars(8)) != "VDISK1.1")
                throw new System.Exception("Not a Ragnarok Online 2 VDK file.");

            path = Path.GetDirectoryName(args[0]) + "\\";
            int unknown1 = br.ReadInt32();
            int fileCount = br.ReadInt32();
            int folderCount = br.ReadInt32();
            int tableStart = br.ReadInt32() + 145;
            int unknown2 = br.ReadInt32();

            br.BaseStream.Position = tableStart;
            List<TableEntry> table = Table();

            foreach (TableEntry file in table)
            {
                br.BaseStream.Position = file.start;
                Subfile(file.name);
            }
        }

        static List<TableEntry> Table()
        {
            int count = br.ReadInt32();
            List<TableEntry> table = new();
            for (int i = 0; i < count; i++)
                table.Add(new());

            return table;
        }

        public class TableEntry
        {
            public string name = new string(br.ReadChars(260)).TrimEnd('\0');
            public int start = br.ReadInt32();
        }

        static void Subfile(string fullName)
        {
            byte isFolder = br.ReadByte();
            string name = new string(br.ReadChars(128)).TrimEnd('\0');
            int sizeUncompressed = br.ReadInt32();
            int sizeCompressed = br.ReadInt32();
            int unknown1 = br.ReadInt32();
            int end = br.ReadInt32();

            Directory.CreateDirectory(path + Path.GetDirectoryName(fullName));//We already know the file path from table, so i don't need to understand the folder data.

            if (isFolder == 1)//This should never happen if we go directly to the file data position given in table, which is good, because i don't understand it.
                throw new System.Exception("Fuck!");

            FileStream fs = File.Create(path + fullName);
            if (sizeUncompressed == sizeCompressed)
            {
                BinaryWriter bw = new(fs);
                bw.Write(br.ReadBytes(sizeUncompressed));
                return;
            }
            br.ReadInt16();
            using (var ds = new DeflateStream(new MemoryStream(br.ReadBytes(sizeCompressed)), CompressionMode.Decompress))
                ds.CopyTo(fs);
        }
    }
}
