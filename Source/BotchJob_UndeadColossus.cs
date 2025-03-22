using RimWorld;
using UnityEngine;
using Verse;

#nullable disable
namespace Profaned
{
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef BotchJob_UndeadColossus;


        static ThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}
{
    [DefOf]
    public static class PawnKindDefOf
{
    public static PawnKindDef BotchJob_UndeadColossus;


    static PawnKindDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
}
}


{
    public class IncidentWorker_BotchJob_UndeadColossus : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            return !target.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout) && (!ModsConfig.BiotechActive || !target.gameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze)) && target.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(ThingDefOf.BotchJob_UndeadColossus) && this.TryFindEntryCell(target, out IntVec3 _);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map target = (Map)parms.target;
            IntVec3 cell;
            if (!this.TryFindEntryCell(target, out cell))
                return false;
            PawnKindDef BotchJob_UndeadColossus = PawnKindDefOf.BotchJob_UndeadColossus;
            int num1 = Mathf.Clamp(GenMath.RoundRandom(StorytellerUtility.DefaultThreatPointsNow((IIncidentTarget)target) / BotchJob_UndeadColossus.combatPower), 2, Rand.RangeInclusive(3, 6));
            int num2 = Rand.RangeInclusive(90000, 150000);
            IntVec3 result = IntVec3.Invalid;
            if (!RCellFinder.TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(cell, target, 10f, out result))
                result = IntVec3.Invalid;
            Pawn newThing = (Pawn)null;
            for (int index = 0; index < num1; ++index)
            {
                IntVec3 loc = CellFinder.RandomClosewalkCellNear(cell, target, 10);
                newThing = PawnGenerator.GeneratePawn(BotchJob_UndeadColossus);
                GenSpawn.Spawn((Thing)newThing, loc, target, Rot4.Random);
                newThing.mindState.exitMapAfterTick = Find.TickManager.TicksGame + num2;
                if (result.IsValid)
                    newThing.mindState.forcedGotoPosition = CellFinder.RandomClosewalkCellNear(result, target, 10);
            }
            this.SendStandardLetter("LetterLabelThrumboPasses".Translate((NamedArgument)BotchJob_UndeadColossus.label).CapitalizeFirst(), "LetterThrumboPasses".Translate((NamedArgument)BotchJob_UndeadColossus.label), LetterDefOf.PositiveEvent, parms, (LookTargets)(Thing)newThing);
            return true;
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return RCellFinder.TryFindRandomPawnEntryCell(out cell, map, CellFinder.EdgeRoadChance_Animal + 0.2f);
        }
    }
}