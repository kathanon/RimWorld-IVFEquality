using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IVFEquality;
public class FertilizeWithComp : ThingComp {
    public SpermSample sampleToUse;

    private static readonly TargetingParameters targetSample =
        new() {
            mapObjectTargetsMustBeAutoAttackable = false,
            canTargetPawns = false,
            canTargetItems = true,
            validator = t => t.Thing?.def == LocalDefOf.SpermSample,
        };

    public override IEnumerable<Gizmo> CompGetGizmosExtra() {
        if (sampleToUse != null && (sampleToUse.Destroyed || sampleToUse.OvumToFertilize != parent)) {
            sampleToUse = null;
        }

        if (sampleToUse == null) {
            yield return new Command_TargetWithMenu {
                defaultLabel = Strings.FertilizeWithLabel,
                defaultDesc = Strings.FertilizeWithDesc,
                icon = Textures.Fertilize,
                targetingParams = targetSample,
                map = parent.Map,
                action = FertilizeWith
            };
        } else {
            string label = Strings.CancelFertilizationWith(sampleToUse);
            yield return new Command_Action {
                defaultLabel = label,
                defaultDesc = label,
                icon = Textures.CancelIcon,
                action = () => FertilizeWith(null),
                activateSound = SoundDefOf.Designate_Cancel
            };
        }
    }

    public void Target(SpermSample sample) => sampleToUse = sample;

    private void FertilizeWith(LocalTargetInfo obj) {
        var sample = obj.Thing as SpermSample;
        sampleToUse?.Target(null);
        sampleToUse = sample;
        sample?.Target(parent as HumanOvum);
    }
}
