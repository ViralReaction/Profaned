using RimWorld;
using Verse;

#nullable disable
namespace Profaned
{
    [DefOf]
    public static class DefOf_Profaned
    {
        public static ThingDef BotchJob_UndeadColossus;

        static DefOf_Profaned() => DefOfHelper.EnsureInitializedInCtor(typeof(DefOf_Profaned));
    }
}
