using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod
{
    // Token: 0x02000006 RID: 6
    public class JobDriver_sd_luciprod_TakeLuciOutOfDistillery : JobDriver
    {
        // Token: 0x04000010 RID: 16
        private const TargetIndex BarrelInd = TargetIndex.A;

        // Token: 0x04000011 RID: 17
        private const TargetIndex sd_luciprod_rawBatchToHaulInd = TargetIndex.B;

        // Token: 0x04000012 RID: 18
        private const TargetIndex StorageCellInd = TargetIndex.C;

        // Token: 0x04000013 RID: 19
        private const int Duration = 200;

        // Token: 0x17000010 RID: 16
        // (get) Token: 0x0600002B RID: 43 RVA: 0x00003128 File Offset: 0x00002128
        protected Building_sd_luciprod_distillery Barrel =>
            (Building_sd_luciprod_distillery) job.GetTarget(TargetIndex.A).Thing;

        // Token: 0x17000011 RID: 17
        // (get) Token: 0x0600002C RID: 44 RVA: 0x00003154 File Offset: 0x00002154
        protected Thing sd_luciprod_rawlucibatch => job.GetTarget(TargetIndex.B).Thing;

        // Token: 0x0600002D RID: 45 RVA: 0x0000317C File Offset: 0x0000217C
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Barrel, job);
        }

        // Token: 0x0600002E RID: 46 RVA: 0x000035B0 File Offset: 0x000025B0
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
}