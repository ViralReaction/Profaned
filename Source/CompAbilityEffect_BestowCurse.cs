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

            // Support both pawn and corpse targeting. Not sure what they want currently
            Pawn pawn = null;
            if (target.HasThing)
            {
                if (target.Thing is Pawn livePawn)
                {
                    pawn = livePawn;
                }
                else if (target.Thing is Corpse corpse)
                {
                    pawn = corpse.InnerPawn;
                }
            }

            if (pawn == null)
            {
                Log.Warning("[Xeno] Apply called on invalid target (no pawn or corpse)");
                return;
            }

            if (!this.parent.pawn.IsColonistPlayerControlled)
            {
                return;
            }

            if (target.Thing is Corpse)
            {
                ResurrectionUtility.TryResurrect(pawn);
            }

            GeneUtility.ReimplantXenogerm(this.parent.pawn, pawn);

            FleckMaker.AttachedOverlay(pawn, FleckDefOf.FlashHollow, new Vector3(0f, 0f, 0.26f), 1f, -1f);

            if (PawnUtility.ShouldSendNotificationAbout(this.parent.pawn) || PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                int max = HediffDefOf.XenogerminationComa.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.max;
                int max2 = HediffDefOf.XenogermLossShock.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.max;

                Find.LetterStack.ReceiveLetter(
                    "LetterLabelGenesImplanted".Translate(),
                    "LetterTextGenesImplanted".Translate(
                        this.parent.pawn.Named("CASTER"),
                        pawn.Named("TARGET"),
                        max.ToStringTicksToPeriod(true, false, true, true, false).Named("COMADURATION"),
                        max2.ToStringTicksToPeriod(true, false, true, true, false).Named("SHOCKDURATION")
                    ),
                    LetterDefOf.NeutralEvent,
                    new LookTargets(new[] { this.parent.pawn, pawn })
                );
            }
        }


        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;
            if (pawn == null)
            {
                return base.Valid(target, throwMessages);
            }
            if (pawn.IsQuestLodger())
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCannotImplantInTempFactionMembers".Translate(), pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            if (pawn.HostileTo(this.parent.pawn) && !pawn.Downed)
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCantUseOnResistingPerson".Translate(this.parent.def.Named("ABILITY")), pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            if (!this.parent.pawn.genes.Xenogenes.Any<Gene>())
            {
                if (throwMessages)
                {
                    Messages.Message("MessagePawnHasNoXenogenes".Translate(this.parent.pawn), this.parent.pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            if (GeneUtility.SameXenotype(pawn, this.parent.pawn))
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCannotUseOnSameXenotype".Translate(pawn), pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            if (!CompAbilityEffect_BestowCurse.PawnIdeoCanAcceptReimplant(this.parent.pawn, pawn))
            {
                if (throwMessages)
                {
                    Messages.Message("MessageCannotBecomeNonPreferredXenotype".Translate(pawn), pawn, MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            return base.Valid(target, throwMessages);
        }

        public override Window ConfirmationDialog(LocalTargetInfo target, Action confirmAction)
        {
            if (GeneUtility.PawnWouldDieFromReimplanting(this.parent.pawn))
            {
                return Dialog_MessageBox.CreateConfirmation("WarningPawnWillDieFromReimplanting".Translate(this.parent.pawn.Named("PAWN")), confirmAction, true, null, WindowLayer.Dialog);
            }
            return null;
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
                    defaultLabel = "ForceXenogermImplantation".Translate(),
                    defaultDesc = "ForceXenogermImplantationDesc".Translate(),
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
                                if (!CompAbilityEffect_ReimplantXenogerm.PawnIdeoCanAcceptReimplant(myPawn, absorber))
                                {
                                    list.Add(new FloatMenuOption(
                                        absorber.LabelCap + ": " + "IdeoligionForbids".Translate(),
                                        null,
                                        absorber,
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
                                else
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
                        }

                        if (list.Any())
                        {
                            Find.WindowStack.Add(new FloatMenu(list));
                        }
                    }
                };

                if (myPawn.IsQuestLodger())
                {
                    command_Action.Disable("TemporaryFactionMember".Translate(myPawn.Named("PAWN")));
                }
                else if (myPawn.health.hediffSet.HasHediff(HediffDefOf.XenogermLossShock, false))
                {
                    command_Action.Disable("XenogermLossShockPresent".Translate(myPawn.Named("PAWN")));
                }
                else if (myPawn.IsPrisonerOfColony && !myPawn.Downed)
                {
                    command_Action.Disable("MessageTargetMustBeDownedToForceReimplant".Translate(myPawn.Named("PAWN")));
                }

                yield return command_Action;
            }
            yield break;
        }


        public static bool PawnIdeoCanAcceptReimplant(Pawn implanter, Pawn implantee)
        {
            if (!ModsConfig.IdeologyActive)
            {
                return true;
            }
            if (!IdeoUtility.DoerWillingToDo(HistoryEventDefOf.PropagateBloodfeederGene, implantee))
            {
                if (implanter.genes.Xenogenes.Any((Gene x) => x.def == GeneDefOf.Bloodfeeder))
                {
                    return false;
                }
            }
            return IdeoUtility.DoerWillingToDo(HistoryEventDefOf.BecomeNonPreferredXenotype, implantee) || implantee.Ideo.IsPreferredXenotype(implanter);
        }

        private static readonly CachedTexture ReimplantIcon = new CachedTexture("UI/Icons/Genes/Gene_XenogermReimplanter");
    }
}