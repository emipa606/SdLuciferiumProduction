using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace sd_luciprod
{
	// Token: 0x02000005 RID: 5
	public class JobDriver_sd_luciprod_FillDistillery : JobDriver
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000025 RID: 37 RVA: 0x00002D50 File Offset: 0x00001D50
		protected Building_sd_luciprod_distillery Barrel
		{
			get
			{
				return (Building_sd_luciprod_distillery)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000026 RID: 38 RVA: 0x00002D7C File Offset: 0x00001D7C
		protected Thing sd_luciprod_mechanite_oil
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002DA4 File Offset: 0x00001DA4
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.Barrel, this.job, 1, -1, null, true) && this.pawn.Reserve(this.sd_luciprod_mechanite_oil, this.job, 1, -1, null, true);
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000030FC File Offset: 0x000020FC
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			Toil toil = Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return toil;
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false).FailOnDestroyedNullOrForbidden(TargetIndex.B);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(toil, TargetIndex.B, TargetIndex.None, false, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.B).FailOnDestroyedNullOrForbidden(TargetIndex.A).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate()
				{
					this.Barrel.Addoil(this.sd_luciprod_mechanite_oil);
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield break;
		}

		// Token: 0x0400000D RID: 13
		private const TargetIndex BarrelInd = TargetIndex.A;

		// Token: 0x0400000E RID: 14
		private const TargetIndex WortInd = TargetIndex.B;

		// Token: 0x0400000F RID: 15
		private const int Duration = 200;
	}
}
