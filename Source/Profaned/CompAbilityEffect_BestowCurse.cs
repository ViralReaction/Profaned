using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace Profaned
{
    public class CompAbilityEffect_BestowCurse : CompAbilityEffect
    {
        private new CompProperties_AbilityBestowCurse Props
        {
            get
            {
                return (CompProperties_AbilityBestowCurse)this.props;
            }
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!ModLister.CheckBiotech("xenogerm reimplantation"))
            {
                return;
            }
            base.Apply(target, dest);
            Pawn pawn = null;
            if (target.HasThing && target.Thing is Corpse corpse)
            {
                pawn = corpse.InnerPawn;
            }

            if (pawn == null)
            {
                Log.Error($"Profaned: {this.parent.def} Apply called on invalid target");
                return;
            }

            if (!this.parent.pawn.IsColonistPlayerControlled)
            {
                return;
            }
            var modExtension = this.parent.def.GetModExtension<RevivalCurseExtension>();
            if (modExtension == null)
            {
                Log.Error($"Profaned: {this.parent.def} is missing required mod extension");
                return;
            }

            if (target.Thing is Corpse)
            {
                ResurrectionUtility.TryResurrect(pawn);
            }
          
            Utility_Profaned.BestowCurse(this.parent.pawn, pawn, modExtension);

            FleckMaker.AttachedOverlay(pawn, FleckDefOf.FlashHollow, new Vector3(0f, 0f, 0.26f), 1f, -1f);

            if (PawnUtility.ShouldSendNotificationAbout(this.parent.pawn) || PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                int hediffOnTargetTickMax = modExtension.hediffOnTarget.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.max;
                int secondHediffOnTargetTickMax = modExtension.secondHediffOnTarget.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.max - hediffOnTargetTickMax;
                int hediffOnCasterTickMax = modExtension.hediffOnCaster.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.max;

                Find.LetterStack.ReceiveLetter(
                    "LetterLabelBestowedCurse".Translate(),
                    "LetterTextBestowedCurse".Translate(
                        this.parent.pawn.Named("CASTER"),
                        pawn.Named("TARGET"),
                        hediffOnTargetTickMax.ToStringTicksToPeriod(true, false, true, true, false).Named("CURSECOMADURATION"),
                        secondHediffOnTargetTickMax.ToStringTicksToPeriod(true, false, true, true, false).Named("CURSEWEAKNESSDURATION"),
                        hediffOnCasterTickMax.ToStringTicksToPeriod(true, false, true, true, false).Named("CURSESHOCKDURATION")
                    ),
                    LetterDefOf.NeutralEvent,
                    new LookTargets(new[] { this.parent.pawn, pawn })
                );
            }
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = null;
            if (target.HasThing && target.Thing is Corpse corpse)
            {
                pawn = corpse.InnerPawn;
            }
            if (pawn == null)
            {
                return base.Valid(target, throwMessages);
            } 
            var modExtension = this.parent.def.GetModExtension<RevivalCurseExtension>();
            if (modExtension == null)
            {
                Log.Error($"Profaned: {this.parent.def} is missing required mod extension");
                return false;
            }
            if (Utility_Profaned.SameXenotype(pawn, this.parent.pawn, modExtension.xenotypeDef))
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCannotUseOnUndead".Translate(pawn), pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            return base.Valid(target, throwMessages);
        }

        public override IEnumerable<Mote> CustomWarmupMotes(LocalTargetInfo target)
        {
            if (target.HasThing && target.Thing is Corpse corpse)
            {
                yield return MoteMaker.MakeAttachedOverlay(corpse, ThingDefOf.Mote_XenogermImplantation, new Vector3(0f, 0f, 0.3f), 1f, -1f);
            }
            yield break;
        }


        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (GeneUtility.CanAbsorbXenogerm(this.parent.pawn))
            {
                Pawn myPawn = this.parent.pawn;
                Command_Action command_Action = new()
                {
                    defaultLabel = "CommandBestowCurse".Translate(),
                    defaultDesc = "CommandBestowCurseDesc".Translate(),
                    icon = CompAbilityEffect_BestowCurse.ReimplantIcon.Texture,
                    action = delegate ()
                    {
                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        List<Pawn> potentialRecipients = myPawn.MapHeld.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);

                        for (int i = 0; i < potentialRecipients.Count; i++)
                        {
                            Pawn absorber = potentialRecipients[i];

                            if (absorber.genes != null &&
                                absorber.IsColonistPlayerControlled &&
                                !GeneUtility.SameXenotype(absorber, myPawn) &&
                                absorber.CanReach(myPawn, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
                            {
                                Pawn target = absorber;
                                list.Add(new FloatMenuOption(
                                    target.LabelShort,
                                    delegate ()
                                    {
                                        void PerformReimplant()
                                        {
                                            GeneUtility.GiveReimplantJob(target, myPawn);
                                        }

                                        if (GeneUtility.PawnWouldDieFromReimplanting(myPawn))
                                        {
                                            TaggedString warning = "WarningPawnWillDieFromReimplanting".Translate(myPawn.Named("PAWN"));
                                            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(warning, PerformReimplant, true));
                                        }
                                        else
                                        {
                                            PerformReimplant();
                                        }
                                    },
                                    target,
                                    Color.white,
                                    MenuOptionPriority.Default,
                                    null,
                                    null,
                                    0f,
                                    null,
                                    null,
                                    true,
                                    0
                                ));
                            }
                        }


                        if (list.Any())
                        {
                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                    }
                };
                yield return command_Action;
            }
            yield break;
        }

        private static readonly CachedTexture ReimplantIcon = new CachedTexture("UI/Icons/Genes/Gene_XenogermReimplanter");
    }
}