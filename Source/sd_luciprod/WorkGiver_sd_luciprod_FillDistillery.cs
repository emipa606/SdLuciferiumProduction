using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod;

public class WorkGiver_sd_luciprod_FillDistillery : WorkGiver_Scanner
{
    private static string temperatureTrans;

    private static string sdLuciprodMechaniteNoOilTrans;

    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForDef(ThingDefOf.sd_luciprod_distillery);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public static void Reset()
    {
        temperatureTrans = "sd_luciprod_txtbadtemperature".Translate().ToLower();
        sdLuciprodMechaniteNoOilTrans = "sd_luciprod_txtnomechaniteoil".Translate();
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        bool result;
        if (t is not Building_sd_luciprod_distillery building_sd_luciprod_distillery ||
            building_sd_luciprod_distillery.Distilled ||
            building_sd_luciprod_distillery.SDLuciprodSpaceLeftForOil <= 0)
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
                JobFailReason.Is(temperatureTrans);
                result = false;
            }
            else if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()) ||
                     pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                result = false;
            }
            else if (findMechOil(pawn) == null)
            {
                JobFailReason.Is(sdLuciprodMechaniteNoOilTrans);
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
        var buildingSDLuciprodDistillery = (Building_sd_luciprod_distillery)t;
        var t2 = findMechOil(pawn);
        return new Job(JobDefOf.sd_luciprod_filldistillery, t, t2)
        {
            count = buildingSDLuciprodDistillery.SDLuciprodSpaceLeftForOil
        };
    }

    private static Thing findMechOil(Pawn pawn)
    {
        var validator = (Predicate<Thing>)predicate;
        return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
            ThingRequest.ForDef(ThingDefOf.sd_luciprod_mechanite_oil), PathEndMode.ClosestTouch,
            TraverseParms.For(pawn), 9999f, validator);

        bool predicate(Thing x)
        {
            return !x.IsForbidden(pawn) && pawn.CanReserve(x);
        }
    }
}