using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace IVFEquality;
public class Settings : ModSettings {
    private static readonly PrisonerInteractionModeDef farmDef = 
        PrisonerInteractionModeDefOf.HemogenFarm;
    private static readonly string farmVanillaLabel = farmDef.label;
    private static readonly string farmVanillaDesc  = farmDef.description;
    private static readonly string[] defaultsLabels = { Strings.Hemogen, Strings.Genes, Strings.OvumOrSperm };

    private static Settings instance;
    
    private bool farming = false;
    private readonly bool[] defaults = { true, false, false };

    public Settings() {
        instance = this;
    }

    public static bool ExpandFarming => instance.farming;

    public static (bool, bool, bool) ExpandedFarmingDefaults 
        => (instance.defaults[0], instance.defaults[1], instance.defaults[2]);

    public void DoGUI(Rect inRect) {
        bool pre;

        var row = inRect.TopPartPixels(Widgets.CheckboxSize);
        var label = Strings.CheckLabel;
        float width = Text.CalcSize(label).x + Widgets.CheckboxSize + 10f;
        pre = farming;
        Widgets.CheckboxLabeled(row.LeftPartPixels(width), label, ref farming);
        if (pre != farming) Update();

        row.y += row.height + 4f;
        Widgets.Label(row, Strings.DefaultLabel);

        row.y += row.height;
        width = defaultsLabels.Max(str => Text.CalcSize(str).x) + Widgets.CheckboxSize + 10f;
        var check = row.LeftPartPixels(width);
        check.x += 40f;
        for (int i = 0; i < defaultsLabels.Length; i++) {
            Widgets.CheckboxLabeled(check, defaultsLabels[i], ref defaults[i]);
            check.y += check.height + 4f;
        }
    }

    public void Update() {
        farmDef.label = farming ? Strings.FarmLabel : farmVanillaLabel;
        farmDef.description = farming ? Strings.FarmDesc : farmVanillaDesc;
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref farming, "expandFarming", false);
        Scribe_Values.Look(ref defaults[0], "defaultsHemogen", true);
        Scribe_Values.Look(ref defaults[1], "defaultsGenes", false);
        Scribe_Values.Look(ref defaults[2], "defaultsOvum", false);
    }
}
