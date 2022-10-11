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
        (Building_sd_luciprod_distillery)job.GetTarget(TargetIndex.A).Thing;

    protected Thing sd_luciprod_mechanite_oil => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Barrel, job) && pawn.Reserve(sd_luciprod_mechanite_oil, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        var toil = Toils_Reserve.Reserve(TargetIndex.B);
        yield return toil;
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(toil, TargetIndex.B, TargetIndex.None);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate { Barrel.Addoil(sd_luciprod_mechanite_oil); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}