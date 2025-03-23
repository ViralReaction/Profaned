using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;

#nullable disable
namespace Profaned
{
    [StaticConstructorOnStartup]
    public class ProfanedHatcherPatch
    {
        static ProfanedHatcherPatch()
        {
            new Harmony("rimworld.profaned.hatcherpatch").PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(CompHatcher))]
        [HarmonyPatch("Hatch")]
        private class Hatch
        {
            public static void Prefix(CompHatcher __instance)
            {
                HatcherExtension modExtension = __instance.parent.def.GetModExtension<HatcherExtension>();
                if (modExtension == null || !modExtension.forcePlayerFaction)
                    return;
                __instance.hatcheeFaction = Faction.OfPlayer;
            }
        }
    }
}

