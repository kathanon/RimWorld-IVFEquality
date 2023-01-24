using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace IVFEquality;
public class JobGiver_FertilizeOvumWithSample : WorkGiver_Scanner {
    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) 
        => pawn.Map.listerThings
            .ThingsOfDef(LocalDefOf.SpermSample)
            .Where(x => (x as SpermSample)?.OvumToFertilize != null);

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) {
        if (t is not SpermSample sample) return false;
        var ovum = sample.OvumToFertilize;
        if (ovum.DestroyedOrNull()) return false;
        if (!pawn.CanReserve(sample, ignoreOtherReservations: forced)) return false;
        if (!pawn.CanReserve(ovum,   ignoreOtherReservations: forced)) return false;
        return true;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false) {
        if (t is SpermSample sample) {
            var ovum = sample.OvumToFertilize;
            if (ovum != null) {
                var job = JobMaker.MakeJob(LocalDefOf.FertilizeOvumWithSample, ovum, sample);
                job.count = 1;
                return job;
            }
        }
        return null;
    }
}
