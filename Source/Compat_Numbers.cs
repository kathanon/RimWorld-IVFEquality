using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Verse;
using Verse.Sound;

namespace IVFEquality;
[HarmonyPatch]
public static class Compat_Numbers {
    private static readonly bool active = 
        LoadedModManager.RunningMods.Any(m => m.PackageId == "mehni.numbers");
    private static bool show = false;

    [HarmonyPrepare]
    public static bool Active() 
        => active;


    [HarmonyTranspiler]
    [HarmonyPatch("Numbers.PawnColumnWorker_PrisonerInteraction", "DoCell")]
    public static IEnumerable<CodeInstruction> DoCell_Transpiler(IEnumerable<CodeInstruction> orig) {
        var me = typeof(Compat_Numbers);
        var tipArgs = new Type[] { typeof(Rect), typeof(TipSignal) };
        var tipRegion = AccessTools.Method(typeof(TooltipHandler), nameof(TooltipHandler.TipRegion), tipArgs);

        yield return new(OpCodes.Ldarg_3);
        yield return CodeInstruction.Call(me, nameof(SetShow));

        int state = 0, skip = 0;
        foreach (var instr in orig) {
            yield return instr;

            if (state == 0) {
                if (instr.Calls(tipRegion)) {
                    skip = 4;
                    state = 1;
                }
            } else if (skip > 0) {
                skip--;
                if (skip == 0) {
                    yield return new(OpCodes.Ldloc_2);
                    yield return new(OpCodes.Ldarg_2);
                    yield return new(OpCodes.Ldloca_S, 0);
                    yield return CodeInstruction.Call(me, nameof(AddExtraFor));
                }
            }
        }
    }

    public static void AddExtraFor(PrisonerInteractionModeDef mode, Pawn pawn, ref float x) {
        if (show && Farm(mode)) {
            bool disabled = !Farm(pawn.guest.interactionMode);
            FarmState.For(pawn).DoNumbersChecks(ref x, disabled);
        }
    }

    public static void SetShow(PawnTable table) {
        bool val = table.PawnsListForReading.Any(p => Farm(p.guest.interactionMode));
        if (val != show) {
            table.SetDirty();
        }
        show = val;
    }

    private static bool Farm(PrisonerInteractionModeDef mode)
        => mode == PrisonerInteractionModeDefOf.HemogenFarm;


    [HarmonyPostfix]
    [HarmonyPatch("Numbers.PawnColumnWorker_PrisonerInteraction", "GetMinWidth")]
    public static void GetMinWidth_Post(ref int __result) {
        if (show) {
            __result += FarmState.NumbersWidth;
        }
    }
}
