using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace sd_luciprod;

[StaticConstructorOnStartup]
public class Building_sd_luciprod_distillery : Building
{
    private const int MaxCapacity = 10;

    private const int BaseFermentationDuration = 600000;

    private const float MinIdealTemperature = 7f;

    private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

    private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

    private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

    private static readonly Material BarUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

    private Material barFilledCachedMat;

    private float progressInt;

    private int sd_luciprod_oilcount;

    public float Progress
    {
        get => progressInt;
        set
        {
            if (value == progressInt)
            {
                return;
            }

            progressInt = value;
            barFilledCachedMat = null;
        }
    }

    private Material BarFilledMat
    {
        get
        {
            if (barFilledCachedMat == null)
            {
                barFilledCachedMat =
                    SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(BarZeroProgressColor, BarFermentedColor,
                        Progress));
            }

            return barFilledCachedMat;
        }
    }

    private float Temperature
    {
        get
        {
            float result;
            if (MapHeld == null)
            {
                Log.ErrorOnce("Tried to get a distillery temperature but MapHeld is null.", 847163513);
                result = 7f;
            }
            else
            {
                result = PositionHeld.GetTemperature(MapHeld);
            }

            return result;
        }
    }

    public int sd_luciprod_SpaceLeftFor_oil
    {
        get
        {
            int result;
            if (Distilled)
            {
                result = 0;
            }
            else
            {
                result = checked(10 - sd_luciprod_oilcount);
            }

            return result;
        }
    }

    private bool Empty => sd_luciprod_oilcount <= 0;

    public bool Distilled => !Empty && Progress >= 1f;

    private float CurrentTempProgressSpeedFactor
    {
        get
        {
            var compProperties = def.GetCompProperties<CompProperties_TemperatureRuinable>();
            var temperature = Temperature;
            float result;
            if (temperature < compProperties.minSafeTemperature)
            {
                result = 0.1f;
            }
            else if (temperature < 7f)
            {
                result = GenMath.LerpDouble(compProperties.minSafeTemperature, 7f, 0.1f, 1f, temperature);
            }
            else
            {
                result = 1f;
            }

            return result;
        }
    }

    private float ProgressPerTickAtCurrentTemp => 1.66666666E-06f * CurrentTempProgressSpeedFactor * 2f;

    private int EstimatedTicksLeft =>
        Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref sd_luciprod_oilcount, "oilcount");
        Scribe_Values.Look(ref progressInt, "progress");
    }

    public override void TickRare()
    {
        base.TickRare();
        if (!Empty)
        {
            Progress = Mathf.Min(Progress + (250f * ProgressPerTickAtCurrentTemp), 1f);
        }
    }

    public void Addoil(int count)
    {
        checked
        {
            if (Distilled)
            {
                Log.Warning(
                    "Tried to add mechanite oil to distillery full of raw luciferium batch. Colonists should take out the raw luciferium batch first.");
            }
            else
            {
                var num = Mathf.Min(count, 10 - sd_luciprod_oilcount);
                if (num <= 0)
                {
                    return;
                }

                Progress = GenMath.WeightedAverage(0f, num, Progress, sd_luciprod_oilcount);
                sd_luciprod_oilcount += num;
                GetComp<CompTemperatureRuinable>().Reset();
            }
        }
    }

    protected override void ReceiveCompSignal(string signal)
    {
        if (signal == "RuinedByTemperature")
        {
            Reset();
        }
    }

    private void Reset()
    {
        sd_luciprod_oilcount = 0;
        Progress = 0f;
    }

    public void Addoil(Thing sd_luciprod_mechanite_oil)
    {
        var comp = GetComp<CompTemperatureRuinable>();
        if (comp.Ruined)
        {
            comp.Reset();
        }

        Addoil(sd_luciprod_mechanite_oil.stackCount);
        sd_luciprod_mechanite_oil.Destroy();
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());
        var comp = GetComp<CompTemperatureRuinable>();
        if (stringBuilder.Length != 0)
        {
            stringBuilder.AppendLine();
        }

        if (!Empty && !comp.Ruined)
        {
            if (Distilled)
            {
                stringBuilder.AppendLine(string.Concat("sd_luciprod_txtcontainsrawluci".Translate(), ": ",
                    sd_luciprod_oilcount, "/10"));
            }
            else
            {
                stringBuilder.AppendLine(string.Concat("sd_luciprod_txtcontainsoil".Translate(), ": ",
                    sd_luciprod_oilcount, "/10"));
            }
        }

        if (!Empty)
        {
            if (Distilled)
            {
                stringBuilder.AppendLine("sd_luciprod_txtdistilled".Translate());
            }
            else
            {
                stringBuilder.AppendLine(string.Concat("sd_luciprod_txtdistillingprogress".Translate(), ": ",
                    Progress.ToStringPercent(), " ~ ", EstimatedTicksLeft.ToStringTicksToPeriod()));
                if (CurrentTempProgressSpeedFactor != 1f)
                {
                    stringBuilder.AppendLine("sd_luciprod_txtdistilleryoutofidealtemperature".Translate() + ": " +
                                             CurrentTempProgressSpeedFactor.ToStringPercent());
                }
            }
        }

        if (MapHeld != null)
        {
            stringBuilder.AppendLine("sd_luciprod_txttemperature".Translate() + ": " +
                                     Temperature.ToStringTemperature("F0"));
        }

        stringBuilder.AppendLine(string.Concat("sd_luciprod_txtidealdistillingtemperature".Translate(), ": ",
            7f.ToStringTemperature("F0"), " ~ ", comp.Props.maxSafeTemperature.ToStringTemperature("F0")));
        return stringBuilder.ToString().TrimEndNewlines();
    }

    public Thing TakeOutrawlucibatch()
    {
        Thing result;
        if (!Distilled)
        {
            Log.Warning("Tried to get raw luciferium batch but it's not yet distilled.");
            result = null;
        }
        else
        {
            var thing = ThingMaker.MakeThing(ThingDefOf.sd_luciprod_rawlucibatch);
            thing.stackCount = sd_luciprod_oilcount / 2;
            Reset();
            result = thing;
        }

        return result;
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        if (Empty)
        {
            return;
        }

        var drawPos = drawLoc;
        drawPos.y += 0.05f;
        drawPos.z += 0.25f;
        GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
        {
            center = drawPos,
            size = BarSize,
            fillPercent = sd_luciprod_oilcount / 10f,
            filledMat = BarFilledMat,
            unfilledMat = BarUnfilledMat,
            margin = 0.1f,
            rotation = Rot4.North
        });
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var current in base.GetGizmos())
        {
            yield return current;
        }

        if (!Prefs.DevMode || Empty)
        {
            yield break;
        }

        var command_Action = new Command_Action
        {
            defaultLabel = "Debug: Set progress to 1", action = delegate { Progress = 1f; }
        };
        yield return command_Action;
    }
}