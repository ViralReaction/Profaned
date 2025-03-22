using RimWorld;
using Verse;

#nullable disable
namespace Profaned
{
    [DefOf]
    public static class PawnKindDefOf
    {
        public static PawnKindDef BotchJob_UndeadColossus;
        public static PawnKindDef BotchJob_Ghoul;
        public static PawnKindDef BotchJob_Skeleton;
        public static PawnKindDef BotchJob_Wraith;
        public static PawnKindDef BotchJob_UndeadDragon;



        static PawnKindDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(PawnKindDefOf));
    }
}

