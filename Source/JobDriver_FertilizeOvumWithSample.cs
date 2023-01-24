using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace IVFEquality;
public class JobDriver_FertilizeOvumWithSample : JobDriver_FertilizeOvum {

    public const TargetIndex SampleIndex = TargetIndex.B;

    private HumanOvum Ovum => (HumanOvum) TargetThingA;

    private SpermSample Sample => (SpermSample) TargetThingB;

    public override bool TryMakePreToilReservations(bool errorOnFailed) {
        if (!base.TryMakePreToilReservations(errorOnFailed)) {
            return false;
        }

        if (!pawn.Reserve(job.targetB, job, errorOnFailed: errorOnFailed)) {
            return false;
        }

        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils() {
        if (!ModsConfig.BiotechActive) {
            yield break;
        }

        //AddFailCondition(() => Sample.OvumToFertilize != Ovum);

        yield return Toils_Goto.GotoThing(SampleIndex, PathEndMode.ClosestTouch)
            .FailOnDespawnedOrNull(SampleIndex)
            .FailOnSomeonePhysicallyInteracting(SampleIndex);
        yield return Toils_Haul.StartCarryThing(SampleIndex,
                                                putRemainderInQueue: false,
                                                subtractNumTakenFromJobCount: true,
                                                failIfStackCountLessThanJobCount: true);
        yield return Toils_General.DoAtomic(delegate {
            if (Sample != pawn.carryTracker.CarriedThing) {
                Sample.FertilizeOvum(null);
            }
        });
        yield return Toils_Goto.GotoThing(OvumIndex, PathEndMode.ClosestTouch)
            .FailOnDespawnedOrNull(OvumIndex)
            .FailOnSomeonePhysicallyInteracting(OvumIndex);
        Toil toil = Toils_General.Wait(180)
            .WithProgressBarToilDelay(OvumIndex);
        toil.tickAction = delegate {
            if (pawn.IsHashIntervalTick(100)) {
                FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.HealingCross);
            }
        };
        yield return toil;
        yield return Toils_General.DoAtomic(delegate {
            var sample = pawn.carryTracker.CarriedThing as SpermSample;
            Pawn father = sample?.Donor;
            Thing embryo = (father != null) ? Ovum.ProduceEmbryo(father) : null;
            if (embryo != null) {
                GenPlace.TryPlaceThing(embryo, pawn.PositionHeld, pawn.Map, ThingPlaceMode.Near);
                pawn.carryTracker.DestroyCarriedThing();
                Ovum.Destroy();
            } else if (PawnUtility.ShouldSendNotificationAbout(pawn)) {
                Messages.Message("MessageFerilizeFailed".Translate(pawn.Named("PAWN")) + ": " 
                                 + "CombinedGenesExceedMetabolismLimits".Translate(), 
                    pawn, 
                    MessageTypeDefOf.NegativeEvent);
            }
        });
    }
}
