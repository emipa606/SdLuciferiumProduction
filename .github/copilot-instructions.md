# GitHub Copilot Instructions for [sd] Luciferium Production (Continued)

## Mod Overview and Purpose

The [sd] Luciferium Production (Continued) mod is an update of the original "luciferium production" mod by sulusdacor. It adds a way to produce luciferium, a valuable resource in RimWorld. This mod allows players to research and build facilities to extract mech oil from downed mechanoids, distill it into raw luciferium, and then convert it into luciferium pills at the drug lab. Players can also build a stasis pod to continuously extract mech oil, ensuring a steady supply of luciferium.

## Key Features and Systems

1. **Mechanite Oil Extraction**: Players can extract oil from downed mechanoids, which contains trace amounts of mechanites needed for luciferium production.
2. **Luciferium Distillery**: Players can build distilleries to convert mech oil into raw luciferium batches.
3. **Luciferium Production**: Players can use raw luciferium batches and neutroamine to produce luciferium pills at the drug lab.
4. **Stasis Pod**: Players can build a stasis pod to continuously extract mech oil from prepared mechanoid corpses.
5. **Trading**: All items related to luciferium production can be traded with exotic traders.

## Coding Patterns and Conventions

### C# Code

- **File Structure**: The mod is structured with multiple C# files, each handling specific tasks or definitions.
  - `WorkGiver_sd_luciprod_TakeLuciOutOfDistillery.cs`: Contains the work giver for taking luciferium out of the distillery.
  - `ThingDefOf.cs`: Contains definitions for various things in the game.
  - `JobDriver_sd_luciprod_TakeLuciOutOfDistillery.cs`: Contains the job driver for taking luciferium out of the distillery.
  - `JobDriver_sd_luciprod_FillDistillery.cs`: Contains the job driver for filling the distillery.
  - `JobDefOf.cs`: Contains definitions for various jobs in the game.

- **Code Patterns**:
  - Use meaningful variable and method names.
  - Follow the RimWorld coding conventions, such as using `DefOf` classes for definitions.
  - Use `JobDriver` and `WorkGiver` classes to handle job-related logic.

### XML Integration

- **File Structure**: The mod uses XML files to define various game elements such as job definitions, recipe definitions, research project definitions, and thing definitions.
  - `JobDefs\sd_luciprod_JobDefs.xml`: Contains job definitions.
  - `RecipeDefs\sd_luciprod_RecipeDefs.xml`: Contains recipe definitions.
  - `ResearchProjectDefs\sd_luciprod_ResearchProjectDefs.xml`: Contains research project definitions.
  - `ThingDefs_Buildings\sd_luciprod_ThingDefs_Buildings.xml`: Contains building definitions.
  - `ThingDefs_Items\sd_luciprod_ThingDefs_Items.xml`: Contains item definitions.

- **XML Patterns**:
  - Use the correct XML tags and attributes to define elements.
  - Ensure that all XML files are properly formatted and validated.
  - Use the `Def` tag to define new elements and the `modExtensions` tag to extend existing elements.

## Harmony Patching

The mod uses Harmony patching to modify the base game behavior. Harmony patches are used to add or modify the behavior of existing methods in the RimWorld game. The mod uses Harmony patches to:

- Add new recipes for extracting mech oil and producing luciferium.
- Add new job drivers for filling the distillery and taking luciferium out of the distillery.
- Add new research projects for luciferium production.

### Harmony Patching Patterns

- Use the `Harmony` library to create harmony patches.
- Ensure that harmony patches are properly named and documented.
- Use the `Transpiler` method to modify existing methods.

## Suggestions for Copilot

### C# Code

- **Generate WorkGiver Classes**: Copilot can generate new `WorkGiver` classes for handling new jobs.
  csharp
  public class WorkGiver_sd_luciprod_NewJob : WorkGiver_Scanner
  {
      public override PathEndMode PathEndMode => PathEndMode.OnCell;

      public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
      {
          if (t is Building_NewBuilding building)
          {
              return JobMaker.MakeJob(JobDefOf.sd_luciprod_NewJob, building);
          }
          return null;
      }

      public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
      {
          return pawn.Map.listerBuildings.allBuildingsColonist.Where(b => b is Building_NewBuilding);
      }
  }
  

- **Generate JobDriver Classes**: Copilot can generate new `JobDriver` classes for handling new jobs.
  csharp
  public class JobDriver_sd_luciprod_NewJob : JobDriver
  {
      public override void ExposeData()
      {
          base.ExposeData();
          Scribe_Values.Look(ref myField, "myField");
      }

      protected override IEnumerable<Toil> MakeNewToils()
      {
          yield return Toils_Goto.GotoCell(TargetA.Cell, PathEndMode.OnCell);
          yield return Toils_General.DoAtomic(() =>
          {
              // Do something
          });
      }
  }
  

### XML Integration

- **Generate JobDefs**: Copilot can generate new job definitions in XML.
  xml
  <Defs>
      <JobDef>
          <defName>sd_luciprod_NewJob</defName>
          <label>New Job</label>
          <description>Perform the new job.</description>
          <workTags>
              <li>WorkTags.None</li>
          </workTags>
      </JobDef>
  </Defs>
  

- **Generate RecipeDefs**: Copilot can generate new recipe definitions in XML.
  xml
  <Defs>
      <RecipeDef>
          <defName>sd_luciprod_NewRecipe</defName>
          <label>New Recipe</label>
          <description>Make something new.</description>
          <researchPrerequisite>ResearchProjectDefOf.sd_luciprod_NewResearch</researchPrerequisite>
          <ingredients>
              <li>
                  <thingDef>ThingDefOf.Ingredient1</thingDef>
                  <count>1</count>
              </li>
          </ingredients>
          <products>
              <li>
                  <thingDef>ThingDefOf.Product1</thingDef>
                  <count>1</count>
              </li>
          </products>
          <workAmount>100</workAmount>
          <workSpeedStat>StatDefOf.CraftingSpeed</workSpeedStat>
          <skillRequirements>
              <li>
                  <skillDef>SkillDefOf.Crafting</skillDef>
                  <requiredLevel>10</requiredLevel>
              </li>
          </skillRequirements>
      </RecipeDef>
  </Defs>
  

- **Generate ResearchProjectDefs**: Copilot can generate new research project definitions in XML.
  xml
  <Defs>
      <ResearchProjectDef>
          <defName>sd_luciprod_NewResearch</defName>
          <label>New Research</label>
          <description>Research something new.</description>
          <prerequisites>
              <li>ResearchProjectDefOf.Prerequisite1</li>
          </prerequisites>
          <baseCost>1000</baseCost>
          <costFactorTechLevel>2</costFactorTechLevel>
          <timeBaseDays>10</timeBaseDays>
          <timeFactorTechLevel>0.5</timeFactorTechLevel>
          <techLevel>TechLevel.Advanced</techLevel>
      </ResearchProjectDef>
  </Defs>
  

- **Generate ThingDefs**: Copilot can generate new thing definitions in XML.
  xml
  <Defs>
      <ThingDef>
          <defName>sd_luciprod_NewThing</defName>
          <label>New Thing</label>
          <description>A new thing.</description>
          <category>Items</category>
          <stackLimit>10</stackLimit>
          <thingClass>Thing</thingClass>
          <baseMarketValue>10</baseMarketValue>
          <statBases>
              <li>
                  <statDef>StatDefOf.MarketValue</statDef>
                  <value>10</value>
              </li>
          </statBases>
      </ThingDef>
  </Defs>
  

### Harmony Patching

- **Generate Harmony Patches**: Copilot can generate new harmony patches.
  csharp
  [HarmonyPatch(typeof(BaseClass), "MethodName")]
  public static class Patch_BaseClass_MethodName
  {
      [HarmonyPostfix]
      public static void Postfix(ref ReturnParameter __result)
      {
          // Do something after the method is executed
      }
  }
  

## Conclusion

The [sd] Luciferium Production (Continued) mod is a valuable addition to the RimWorld modding community, providing players with a new way to produce luciferium. By following the coding patterns and conventions outlined in this document, modders can ensure that their contributions to the mod are consistent and high-quality. Additionally, the suggestions for Copilot can help modders generate new code and XML files quickly and efficiently.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

