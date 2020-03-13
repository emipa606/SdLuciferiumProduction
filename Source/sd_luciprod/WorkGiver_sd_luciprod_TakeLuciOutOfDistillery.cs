using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod
{
	// Token: 0x02000004 RID: 4
	public class WorkGiver_sd_luciprod_TakeLuciOutOfDistillery : WorkGiver_Scanner
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000020 RID: 32 RVA: 0x00002CA4 File Offset: 0x00001CA4
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.sd_luciprod_distillery);
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000021 RID: 33 RVA: 0x00002CC0 File Offset: 0x00001CC0
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002CD4 File Offset: 0x00001CD4
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_sd_luciprod_distillery building_sd_luciprod_distillery = t as Building_sd_luciprod_distillery;
			return building_sd_luciprod_distillery != null && building_sd_luciprod_distillery.Distilled && !t.IsBurning() && !t.IsForbidden(pawn) && pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, false);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002D24 File Offset: 0x00001D24
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return new Job(JobDefOf.sd_luciprod_takelucioutofdistillery, t);
		}
	}
}
