using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality;
public class Recipe_ExtractSample : Recipe_AddHediff {
    public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null) {
        if (!Find.Storyteller.difficulty.ChildrenAllowed) {
            return false;
        }

        Pawn pawn;
        if ((pawn = thing as Pawn) == null) {
            return false;
        }

        if ((recipe.genderPrerequisite ?? pawn.gender) != pawn.gender) {
            return false;
        }

        if (pawn.ageTracker.AgeBiologicalYears < recipe.minAllowedAge) {
            return "CannotMustBeAge".Translate(recipe.minAllowedAge);
        }

        if (pawn.Sterile()) {
            return "CannotSterile".Translate();
        }

        if (pawn.health.hediffSet.HasHediff(LocalDefOf.SampleExtracted)) {
            return Strings.RecentlyExtracted;
        }

        return base.AvailableReport(thing, part);
    }

    public override bool CompletableEver(Pawn surgeryTarget) {
        return IsValidNow(surgeryTarget, null, ignoreBills: true);
    }

    protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill) {
        SpermSample thing = ThingMaker.MakeThing(LocalDefOf.SpermSample) as SpermSample;
        thing.TryGetComp<CompHasPawnSources>().AddSource(pawn);
        if (!GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, null, (IntVec3 x) => x.InBounds(pawn.Map) && x.Standable(pawn.Map) && !x.Fogged(pawn.Map))) {
            Log.Error("Could not drop sperm sample near " + pawn.Position);
        }
    }
}
