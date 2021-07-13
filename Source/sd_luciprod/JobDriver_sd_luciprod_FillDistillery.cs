using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace sd_luciprod
{
    // Token: 0x02000005 RID: 5
    public class JobDriver_sd_luciprod_FillDistillery : JobDriver
    {
        // Token: 0x0400000D RID: 13
        private const TargetIndex BarrelInd = TargetIndex.A;

        // Token: 0x0400000E RID: 14
        private const TargetIndex WortInd = TargetIndex.B;

        // Token: 0x0400000F RID: 15
        private const int Duration = 200;

        // Token: 0x1700000E RID: 14
        // (get) Token: 0x06000025 RID: 37 RVA: 0x00002D50 File Offset: 0x00001D50
        protected Building_sd_luciprod_distillery Barrel =>
            (Building_sd_luciprod_distillery) job.GetTarget(TargetIndex.A).Thing;

        // Token: 0x1700000F RID: 15
        // (get) Token: 0x06000026 RID: 38 RVA: 0x00002D7C File Offset: 0x00001D7C
        protected Thing sd_luciprod_mechanite_oil => job.GetTarget(TargetIndex.B).Thing;

        // Token: 0x06000027 RID: 39 RVA: 0x00002DA4 File Offset: 0x00001DA4
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Barrel, job) && pawn.Reserve(sd_luciprod_mechanite_oil, job);
        }

        // Token: 0x06000028 RID: 40 RVA: 0x000030FC File Offset: 0x000020FC
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
}