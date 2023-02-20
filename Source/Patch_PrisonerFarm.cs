using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality;
[HarmonyPatch]
public static class Patch_PrisonerFarm {

    // Add checkboxes to proisoner tab

    private static readonly FieldInfo fieldConvert = 
        AccessTools.Field(typeof(PrisonerInteractionModeDefOf), "Convert");

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ITab_Pawn_Visitor), "FillTab")]
    public static IEnumerable<CodeInstruction> Visitor_FillTab_Transpiler(IEnumerable<CodeInstruction> instrs) {
        int state = 0;
        Label? label = null;

        Func<CodeInstruction, int>[] transitions = {
            i => i.LoadsField(fieldConvert)          ?  1 : 0,
            i => i.Branches(out label)               ?  2 : 0,
            i => i.labels.Contains(label ?? default) ? -1 : 2,
        };

        foreach (var instr in instrs) {
            if (state >= 0) {
                state = transitions[state](instr);
                if (state < 0) {
                    // Load instance (inserting before instr, so move labels)
                    yield return new CodeInstruction(OpCodes.Ldloc_0)
                        .MoveLabelsFrom(instr);
                    // Get pawn
                    yield return CodeInstruction.Call(typeof(ITab), "get_SelPawn");
                    // Load the listing object
                    yield return new CodeInstruction(OpCodes.Ldloc, 4);
                    // Call our addition
                    yield return CodeInstruction.Call(typeof(Patch_PrisonerFarm), nameof(FillTab));
                }
            }
            yield return instr;
        }
    }

    public static void FillTab(Pawn pawn, Listing_Standard list) {
        if (Settings.ExpandFarming && pawn.guest.interactionMode == PrisonerInteractionModeDefOf.HemogenFarm) {
            FarmState.For(pawn).FillTab(list);
        }
    }


    // Update limiting of "awaiting operation" alert

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Alert_AwaitingMedicalOperation), "AwaitingMedicalOperation", MethodType.Getter)]
    public static List<Pawn> AwaitingMedicalOperation(List<Pawn> result) 
        => result
            .Where(x => !x.IsPrisonerOfColony)
            .Concat(PawnsFinder.AllMaps_PrisonersOfColonySpawned.Where(NeedAlert))
            .ToList();

    public static bool NeedAlert(Pawn pawn) {
        if (!HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) || !pawn.InBed()) return false;
        if (pawn.guest.interactionMode != PrisonerInteractionModeDefOf.HemogenFarm) return true;
        RecipeDef[] filter = FarmState.For(pawn).AffectedRecipes;
        return pawn.health.surgeryBills.Bills.Any(x => !filter.Contains(x.recipe));
    }


    // On interaction change

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ITab_Pawn_Visitor), "InteractionModeChanged")]
    public static bool InteractionModeChanged(ref PrisonerInteractionModeDef oldMode, PrisonerInteractionModeDef newMode) {
        if (!Settings.ExpandFarming) return true;
        if (Find.Selector.SingleSelectedThing is not Pawn pawn) return true;

        var state = FarmState.For(pawn);

        if (oldMode == PrisonerInteractionModeDefOf.HemogenFarm) {
            state.ChangeFrom();
            oldMode = PrisonerInteractionModeDefOf.NoInteraction;
        }
        if (newMode == PrisonerInteractionModeDefOf.HemogenFarm) {
            state.ChangeTo();
            return false;
        }

        return true;
    }


    // Tick

    private static bool resetInteraction = false;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.GuestTrackerTick))]
    public static void GuestTrackerTick_Pre(Pawn_GuestTracker __instance, Pawn ___pawn) {
        if (Settings.ExpandFarming 
                && ModsConfig.BiotechActive 
                && ___pawn.Spawned
                && __instance.interactionMode == PrisonerInteractionModeDefOf.HemogenFarm
                && __instance.GuestStatus == GuestStatus.Prisoner) {

            __instance.interactionMode = PrisonerInteractionModeDefOf.NoInteraction;
            resetInteraction = true;

            if (___pawn.IsHashIntervalTick(1250)) {
                FarmState.For(___pawn).Tick();
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.GuestTrackerTick))]
    public static void GuestTrackerTick_Post(Pawn_GuestTracker __instance) {
        if (resetInteraction) {
            __instance.interactionMode = PrisonerInteractionModeDefOf.HemogenFarm;
            resetInteraction = false;
        }
    }


    // Save/load

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.ExposeData))]
    public static void ExposeData(Pawn __instance) 
        => FarmState.For(__instance).ExposeData();
}
