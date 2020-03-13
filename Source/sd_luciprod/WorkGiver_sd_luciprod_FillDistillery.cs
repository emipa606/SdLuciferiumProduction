using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod
{
	// Token: 0x02000003 RID: 3
	public class WorkGiver_sd_luciprod_FillDistillery : WorkGiver_Scanner
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002A2C File Offset: 0x00001A2C
		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForDef(ThingDefOf.sd_luciprod_distillery);
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002A48 File Offset: 0x00001A48
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00002A5B File Offset: 0x00001A5B
		public static void Reset()
		{
			WorkGiver_sd_luciprod_FillDistillery.TemperatureTrans = Translator.Translate("sd_luciprod_txtbadtemperature").ToLower();
			WorkGiver_sd_luciprod_FillDistillery.sd_luciprod_mechanite_No_oil_Trans = Translator.Translate("sd_luciprod_txtnomechaniteoil");
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00002A84 File Offset: 0x00001A84
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_sd_luciprod_distillery building_sd_luciprod_distillery = t as Building_sd_luciprod_distillery;
			bool result;
			if (building_sd_luciprod_distillery == null || building_sd_luciprod_distillery.Distilled || building_sd_luciprod_distillery.sd_luciprod_SpaceLeftFor_oil <= 0)
			{
				result = false;
			}
			else
			{
				float temperature = building_sd_luciprod_distillery.Position.GetTemperature(building_sd_luciprod_distillery.Map);
				CompProperties_TemperatureRuinable compProperties = building_sd_luciprod_distillery.def.GetCompProperties<CompProperties_TemperatureRuinable>();
				if (temperature < compProperties.minSafeTemperature + 2f || temperature > compProperties.maxSafeTemperature - 2f)
				{
					JobFailReason.Is(WorkGiver_sd_luciprod_FillDistillery.TemperatureTrans, null);
					result = false;
				}
				else if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, false))
				{
					result = false;
				}
				else if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
				{
					result = false;
				}
				else if (this.Findmech_oil(pawn, building_sd_luciprod_distillery) == null)
				{
					JobFailReason.Is(WorkGiver_sd_luciprod_FillDistillery.sd_luciprod_mechanite_No_oil_Trans, null);
					result = false;
				}
				else
				{
					result = !t.IsBurning();
				}
			}
			return result;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00002BA4 File Offset: 0x00001BA4
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_sd_luciprod_distillery building_sd_luciprod_distillery = (Building_sd_luciprod_distillery)t;
			Thing t2 = this.Findmech_oil(pawn, building_sd_luciprod_distillery);
			return new Job(JobDefOf.sd_luciprod_filldistillery, t, t2)
			{
				count = building_sd_luciprod_distillery.sd_luciprod_SpaceLeftFor_oil
			};
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002C2C File Offset: 0x00001C2C
		private Thing Findmech_oil(Pawn pawn, Building_sd_luciprod_distillery barrel)
		{
			Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(ThingDefOf.sd_luciprod_mechanite_oil), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
		}

		// Token: 0x0400000B RID: 11
		private static string TemperatureTrans;

		// Token: 0x0400000C RID: 12
		private static string sd_luciprod_mechanite_No_oil_Trans;
	}
}
