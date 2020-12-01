using System.Collections.Generic;
using System.Linq;

namespace PlanetZooGeneHelper
{
    public enum GeneType
    {
        SIZE,
        LONGEVITY,
        UNKNOWN_1,
        FERTILITY,
        IMMUNITY,
        UNKNOWN_2
    }

    public class AnimalData
    {
        public const int GENE_LENGTH = 12;

        public uint AnimalId { get; set; }
        public ushort SpeciesId { get; set; }
        public string Species { get; set; }
        public ushort NameId { get; set; }
        public string Name { get; set; }

        public byte Sex { get; set; }

        public string SizeGeneString { get { return SizeGene.geneString; } }
        public string LongevityGeneString { get { return LongevityGene.geneString; } }
        public string UnknownGeneString_1 { get { return UnknownGene_1.geneString; } }
        public string FertilityGeneString { get { return FertilityGene.geneString; } }
        public string ImmunityGeneString { get { return ImmunityGene.geneString; } }
        public string UnknownGeneString_2 { get { return UnknownGene_2.geneString; } }
        public string SexString { get { return Sex == 0x00 ? "M" : "F"; } }

        private Gene SizeGene;
        private Gene LongevityGene;
        private Gene UnknownGene_1;
        private Gene FertilityGene;
        private Gene ImmunityGene;
        private Gene UnknownGene_2;
        

        public float SizeValue { get; private set; }
        public float LongevityValue { get; private set; }
        public float FertilityValue { get; private set; }
        public float ImmunityValue { get; private set; }

        public AnimalData()
        {
            AnimalId = 0;
            SpeciesId = 0;
            NameId = 0;
            Name = "";
            SizeGene = new Gene();
            LongevityGene = new Gene();
            UnknownGene_1 = new Gene();
            FertilityGene = new Gene();
            ImmunityGene = new Gene();
            UnknownGene_2 = new Gene();
        }

        public AnimalData(uint animalId, ushort speciesId) : this()
        {
            AnimalId = animalId;
            SpeciesId = speciesId;
        }

        public void SetGene(IEnumerable<byte> geneArr)
        {
            SetGene(geneArr.Take(GENE_LENGTH).ToArray(), GeneType.SIZE);
            SetGene(geneArr.Skip(GENE_LENGTH).Take(GENE_LENGTH).ToArray(), GeneType.LONGEVITY);
            SetGene(geneArr.Skip(GENE_LENGTH * 2).Take(GENE_LENGTH).ToArray(), GeneType.UNKNOWN_1);
            SetGene(geneArr.Skip(GENE_LENGTH * 3).Take(GENE_LENGTH).ToArray(), GeneType.FERTILITY);
            SetGene(geneArr.Skip(GENE_LENGTH * 4).Take(GENE_LENGTH).ToArray(), GeneType.IMMUNITY);

            SizeValue = SizeGene.CountAandB() / 12f;
            LongevityValue = LongevityGene.CountAandB() / 12f;
            FertilityValue = 1f - FertilityGene.FindSimilarGenes() / 6f;
            ImmunityValue = 1f - ImmunityGene.FindSimilarGenes() / 6f;
        }

        public bool SetGene(byte[] geneArr, GeneType geneId)
        {
            if (geneArr.Length != 12)
                return false;
            switch (geneId)
            {
                case GeneType.SIZE:
                    SizeGene.Set(geneArr);
                    break;

                case GeneType.LONGEVITY:
                    LongevityGene.Set(geneArr);
                    break;

                case GeneType.UNKNOWN_1:
                    UnknownGene_1.Set(geneArr);
                    break;

                case GeneType.FERTILITY:
                    FertilityGene.Set(geneArr);
                    break;

                case GeneType.IMMUNITY:
                    ImmunityGene.Set(geneArr);
                    break;

                case GeneType.UNKNOWN_2:
                    UnknownGene_2.Set(geneArr);
                    break;
            }

            return true;
        }
    }
}