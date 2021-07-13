using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod
{
    // Token: 0x02000003 RID: 3
    public class WorkGiver_sd_luciprod_FillDistillery : WorkGiver_Scanner
    {
        // Token: 0x0400000B RID: 11
        private static string TemperatureTrans;

        // Token: 0x0400000C RID: 12
        private static string sd_luciprod_mechanite_No_oil_Trans;

        // Token: 0x1700000A RID: 10
        // (get) Token: 0x06000019 RID: 25 RVA: 0x00002A2C File Offset: 0x00001A2C
        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForDef(ThingDefOf.sd_luciprod_distillery);

        // Token: 0x1700000B RID: 11
        // (get) Token: 0x0600001A RID: 26 RVA: 0x00002A48 File Offset: 0x00001A48
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        // Token: 0x0600001B RID: 27 RVA: 0x00002A5B File Offset: 0x00001A5B
        public static void Reset()
        {
            TemperatureTrans = "sd_luciprod_txtbadtemperature".Translate().ToLower();
            sd_luciprod_mechanite_No_oil_Trans = "sd_luciprod_txtnomechaniteoil".Translate();
        }

        // Token: 0x0600001C RID: 28 RVA: 0x00002A84 File Offset: 0x00001A84
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            bool result;
            if (!(t is Building_sd_luciprod_distillery building_sd_luciprod_distillery) ||
                building_sd_luciprod_distillery.Distilled ||
                building_sd_luciprod_distillery.sd_luciprod_SpaceLeftFor_oil <= 0)
            {
                result = false;
            }
            else
            {
                var temperature =
                    building_sd_luciprod_distillery.Position.GetTemperature(building_sd_luciprod_distillery.Map);
                var compProperties = building_sd_luciprod_distillery.def
                    .GetCompProperties<CompProperties_TemperatureRuinable>();
                if (temperature < compProperties.minSafeTemperature + 2f ||
                    temperature > compProperties.maxSafeTemperature - 2f)
                {
                    JobFailReason.Is(TemperatureTrans);
                    result = false;
                }
                else if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()))
                {
                    result = false;
                }
                else if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
                {
                    result = false;
                }
                else if (Findmech_oil(pawn) == null)
                {
                    JobFailReason.Is(sd_luciprod_mechanite_No_oil_Trans);
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
            var building_sd_luciprod_distillery = (Building_sd_luciprod_distillery) t;
            var t2 = Findmech_oil(pawn);
            return new Job(JobDefOf.sd_luciprod_filldistillery, t, t2)
            {
                count = building_sd_luciprod_distillery.sd_luciprod_SpaceLeftFor_oil
            };
        }

        // Token: 0x0600001E RID: 30 RVA: 0x00002C2C File Offset: 0x00001C2C
        private Thing Findmech_oil(Pawn pawn)
        {
            bool Predicate(Thing x)
            {
                return !x.IsForbidden(pawn) && pawn.CanReserve(x);
            }

            var validator = (Predicate<Thing>) Predicate;
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForDef(ThingDefOf.sd_luciprod_mechanite_oil), PathEndMode.ClosestTouch,
                TraverseParms.For(pawn), 9999f, validator);
        }
    }
}