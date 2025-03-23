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
        public static bool SameXenotype(Pawn pawn, Pawn caster, XenotypeDef xenotype)
        {
            var pawnGenes = pawn?.genes;
            var casterGenes = caster?.genes;
            if (pawnGenes == null || casterGenes == null)
            {
                return false;
            }
            if (!pawnGenes.UniqueXenotype)
            {
                return pawnGenes.Xenotype == xenotype || pawnGenes.Xenotype == casterGenes.Xenotype;
            }
            if (casterGenes.UniqueXenotype)
            {
                var pawnCustomXenotype = pawnGenes.CustomXenotype;
                var casterCustomXenotype = casterGenes.CustomXenotype;

                if (pawnCustomXenotype != null && casterCustomXenotype != null)
                {
                    return pawnCustomXenotype.name == casterCustomXenotype.name;
                }
                return false;
            }
            return false;
        }

    }

}
