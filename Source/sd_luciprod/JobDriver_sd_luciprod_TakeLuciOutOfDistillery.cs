using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod
{
	// Token: 0x02000006 RID: 6
	public class JobDriver_sd_luciprod_TakeLuciOutOfDistillery : JobDriver
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600002B RID: 43 RVA: 0x00003128 File Offset: 0x00002128
		protected Building_sd_luciprod_distillery Barrel
		{
			get
			{
				return (Building_sd_luciprod_distillery)this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600002C RID: 44 RVA: 0x00003154 File Offset: 0x00002154
		protected Thing sd_luciprod_rawlucibatch
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		// Token: 0x0600002D RID: 45 RVA: 0x0000317C File Offset: 0x0000217C
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.Barrel, this.job, 1, -1, null, true);
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000035B0 File Offset: 0x000025B0
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200, TargetIndex.None).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOn(() => !this.Barrel.Distilled).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f);
			yield return new Toil
			{
				initAction = delegate()
				{
					Thing thing = this.Barrel.TakeOutrawlucibatch();
					GenPlace.TryPlaceThing(thing, this.pawn.Position, base.Map, ThingPlaceMode.Near, null, null);
					StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
					IntVec3 c;
					if (StoreUtility.TryFindBestBetterStoreCellFor(thing, this.pawn, base.Map, currentPriority, this.pawn.Faction, out c, true))
					{
						this.job.SetTarget(TargetIndex.C, c);
						this.job.SetTarget(TargetIndex.B, thing);
						this.job.count = thing.stackCount;
					}
					else
					{
						base.EndJobWith(JobCondition.Incompletable);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
			yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
			yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
			Toil toil = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return toil;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, toil, true);
			yield break;
		}

		// Token: 0x04000010 RID: 16
		private const TargetIndex BarrelInd = TargetIndex.A;

		// Token: 0x04000011 RID: 17
		private const TargetIndex sd_luciprod_rawBatchToHaulInd = TargetIndex.B;

		// Token: 0x04000012 RID: 18
		private const TargetIndex StorageCellInd = TargetIndex.C;

		// Token: 0x04000013 RID: 19
		private const int Duration = 200;
	}
}
