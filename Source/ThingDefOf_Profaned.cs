using RimWorld;
using Verse;

#nullable disable
namespace Profaned
{
    [DefOf]
    public static class ThingDefOf_Profaned
    {
        public static ThingDef BotchJob_UndeadColossus;

        static ThingDefOf_Profaned() => DefOfHelper.EnsureInitializedInCtor(typeof(ThingDefOf_Profaned));
    }
}
