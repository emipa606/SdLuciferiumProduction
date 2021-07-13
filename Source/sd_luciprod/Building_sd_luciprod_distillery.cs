using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace sd_luciprod
{
    // Token: 0x02000002 RID: 2
    [StaticConstructorOnStartup]
    public class Building_sd_luciprod_distillery : Building
    {
        // Token: 0x04000001 RID: 1
        private const int MaxCapacity = 10;

        // Token: 0x04000002 RID: 2
        private const int BaseFermentationDuration = 600000;

        // Token: 0x04000003 RID: 3
        private const float MinIdealTemperature = 7f;

        // Token: 0x04000007 RID: 7
        private static readonly Vector2 BarSize = new Vector2(0.55f, 0.1f);

        // Token: 0x04000008 RID: 8
        private static readonly Color BarZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);

        // Token: 0x04000009 RID: 9
        private static readonly Color BarFermentedColor = new Color(0.9f, 0.85f, 0.2f);

        // Token: 0x0400000A RID: 10
        private static readonly Material BarUnfilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f));

        // Token: 0x04000006 RID: 6
        private Material barFilledCachedMat;

        // Token: 0x04000005 RID: 5
        private float progressInt;

        // Token: 0x04000004 RID: 4
        private int sd_luciprod_oilcount;

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00001050
        // (set) Token: 0x06000002 RID: 2 RVA: 0x00002068 File Offset: 0x00001068
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

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000003 RID: 3 RVA: 0x00002098 File Offset: 0x00001098
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

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000004 RID: 4 RVA: 0x000020E8 File Offset: 0x000010E8
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

        // Token: 0x17000004 RID: 4
        // (get) Token: 0x06000005 RID: 5 RVA: 0x00002138 File Offset: 0x00001138
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

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x06000006 RID: 6 RVA: 0x00002168 File Offset: 0x00001168
        private bool Empty => sd_luciprod_oilcount <= 0;

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000007 RID: 7 RVA: 0x00002188 File Offset: 0x00001188
        public bool Distilled => !Empty && Progress >= 1f;

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000008 RID: 8 RVA: 0x000021B8 File Offset: 0x000011B8
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

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000009 RID: 9 RVA: 0x0000222C File Offset: 0x0000122C
        private float ProgressPerTickAtCurrentTemp => 1.66666666E-06f * CurrentTempProgressSpeedFactor * 2f;

        // Token: 0x17000009 RID: 9
        // (get) Token: 0x0600000A RID: 10 RVA: 0x00002250 File Offset: 0x00001250
        private int EstimatedTicksLeft =>
            Mathf.Max(Mathf.RoundToInt((1f - Progress) / ProgressPerTickAtCurrentTemp), 0);

        // Token: 0x0600000B RID: 11 RVA: 0x00002280 File Offset: 0x00001280
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref sd_luciprod_oilcount, "oilcount");
            Scribe_Values.Look(ref progressInt, "progress");
        }

        // Token: 0x0600000C RID: 12 RVA: 0x000022B4 File Offset: 0x000012B4
        public override void TickRare()
        {
            base.TickRare();
            if (!Empty)
            {
                Progress = Mathf.Min(Progress + (250f * ProgressPerTickAtCurrentTemp), 1f);
            }
        }

        // Token: 0x0600000D RID: 13 RVA: 0x000022FC File Offset: 0x000012FC
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

        // Token: 0x0600000E RID: 14 RVA: 0x0000237C File Offset: 0x0000137C
        protected override void ReceiveCompSignal(string signal)
        {
            if (signal == "RuinedByTemperature")
            {
                Reset();
            }
        }

        // Token: 0x0600000F RID: 15 RVA: 0x000023A5 File Offset: 0x000013A5
        private void Reset()
        {
            sd_luciprod_oilcount = 0;
            Progress = 0f;
        }

        // Token: 0x06000010 RID: 16 RVA: 0x000023BC File Offset: 0x000013BC
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

        // Token: 0x06000011 RID: 17 RVA: 0x000023FC File Offset: 0x000013FC
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

        // Token: 0x06000012 RID: 18 RVA: 0x00002660 File Offset: 0x00001660
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

        // Token: 0x06000013 RID: 19 RVA: 0x000026B0 File Offset: 0x000016B0
        public override void Draw()
        {
            base.Draw();
            if (Empty)
            {
                return;
            }

            var drawPos = DrawPos;
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

        // Token: 0x06000014 RID: 20 RVA: 0x00002974 File Offset: 0x00001974
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
}