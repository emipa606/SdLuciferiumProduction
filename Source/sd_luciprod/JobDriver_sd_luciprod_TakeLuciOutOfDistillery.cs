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
        (Building_sd_luciprod_distillery)job.GetTarget(BarrelInd).Thing;

    protected Thing sd_luciprod_rawlucibatch => job.GetTarget(sd_luciprod_rawBatchToHaulInd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Barrel, job);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(BarrelInd);
        this.FailOnBurningImmobile(BarrelInd);
        yield return Toils_Reserve.Reserve(BarrelInd);
        yield return Toils_Goto.GotoThing(BarrelInd, PathEndMode.Touch);
        yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(BarrelInd)
            .FailOn(() => !Barrel.Distilled).WithProgressBarToilDelay(BarrelInd);
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
                    job.SetTarget(StorageCellInd, c);
                    job.SetTarget(sd_luciprod_rawBatchToHaulInd, thing);
                    job.count = thing.stackCount;
                }
                else
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return Toils_Reserve.Reserve(sd_luciprod_rawBatchToHaulInd);
        yield return Toils_Reserve.Reserve(StorageCellInd);
        yield return Toils_Goto.GotoThing(sd_luciprod_rawBatchToHaulInd, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(sd_luciprod_rawBatchToHaulInd);
        var toil = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
        yield return toil;
        yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, toil, true);
    }
}