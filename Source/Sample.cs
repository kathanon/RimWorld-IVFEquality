using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace IVFEquality;
[StaticConstructorOnStartup]
public class SpermSample : ThingWithComps {
    private Pawn toInseminate;
    private HumanOvum ovumToFertilize;

    public HumanOvum OvumToFertilize => ovumToFertilize;

    public Pawn Donor 
        => GetComp<CompHasPawnSources>()?.pawnSources?.FirstOrFallback();

    private static readonly TargetingParameters targetOvum =
        new() {
            mapObjectTargetsMustBeAutoAttackable = false,
            canTargetPawns = false,
            canTargetItems = true,
            validator = t => t.Thing?.def == ThingDefOf.HumanOvum,
        };

    private bool IsInseminationOfThis(Bill bill) 
        => bill.recipe == LocalDefOf.Insemination 
        && bill is Bill_Medical oper 
        && oper.uniqueRequiredIngredients.Contains(this);

    public override IEnumerable<Gizmo> GetGizmos() {
        foreach (Gizmo gizmo in base.GetGizmos()) {
            yield return gizmo;
        }

        if (!ModsConfig.BiotechActive || MapHeld == null) {
            yield break;
        }

        if (toInseminate?.BillStack.Bills.Any(IsInseminationOfThis) ?? false) {
            //Log.Message($"{toInseminate.LabelShort}, {toInseminate.BillStack.Bills.Count} bills, ");
            toInseminate = null;
        }
        if (ovumToFertilize?.Destroyed ?? false) {
            ovumToFertilize = null;
        }

        /*
        if (toInseminate == null) {
            List<FloatMenuOption> targets = new List<FloatMenuOption>();
            foreach (Pawn item in MapHeld.mapPawns.FreeColonistsSpawned) {
                FloatMenuOption floatMenuOption = CanInseminateFloatOption(item);
                if (floatMenuOption != null) {
                    targets.Add(floatMenuOption);
                }
            }

            Command_Action inseminate = new Command_Action {
                defaultLabel = Strings.InseminationLabel,
                defaultDesc = Strings.InseminationDesc,
                icon = Textures.Inseminate,
                action = delegate {
                    Find.WindowStack.Add(new FloatMenu(targets));
                }
            };
            if (targets.Count == 0) {
                inseminate.Disable(Strings.NoTargets);
            }

            yield return inseminate;
        } else {
            string label = Strings.CancelInsemination(toInseminate);
            yield return new Command_Action {
                defaultLabel = label,
                defaultDesc = label,
                icon = Textures.CancelIcon,
                action = delegate {
                    toInseminate.BillStack.Bills.RemoveAll(IsInseminationOfThis);
                    toInseminate = null;
                },
                activateSound = SoundDefOf.Designate_Cancel
            };
        }
        */

        if (ovumToFertilize == null) {
            yield return new Command_TargetWithMenu {
                defaultLabel = Strings.FertilizeLabel,
                defaultDesc = Strings.FertilizeDesc,
                icon = Textures.Fertilize,
                targetingParams = targetOvum,
                map = Map,
                action = FertilizeOvum
            };
        } else {
            string label = Strings.CancelFertilization(ovumToFertilize);
            yield return new Command_Action {
                defaultLabel = label,
                defaultDesc = label,
                icon = Textures.CancelIcon,
                action = () => FertilizeOvum(null),
                activateSound = SoundDefOf.Designate_Cancel
            };
        }
    }

    public void Target(HumanOvum ovum) => ovumToFertilize = ovum;

    public void FertilizeOvum(LocalTargetInfo obj) {
        var ovum = obj.Thing as HumanOvum;
        UpdateOvum(unset: true);
        ovumToFertilize = ovum;
        UpdateOvum();
    }

    private void UpdateOvum(bool unset = false) 
        => ovumToFertilize?.GetComp<FertilizeWithComp>()?.Target(unset ? null : this);

    public override Thing SplitOff(int count) {
        return base.SplitOff(count) as SpermSample;
    }

    private FloatMenuOption CanInseminateFloatOption(Pawn mother) {
        var canFertilize = CanFertilizeReport(mother);
        if (canFertilize.Accepted) {
            Action action = AddSurgery;
            var father = Donor;
            float success = Recipe_Insemination.InseminationChance(mother, father);
            bool successReduced = success < Recipe_Insemination.BaseInseminationChance;
            float inbred = PregnancyUtility.InbredChanceFromParents(mother, father, out var relation);
            string message = null;
            if (successReduced && inbred > 0) {
                message = Strings.InseminationBothConfirm(mother, this, success, relation, inbred);
            } else if (inbred > 0f) {
                message = "FertilizeOvumInbredChance".Translate(
                    mother.Named("MOTHER"),
                    father.Named("FATHER"),
                    relation.label.Named("RELATION"),
                    inbred.ToStringPercent().Named("CHANCE"));
            } else if (successReduced) {
                message = Strings.InseminationChanceConfirm(mother, this, success);
            }
            if (message != null) {
                var localAction = action;
                action = () => Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(message, localAction, destructive: true));
            }

            return new(mother.LabelShort, action);
        }

        if (!canFertilize.Reason.NullOrEmpty()) {
            return new FloatMenuOption("DisabledOption".Translate(mother.LabelShort, canFertilize.Reason).ToString(), null);
        }

        return null;

        void AddSurgery() {
            HealthCardUtility.CreateSurgeryBill(mother, LocalDefOf.Insemination, null, new List<Thing> { this });
            toInseminate = mother;
        }
    }

    private AcceptanceReport CanFertilizeReport(Pawn pawn) {
        if (pawn.gender != Gender.Female || pawn.IsQuestLodger()) {
            return false;
        }

        if (pawn.ageTracker.AgeBiologicalYears < 16f) {
            return "CannotMustBeAge".Translate(16f).CapitalizeFirst();
        }

        if (pawn.Sterile()) {
            return "CannotSterile".Translate();
        }

        return true;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_References.Look(ref toInseminate, "toInseminate");
        Scribe_References.Look(ref ovumToFertilize, "ovumToFertilize");
        if (Scribe.mode == LoadSaveMode.PostLoadInit) {
            UpdateOvum();
        }
    }
}
