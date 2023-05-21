using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality;
[DefOf]
public static class LoadDefOf {
    [MayRequireBiotech]
    public static RecipeDef kathanon_IVFEquality_ExtractSample;

    [MayRequireBiotech]
    public static RecipeDef kathanon_IVFEquality_Insemination;

    [MayRequireBiotech]
    public static RecipeDef ExtractOvum;

    [MayRequireBiotech]
    public static ResearchProjectDef FertilityProcedures;

    [MayRequireBiotech]
    public static HediffDef kathanon_IVFEquality_SampleExtracted;

    [MayRequireBiotech]
    public static JobDef kathanon_IVFEquality_FertilizeOvumWithSample;

    [MayRequireBiotech]
    public static ThingDef kathanon_IVFEquality_SpermSample;

    static LoadDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(LoadDefOf));
    }
}

public static class LocalDefOf {
    public static readonly RecipeDef ExtractSample = LoadDefOf.kathanon_IVFEquality_ExtractSample;

    public static readonly RecipeDef Insemination = LoadDefOf.kathanon_IVFEquality_Insemination;

    public static readonly RecipeDef ExtractOvum = LoadDefOf.ExtractOvum;

    public static readonly ResearchProjectDef FertilityProcedures = LoadDefOf.FertilityProcedures;

    public static readonly HediffDef SampleExtracted = LoadDefOf.kathanon_IVFEquality_SampleExtracted;

    public static readonly JobDef FertilizeOvumWithSample = LoadDefOf.kathanon_IVFEquality_FertilizeOvumWithSample;

    public static readonly ThingDef SpermSample = LoadDefOf.kathanon_IVFEquality_SpermSample;
}
