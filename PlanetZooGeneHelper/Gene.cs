using System.Collections.Generic;
using System.Linq;

namespace PlanetZooGeneHelper
{
    public enum GeneValueType
    {
        HOMOGENEITY,
        DIVERSITY
    }

    public class Gene
    {
        public byte[] geneBytes;
        public string geneString;

        public Gene()
        {
            geneBytes = new byte[12];
            geneString = "";
        }

        public Gene(byte[] geneArr)
        {
            Set(geneArr);
        }

        public void Set(byte[] geneArr)
        {
            geneBytes = geneArr;
            string s = "";
            for (int i = 0; i < 6; i++)
            {
                s += ByteToGeneChar(geneArr[i]);
                s += ByteToGeneChar(geneArr[i + 6]);
                s += " ";
            }
#if DEBUG
            s += "\n";
            foreach(byte b in geneArr)
            {
                s += b.ToString("X2") + " ";
            }
#endif
            geneString = s;
        }

        private int CountAandB()
        {
            int valueCount = 0;
            foreach (byte b in geneBytes)
            {
                if (b < 0x02)
                    valueCount++;
            }
            return valueCount;
        }

        private int FindSimilarGenes()
        {
            string[] splitStrings = geneString.Split(' ');
            HashSet<int> similars = new HashSet<int>();
            for (int i = 0; i < 3; i++)
            {
                if (splitStrings[i][0] == splitStrings[i][1])
                    similars.Add(i);
                if (splitStrings[i + 3][0] == splitStrings[i + 3][1])
                    similars.Add(i + 3);
            }

            return similars.Count;
        }

        public float GetGeneValue(GeneValueType type)
        {
            switch (type)
            {
                case GeneValueType.HOMOGENEITY:
                    return CountAandB() / 12f;
                case GeneValueType.DIVERSITY:
                    return 1f - FindSimilarGenes() / 6f;
            }

            return 0f;
        }

        public static char ByteToGeneChar(byte b)
        {
            return (char)('A' + b);
        }

        public static bool IsValidGeneSequence(IEnumerable<byte> genome)
        {
            if (genome == null || genome.Count() != 60)
                return false;
            foreach (byte b in genome)
            {
                if (b > 0x03)
                {
                    return false;
                }
            }
            return true;
        }
    }
}