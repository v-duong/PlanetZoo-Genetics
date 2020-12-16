using System.Collections.Generic;

namespace PlanetZooGeneHelper
{
    public class PairingData
    {
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        private Dictionary<string, OffspringData> offspringList;
        public OffspringData BestOffspring { get; set; }
        public int BestSizeLongevity { get; set; }
        public int BestFertilityImmunity { get; set; }
        public List<OffspringData> publicView { get; set; }
        public float AverageTotal { get; set; }
        public float AverageSize { get; set; }
        public float AverageLongevity { get; set; }
        public float AverageFertility { get; set; }
        public float AverageImmunity { get; set; }

        public PairingData(AnimalData female, AnimalData male)
        {
            offspringList = new Dictionary<string, OffspringData>();
            MotherName = female.Name;
            FatherName = male.Name;
            BestSizeLongevity = 0;
            BestFertilityImmunity = 0;

            InitializePairings(female, male);
        }

        public void InitializePairings(AnimalData female, AnimalData male)
        {
            Dictionary<int, List<Gene>> sizeOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.SIZE);
            Dictionary<int, List<Gene>> longevityOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.LONGEVITY);
            Dictionary<int, List<Gene>> fertilityOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.FERTILITY);
            Dictionary<int, List<Gene>> immunityOccurences = AnimalData.CalculateGenePairings(female, male, GeneType.IMMUNITY);

            foreach (var sPair in sizeOccurences)
                foreach (var lPair in longevityOccurences)
                    foreach (var fPair in fertilityOccurences)
                        foreach (var iPair in immunityOccurences)
                        {
                            AddOffspringData(sPair.Key, lPair.Key, fPair.Key, iPair.Key, sPair.Value.Count
                                                                                                     * lPair.Value.Count
                                                                                                     * fPair.Value.Count
                                                                                                     * iPair.Value.Count);
                        }

            CalculateProbabilities();
        }

        public void CalculateProbabilities()
        {
            AverageTotal = 0;
            AverageSize = 0;
            AverageLongevity = 0;
            AverageFertility = 0;
            AverageImmunity = 0;
            publicView = new List<OffspringData>();
            foreach (var keyValue in offspringList)
            {
                OffspringData offspringOutcome = keyValue.Value;
                offspringOutcome.probability = offspringOutcome.genePossibilities / 256f;
                publicView.Add(offspringOutcome);
                AverageTotal += offspringOutcome.TotalValue * offspringOutcome.probability;
                AverageSize += offspringOutcome.SizeValue * offspringOutcome.probability;
                AverageLongevity += offspringOutcome.LongevityValue * offspringOutcome.probability;
                AverageFertility += offspringOutcome.FertilityValue * offspringOutcome.probability;
                AverageImmunity += offspringOutcome.ImmunityValue * offspringOutcome.probability;
            }
        }

        public void AddOffspringData(int s, int l, int f, int i, Gene gene, int occurences)
        {
            string valueString = s + " " + l + " " + f + " " + i;
            AddOffspringData(s, l, f, i, valueString);
            offspringList[valueString].AddGeneOccurence(gene, occurences);
        }

        public void AddOffspringData(int s, int l, int f, int i, int occurences)
        {
            string valueString = s + " " + l + " " + f + " " + i;
            AddOffspringData(s, l, f, i, valueString);
            offspringList[valueString].genePossibilities += occurences;
        }

        private void AddOffspringData(int s, int l, int f, int i, string valueString)
        {
            if (!offspringList.ContainsKey(valueString))
            {
                offspringList.Add(valueString, new OffspringData(s, l, f, i));
            }
            if (s + l + f + i > BestSizeLongevity + BestFertilityImmunity)
            {
                BestSizeLongevity = s + l;
                BestFertilityImmunity = f + i;
                BestOffspring = offspringList[valueString];
            }
            else if (s + l + f + i == BestSizeLongevity + BestFertilityImmunity)
            {
                if (s + l > BestSizeLongevity)
                {
                    BestSizeLongevity = s + l;
                    BestFertilityImmunity = f + i;
                    BestOffspring = offspringList[valueString];
                }
            }
        }
    }

    public class OffspringData
    {
        public int SizeValue { get; private set; }
        public int LongevityValue { get; private set; }
        public int FertilityValue { get; private set; }
        public int ImmunityValue { get; private set; }

        public Dictionary<string, int> GeneOccurences;

        public int TotalValue { get; private set; }
        public int genePossibilities { get; set; }
        public float probability { get; set; }

        public OffspringData(int s, int l, int f, int i)
        {
            SizeValue = s;
            LongevityValue = l;
            FertilityValue = f;
            ImmunityValue = i;
            TotalValue = s + l + f + i;
        }

        public void AddGeneOccurence(Gene gene, int occurences)
        {
            if (GeneOccurences.ContainsKey(gene.geneString))
            {
                GeneOccurences[gene.geneString] += occurences;
                genePossibilities += occurences;
            }
            else
            {
                GeneOccurences.Add(gene.geneString, occurences);
                genePossibilities += occurences;
            }
        }
    }
}