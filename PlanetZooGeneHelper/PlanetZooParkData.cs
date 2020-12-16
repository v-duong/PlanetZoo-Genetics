using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PlanetZooGeneHelper
{
    public class PlanetZooParkData
    {
        private static byte[] animalSerialization_startBytes = Encoding.ASCII.GetBytes("AnimalSerialisation");
        private static byte[] animalSerialization_endBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01 };
        private static byte[] animalDataStore_startBytes = Encoding.ASCII.GetBytes("LocalAnimalDatastore");
        private static byte[] localAnimalExchange_startBytes = Encoding.ASCII.GetBytes("LocalAnimalExchangeManager");
        private static byte[] nameSearch_Sandbox = new byte[] { 0xF3, 0x00, 0x00, 0xF3, 0x00, 0x00, 0xF3, 0x00 };
        private static byte[] nameSearch_Franchise = new byte[] { 0xF3, 0x00, 0x00, 0xF3, 0x00 };

        private static byte[] stringList_startBytes = Encoding.ASCII.GetBytes("Strings>");
        private static byte[] stringList_endBytes = Encoding.ASCII.GetBytes("String>");

        private long serializationSection_start;
        private long datastoreSection_start;
        private long datastoreSection_end;

        public List<AnimalData> Animals;
        private List<string> StringsList;
        public Gamemode Gamemode;

        public PlanetZooParkData(Stream stream)
        {
            StringsList = new List<string>();
            Animals = new List<AnimalData>();
            ReadData(stream);
        }

        private void ReadData(Stream stream)
        {
            ReadStringList(stream);
            GetGameMode();
            if (Gamemode == Gamemode.SCENARIO)
            {
                MessageBox.Show("Scenario Mode saves are currently not supported.", "Unsupported Save Type", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            serializationSection_start = Helper.Seek(stream, animalSerialization_startBytes) + animalSerialization_startBytes.Length + 3;
            datastoreSection_start = Helper.Seek(stream, animalDataStore_startBytes) + animalDataStore_startBytes.Length;
            datastoreSection_end = Helper.Seek(stream, localAnimalExchange_startBytes);

            ReadAnimalData(stream, StringsList);
        }

        private void ReadStringList(Stream stream)
        {
            byte[] buffer;
            long startIndex = Helper.Seek(stream, stringList_startBytes);
            long endIndex = Helper.Seek(stream, stringList_endBytes);

            long length = endIndex - startIndex;

            stream.Seek(startIndex + stringList_startBytes.Length + 2, SeekOrigin.Begin);
            buffer = new byte[length];
            stream.Read(buffer, 0, (int)length);
            var list = Encoding.ASCII.GetString(buffer).Split('\0');
            for (int i = 0; i < list.Length; i++)
            {
                StringsList.Add(list[i].Trim(' ', '?'));
            }
        }

        private void GetGameMode()
        {
            string gameModeString = StringsList[StringsList.FindIndex(x => x == "sGameMode") + 1];
            if (gameModeString.Contains("Scenario"))
            {
                Gamemode = Gamemode.SCENARIO;
            }
            else if (gameModeString == "Sandbox")
            {
                Gamemode = Gamemode.SANDBOX;
            }
            else if (gameModeString == "Franchise")
            {
                Gamemode = Gamemode.FRANCHISE;
            }
        }

        private void ReadAnimalData(Stream stream, List<string> stringsList)
        {
            byte[] buffer = new byte[1024];
            stream.Seek(serializationSection_start, SeekOrigin.Begin);
            stream.Read(buffer, 0, 1);
            Debug.WriteLine("Expected number of entries: " + (int)buffer[0]);

            while (true)
            {
                long animalDataBlock_Start = stream.Position;
                stream.Read(buffer, 0, 1024);

                if (buffer[0] == 0x00)
                    break;

                ReadOnlySpan<byte> ro = buffer;
                long animalDataBlock_End = animalDataBlock_Start + ro.IndexOf(animalSerialization_endBytes) + animalSerialization_endBytes.Length;

                AnimalData animal = new AnimalData();
                Animals.Add(animal);

                if (Gamemode == Gamemode.FRANCHISE)
                {
                    if (Helper.CheckFirstFourBits(buffer[0], 0xC0))
                    {
                        animal.AnimalId = Helper.ConvertLastThreeBytes(buffer[0], buffer[1]);

                        if (Helper.CheckFirstFourBits(buffer[2], 0xC0))
                            animal.SpeciesId = Helper.ConvertLastThreeBytes(buffer[2], buffer[3]);
                        else
                            animal.SpeciesId = buffer[2];
                    }
                    else
                    {
                        animal.AnimalId = buffer[0];
                        animal.SpeciesId = buffer[1];
                    }
                }
                else
                {
                    if (BitConverter.IsLittleEndian)
                        animal.AnimalId = BitConverter.ToUInt32(buffer.Skip(1).Take(4).Reverse().ToArray(), 0);
                    else
                        animal.AnimalId = BitConverter.ToUInt32(buffer.Skip(1).Take(4).ToArray(), 0);
                    animal.SpeciesId = buffer[5];
                }

                if (stringsList.Count >= animal.SpeciesId)
                    animal.Species = stringsList[(int)animal.SpeciesId];

                int genePos = 0;
                IEnumerable<byte> geneSequence = FallbackGeneSearch(buffer, stream, ref genePos);

                // 8 bytes that we will use to find the animal's name and verify genes
                byte[] geneIdBytes = new byte[8];
                for (int i = genePos; i < buffer.Length; i++)
                {
                    if (Helper.CheckFirstFourBits(buffer[i], 0x80))
                    {
                        geneIdBytes = buffer.Skip(i).Take(8).ToArray();
                        break;
                    }
                }

                animal.GeneId = BitConverter.ToUInt64(geneIdBytes, 0);

                stream.Seek(datastoreSection_start, SeekOrigin.Begin);
                long geneIdPosition = Helper.Seek(stream, geneIdBytes, datastoreSection_end);

                byte[] buffer2 = new byte[60];
                stream.Seek(geneIdPosition + 8, SeekOrigin.Begin);
                stream.Read(buffer2, 0, 60);
                ro = buffer2;

                //Verify gene, both should be the same; only type of error should be gene being offset by some number of bytes
                // If they don't match, check if a subset of the 1st gene sequence is within the 2nd gene sequence
                // else, keep checking ahead for the ID and repeat
                if (ro.IndexOf(geneSequence.ToArray()) == -1)
                {
                    if (ro.IndexOf(geneSequence.Skip(8).ToArray()) != -1)
                    {
                        geneSequence = ro.ToArray();
                    }
                    else
                    {
                        long geneIdPosition2 = Helper.Seek(stream, geneIdBytes, datastoreSection_end);
                        while (geneIdPosition2 != -1)
                        {
                            stream.Seek(geneIdPosition2 + 8, SeekOrigin.Begin);
                            stream.Read(buffer2, 0, 60);
                            ro = buffer2;
                            if (ro.IndexOf(geneSequence.ToArray()) == -1 && ro.IndexOf(geneSequence.Skip(8).ToArray()) != -1)
                            {
                                geneIdPosition2 = Helper.Seek(stream, geneIdBytes, datastoreSection_end);
                            }
                            else
                            {
                                geneIdPosition = geneIdPosition2;
                                break;
                            }
                        }
                    }
                }

                if (Gene.IsValidGeneSequence(geneSequence))
                {
                    animal.SetGene(geneSequence);
                }

                animal.Name = ReadAnimalDataStore(animal, stream, geneIdPosition, AnimalData.GENE_LENGTH * 5 + geneIdBytes.Length);

                stream.Seek(animalDataBlock_End, SeekOrigin.Begin);
            }

            Debug.WriteLine("Found " + Animals.Count);
        }

        //TODO: find better method
        // Not 100% accurate
        private IEnumerable<byte> FallbackGeneSearch(byte[] buffer, Stream stream, ref int returnPos)
        {
            Queue<byte> geneSequence = new Queue<byte>();
            for (int i = 0; i < buffer.Length; i++)
            {
                byte b = buffer[i];
                if (b <= 0x03)
                {
                    geneSequence.Enqueue(b);
                }
                else
                {
                    geneSequence.Clear();
                }
                if (geneSequence.Count == 60)
                {
                    long currentPosition = stream.Position;
                    stream.Seek(datastoreSection_start, SeekOrigin.Begin);
                    long searchResult = Helper.Seek(stream, geneSequence.ToArray(), datastoreSection_end);
                    stream.Seek(currentPosition, SeekOrigin.Begin);
                    if (searchResult != -1)
                    {
                        returnPos = i;
                        return geneSequence;
                    }
                    else
                    {
                        geneSequence.Dequeue();
                    }
                }
            }

            return null;
        }

        /* First 12 bytes seem to be some unknown gene
         * The next byte is the animal's sex
         *
         * Name is (most of the time) stored as a reference to a string in string list.
         * Only interested in the 12 bits, marked as '?' below.
         * Format: 0xC?, ??, 0x0, 0x0, 0x0, 0x0 (preceded by some series of 0x00 and 0xF3)
         * There may be multiple references but we just take the last one for now.
        */

        private string ReadAnimalDataStore(AnimalData animal, Stream stream, long geneIdPosition, int initialSkip)
        {
            string name = "";
            byte[] buffer = new byte[128];
            stream.Seek(geneIdPosition + initialSkip, SeekOrigin.Begin);
            stream.Read(buffer, 0, 128);
            ReadOnlySpan<byte> ro = buffer;

            animal.SetGene(buffer.Take(12).ToArray(), GeneType.UNKNOWN_2);
            animal.Sex = buffer.Skip(12).Take(1).First();

            int searchPos;

            if (Gamemode == Gamemode.FRANCHISE)
            {
                searchPos = ro.IndexOf(nameSearch_Franchise);
            }
            else
            {
                searchPos = ro.IndexOf(nameSearch_Sandbox);
            }

            if (searchPos != -1)
            {
                int i = 0, tempNamePos = searchPos;
                int zeroCount = 0;
                bool foundSomeReference = false;
                for (i = searchPos; i < ro.Length; i++)
                {
                    if ((Helper.CheckFirstFourBits(ro[i], 0xC0)) && zeroCount > 0)
                    {
                        foundSomeReference = true;
                        tempNamePos = i;
                        zeroCount = 0;
                    }
                    else if (ro[i] == 0x00)
                    {
                        zeroCount++;
                        if (zeroCount >= 4)
                            break;
                    }
                    else
                    {
                        zeroCount = 0;
                    }
                }

                // Sometimes the name is stored here instead of in the string list.
                // Format in this case: 0xF3, [ character bytes ], 0x0, 0x0, 0x0, 0x0
                if (i - tempNamePos > 7 || !foundSomeReference)
                {
                    zeroCount = 0;

                    for (i = tempNamePos; i < ro.Length; i++)
                    {
                        if (ro[i] == 0xF3)
                        {
                            zeroCount = 0;
                            name = "";
                        }
                        else if (ro[i] == 0x00)
                        {
                            zeroCount++;
                            if (zeroCount >= 4)
                            {
                                return name;
                            }
                        }
                        else
                        {
                            zeroCount = 0;
                            name += (char)ro[i];
                        }
                    }
                }
                else
                {
                    int nameIndex = ((ro[tempNamePos] & 0x0F) << 8) + ro[tempNamePos + 1];
                    if (nameIndex < StringsList.Count)
                        name = StringsList[nameIndex];
                    else
                    {
                        name = "";
#if DEBUG
                        string debugString = "";
                        foreach (byte b in buffer)
                        {
                            debugString += b.ToString("X2");
                        }
                        Debug.WriteLine(debugString);
#endif
                    }
                }
            }

            return name;
        }
    }

    public enum Gamemode
    {
        SCENARIO,
        SANDBOX,
        FRANCHISE
    }
}