using RimWorld;
using Verse;

#nullable disable
namespace Profaned
{
    [DefOf]
    public static class ThingDefOf
    {
        public static ThingDef BotchJob_UndeadColossus;
        public static PawnKindDef BotchJob_Ghoul;
        public static PawnKindDef BotchJob_Skeleton;
        public static PawnKindDef BotchJob_Wraith;
        public static PawnKindDef BotchJob_UndeadDragon;


        static ThingDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf));
    }
}
