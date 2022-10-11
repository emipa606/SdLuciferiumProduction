using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod;

public class WorkGiver_sd_luciprod_FillDistillery : WorkGiver_Scanner
{
    private static string TemperatureTrans;

    private static string sd_luciprod_mechanite_No_oil_Trans;

    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForDef(ThingDefOf.sd_luciprod_distillery);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public static void Reset()
    {
        TemperatureTrans = "sd_luciprod_txtbadtemperature".Translate().ToLower();
        sd_luciprod_mechanite_No_oil_Trans = "sd_luciprod_txtnomechaniteoil".Translate();
    }

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

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var building_sd_luciprod_distillery = (Building_sd_luciprod_distillery)t;
        var t2 = Findmech_oil(pawn);
        return new Job(JobDefOf.sd_luciprod_filldistillery, t, t2)
        {
            count = building_sd_luciprod_distillery.sd_luciprod_SpaceLeftFor_oil
        };
    }

    private Thing Findmech_oil(Pawn pawn)
    {
        bool Predicate(Thing x)
        {
            return !x.IsForbidden(pawn) && pawn.CanReserve(x);
        }

        var validator = (Predicate<Thing>)Predicate;
        return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.sd_luciprod_mechanite_oil), PathEndMode.ClosestTouch,
            TraverseParms.For(pawn), 9999f, validator);
    }
}