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
    [MayRequireRoyalty]
    public static RecipeDef kathanon_IVFEquality_ExtractSample;

    [MayRequireRoyalty]
    public static RecipeDef kathanon_IVFEquality_Insemination;

    [MayRequireRoyalty]
    public static HediffDef kathanon_IVFEquality_SampleExtracted;

    [MayRequireRoyalty]
    public static JobDef kathanon_IVFEquality_FertilizeOvumWithSample;

    [MayRequireRoyalty]
    public static ThingDef kathanon_IVFEquality_SpermSample;

    static LoadDefOf() {
        DefOfHelper.EnsureInitializedInCtor(typeof(LoadDefOf));
    }
}

public static class LocalDefOf {
    public static readonly RecipeDef ExtractSample = LoadDefOf.kathanon_IVFEquality_ExtractSample;

    public static readonly RecipeDef Insemination = LoadDefOf.kathanon_IVFEquality_Insemination;

    public static readonly HediffDef SampleExtracted = LoadDefOf.kathanon_IVFEquality_SampleExtracted;

    public static readonly JobDef FertilizeOvumWithSample = LoadDefOf.kathanon_IVFEquality_FertilizeOvumWithSample;

    public static readonly ThingDef SpermSample = LoadDefOf.kathanon_IVFEquality_SpermSample;
}
