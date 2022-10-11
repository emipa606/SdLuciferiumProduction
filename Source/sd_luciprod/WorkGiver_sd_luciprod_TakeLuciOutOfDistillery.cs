using RimWorld;
using Verse;
using Verse.AI;

namespace sd_luciprod;

public class WorkGiver_sd_luciprod_TakeLuciOutOfDistillery : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForDef(ThingDefOf.sd_luciprod_distillery);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return t is Building_sd_luciprod_distillery { Distilled: true } && !t.IsBurning() && !t.IsForbidden(pawn) &&
               pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger());
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return new Job(JobDefOf.sd_luciprod_takelucioutofdistillery, t);
    }
}