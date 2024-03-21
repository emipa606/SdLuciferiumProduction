using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace sd_luciprod;

public class JobDriver_sd_luciprod_FillDistillery : JobDriver
{
    private const TargetIndex BarrelInd = TargetIndex.A;

    private const TargetIndex WortInd = TargetIndex.B;

    private const int Duration = 200;

    protected Building_sd_luciprod_distillery Barrel =>
        (Building_sd_luciprod_distillery)job.GetTarget(BarrelInd).Thing;

    protected Thing sd_luciprod_mechanite_oil => job.GetTarget(WortInd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Barrel, job) && pawn.Reserve(sd_luciprod_mechanite_oil, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(BarrelInd);
        this.FailOnBurningImmobile(BarrelInd);
        yield return Toils_Reserve.Reserve(BarrelInd);
        var toil = Toils_Reserve.Reserve(WortInd);
        yield return toil;
        yield return Toils_Goto.GotoThing(WortInd, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(WortInd).FailOnSomeonePhysicallyInteracting(WortInd);
        yield return Toils_Haul.StartCarryThing(WortInd).FailOnDestroyedNullOrForbidden(WortInd);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(toil, WortInd, TargetIndex.None);
        yield return Toils_Goto.GotoThing(BarrelInd, PathEndMode.Touch);
        yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(WortInd)
            .FailOnDestroyedNullOrForbidden(BarrelInd).WithProgressBarToilDelay(BarrelInd);
        yield return new Toil
        {
            initAction = delegate { Barrel.Addoil(sd_luciprod_mechanite_oil); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}