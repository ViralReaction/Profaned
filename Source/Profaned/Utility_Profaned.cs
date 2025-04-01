using RimWorld;
using Verse.Sound;
using Verse;

namespace Profaned
{
    public static class Utility_Profaned
    {
        public static void BestowCurse(Pawn caster, Pawn recipient, RevivalCurseExtension modExtension)
        {
            if (!ModLister.CheckBiotech("xenogerm reimplantation"))
            {
                return;
            }
            var xenoType = modExtension.xenotypeDef;
            QuestUtility.SendQuestTargetSignals(caster.questTags, "XenogermReimplanted", caster.Named("SUBJECT"));
            recipient.genes.SetXenotype(xenoType);
            recipient.genes.ClearXenogenes();

            foreach (GeneDef geneDef in xenoType.genes)
            {
                recipient.genes.AddGene(geneDef, true);
            }
            if (!caster.genes.Xenotype.soundDefOnImplant.NullOrUndefined())
            {
                caster.genes.Xenotype.soundDefOnImplant.PlayOneShot(SoundInfo.InMap(recipient, MaintenanceType.None));
            }
            var hediffOnTarget = modExtension.hediffOnTarget;
            recipient.health.AddHediff(hediffOnTarget, null, null, null);

            var secondHediffOnTarget = modExtension.secondHediffOnTarget;
            recipient.health.AddHediff(secondHediffOnTarget, null, null, null);

            recipient.needs.mood?.thoughts?.memories?.TryGainMemory(modExtension.targetThought, caster);

            var hediffOnCaster = modExtension.hediffOnCaster;
            caster.health.AddHediff(hediffOnCaster, null, null, null);

        }
        public static bool CurseXenotypCheck(Pawn pawn, Pawn caster, XenotypeDef xenotype, out Pawn_GeneTracker pawnGenes)
        {
            pawnGenes = pawn?.genes;
            if (pawnGenes == null)
            {
                return false;
            }
            if (!pawnGenes.UniqueXenotype)
            {
                return pawnGenes.Xenotype == xenotype;
            }
            return false;
        }

        public static bool CurseGeneCheck(Pawn_GeneTracker pawnGenes)
        {
            if (pawnGenes == null)
            {
                return false;
            }
            foreach (var gene in pawnGenes.GenesListForReading)
            {
                if (gene.def == DefOf_Profaned.BotchJob_Cursebearer)
                {
                    return true;
                }

            }
            return false;
        }

    }

}
