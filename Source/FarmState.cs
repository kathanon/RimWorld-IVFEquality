using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IVFEquality;
public class FarmState {
    private static readonly ConditionalWeakTable<Pawn, FarmState> table = new();

    private const float NumbersSize    = 30f;
    private const float NumbersCheck   = 16f;
    private const float NumbersStep    = 20f;
    private const float NumbersXMargin = -2f;
    private const float NumbersYMargin = (NumbersSize - NumbersCheck) / 2;
    public  const int   NumbersWidth   = 3 * (int) NumbersStep;

    private static readonly RecipeDef[][] recipeDefs = {
        new RecipeDef[] {},
        new RecipeDef[] { RecipeDefOf.ExtractHemogenPack },
        new RecipeDef[] { LocalDefOf.ExtractOvum, LocalDefOf.ExtractSample },
        new RecipeDef[] { RecipeDefOf.ExtractHemogenPack, LocalDefOf.ExtractOvum, LocalDefOf.ExtractSample },
    };

    private readonly Pawn pawn;
    private bool hemogen;
    private bool genes;
    private bool ovumOrSperm;

    public RecipeDef[] AffectedRecipes 
        => recipeDefs[Settings.ExpandFarming ? ((hemogen ? 1 : 0) + (ovumOrSperm ? 2 : 0)) : 1];

    public static FarmState For(Pawn pawn) 
        => table.GetValue(pawn, p => new(p));

    private FarmState(Pawn p) {
        pawn = p;
        (hemogen, genes, ovumOrSperm) = Settings.ExpandedFarmingDefaults;
    }

    public void FillTab(Listing_Standard list) {
        string ovumSperm = pawn.gender == Gender.Female ? Strings.Ovum : Strings.Sperm;
        list.Gap(4f);
        list.Label(Strings.WhatToFarm);
        Rect temp = default;
        DoCheck(ref temp, list, true,  Strings.Hemogen, ref hemogen,     UpdateHemogen);
        DoCheck(ref temp, list, false, Strings.Genes,   ref genes,       UpdateGenes);
        DoCheck(ref temp, list, true,  ovumSperm,       ref ovumOrSperm, UpdateOvumOrSperm);
        list.Gap(4f);
    }

    private static void DoCheck(ref Rect rect, Listing_Standard list, bool left, string label, ref bool var, Action<bool> onChange) {
        bool pre = var;
        Rect part;
        if (left) {
            rect = list.GetRect(Widgets.CheckboxSize);
            part = rect.LeftHalf();
        } else {
            part = rect.RightHalf();
        }
        part.xMin += 10f;
        Widgets.CheckboxLabeled(part, label, ref var);
        if (pre != var) onChange(var);
    }

    public void DoNumbersChecks(ref float x, bool disabled) {
        string ovumSperm = pawn.gender == Gender.Female ? Strings.Ovum : Strings.Sperm;
        DoNumbersCheck(ref x, disabled, ref hemogen,     Strings.Hemogen, UpdateHemogen);
        DoNumbersCheck(ref x, disabled, ref genes,       Strings.Genes,   UpdateGenes);
        DoNumbersCheck(ref x, disabled, ref ovumOrSperm, ovumSperm,       UpdateOvumOrSperm);
    }

    private void DoNumbersCheck(ref float x, bool disabled, ref bool var, string tip, Action<bool> onChange) {
        bool pre = var;
        var area = new Rect(x, 0f, NumbersStep, NumbersSize);
        Widgets.Checkbox(new(x + NumbersXMargin, NumbersYMargin), ref var, NumbersCheck, disabled, paintable: true);
        TooltipHandler.TipRegion(area, tip);
        x += NumbersStep;
        if (pre != var) onChange(var);
    }

    private RecipeDef OvumOrSpermRecipe 
        => pawn.gender == Gender.Female ? LocalDefOf.ExtractOvum : LocalDefOf.ExtractSample;

    private void UpdateRecipe(RecipeDef def, bool add, Func<bool> check = null, ResearchProjectDef research = null) {
        if (!(research?.IsFinished ?? true)) return;
        check ??= () => true;
        var stack = pawn.BillStack.Bills;
        var bill = stack.FirstOrDefault(x => x.recipe == def);
        if (add ^ bill == null) return;
        if (!add) {
            stack.Remove(bill);
        } else if (def.Worker.AvailableOnNow(pawn) && check()) {
            HealthCardUtility.CreateSurgeryBill(pawn, def, null, sendMessages: false);
        }
    }

    private void Update(bool add) {
        if (hemogen)     UpdateHemogen(add);
        if (genes)       UpdateGenes(add);
        if (ovumOrSperm) UpdateOvumOrSperm(add);
    }

    private void UpdateHemogen(bool add) 
        => UpdateRecipe(RecipeDefOf.ExtractHemogenPack, add, HasNoBleed);

    private void UpdateOvumOrSperm(bool add) 
        => UpdateRecipe(OvumOrSpermRecipe, add, research: LocalDefOf.FertilityProcedures);

    private void UpdateGenes(bool add) {
        var extractors = pawn.Map.listerThings
            .ThingsOfDef(ThingDefOf.GeneExtractor)
            .OfType<Building_GeneExtractor>();
        var current = extractors
            .Where(x => x.SelectedPawn == pawn)
            .FirstOrDefault();
        if (add ^ current == null) return;
        if (add) {
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.XenogermReplicating)) return;
            var pos = pawn.Position;
            var toUse = extractors
                .Where(x => x.CanAcceptPawn(pawn))
                .OrderBy(x => x.Position.DistanceToSquared(pos))
                .FirstOrDefault();
            if (toUse == null) return;
            toUse.SelectedPawn = pawn;
        } else {
            current.SelectedPawn = null;
        }
    }

    private bool HasNoBleed() 
        => !pawn.health.hediffSet.HasHediff(HediffDefOf.BloodLoss);

    public void ChangeFrom() 
        => Update(false);

    public void ChangeTo() 
        => Update(true);

    public void Tick() 
        => Update(true);

    public void ExposeData() {
        if (Scribe.EnterNode(Strings.ID)) {
            try {
                var defs = Settings.ExpandedFarmingDefaults;
                Scribe_Values.Look(ref hemogen,     "hemogen",     defs.Item1);
                Scribe_Values.Look(ref genes,       "genes",       defs.Item2);
                Scribe_Values.Look(ref ovumOrSperm, "ovumOrSperm", defs.Item3);
            } finally {
                Scribe.ExitNode();
            }
        }
    }
}
