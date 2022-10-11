using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod;

public class JobDriver_sd_luciprod_TakeLuciOutOfDistillery : JobDriver
{
    private const TargetIndex BarrelInd = TargetIndex.A;

    private const TargetIndex sd_luciprod_rawBatchToHaulInd = TargetIndex.B;

    private const TargetIndex StorageCellInd = TargetIndex.C;

    private const int Duration = 200;

    protected Building_sd_luciprod_distillery Barrel =>
        (Building_sd_luciprod_distillery)job.GetTarget(TargetIndex.A).Thing;

    protected Thing sd_luciprod_rawlucibatch => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Barrel, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        yield return Toils_Reserve.Reserve(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.A)
            .FailOn(() => !Barrel.Distilled).WithProgressBarToilDelay(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate
            {
                var thing = Barrel.TakeOutrawlucibatch();
                GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near);
                var currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, Map, currentPriority, pawn.Faction,
                        out var c))
                {
                    job.SetTarget(TargetIndex.C, c);
                    job.SetTarget(TargetIndex.B, thing);
                    job.count = thing.stackCount;
                }
                else
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return Toils_Reserve.Reserve(TargetIndex.B);
        yield return Toils_Reserve.Reserve(TargetIndex.C);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B);
        var toil = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
        yield return toil;
        yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, toil, true);
    }
}