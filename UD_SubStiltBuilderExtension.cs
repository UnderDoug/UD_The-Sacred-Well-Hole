using Genkit;
using System.Collections.Generic;
using XRL;
using XRL.UI;
using XRL.Wish;
using XRL.World;
using XRL.World.Parts;
using XRL.World.WorldBuilders;
using XRL.World.ZoneBuilders;

namespace UD_SacredWellHole
{
    [HasWishCommand]
    [JoppaWorldBuilderExtension]
    public class UD_SubStiltBuilderExtension : IJoppaWorldBuilderExtension
    {
        public static ModInfo ThisMod = ModManager.GetMod("UD_SacredWellHole");

        public const string CathedralZoneID = "JoppaWorld.5.2.1.1.10";
        public const string MAP_PARTIAL = "UD_GrandCathedralWell_";

        public static string EmptyMaterial => "Air";

        public static Zone CathedralZone => The.ZoneManager?.GetZone(CathedralZoneID);

        public static Location2D StiltWellLocation => GetStiltWellLocation(CathedralZone); // [38,12]

        public static Dictionary<int, string> CathedralSubStrata => GetCathedralStrataSubStrataZoneIDs(CathedralZone);

        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            MetricsManager.rngCheckpoint("sacrifice");
            builder.BuildStep("Sacrificing artifacts", SacrificeArtifacts);
        }

        public void SacrificeArtifacts(string WorldID)
        {
            if (!(WorldID == "JoppaWorld"))
            {
                return;
            }
            WorldCreationProgress.StepProgress("Sacrificing artifacts...");

            // Assign the 10 yet-to-be-finished map files for the 10 strata below the stilt.
            List<Location2D> solidJunkPileCellsBelow = new();
            ZoneManager zoneManager = The.ZoneManager;
            Dictionary<int, string> cathedralSubStrata = CathedralSubStrata;
            for (int i = cathedralSubStrata.Count; i > 0; i--)
            {
                Zone stratumBelow = zoneManager.GetZone(cathedralSubStrata[i]);
                string stratumBelowZoneID = stratumBelow.ZoneID;
                Cell stiltWellHoleCell = stratumBelow.GetCell(StiltWellLocation);


                int digitDifference = cathedralSubStrata.Count.ToString().Length - i.ToString().Length;
                string leadingZeros = "";
                for (int j = 0; j < digitDifference; j++)
                {
                    leadingZeros += $"{0}";
                }
                string MapFileName = MAP_PARTIAL + $"{leadingZeros}{i}";

                zoneManager.RemoveZoneBuilders(stratumBelowZoneID, nameof(Cave));
                zoneManager.RemoveZoneBuilders(stratumBelowZoneID, nameof(SurfaceCave));
                zoneManager.RemoveZoneBuilders(stratumBelowZoneID, nameof(Strata));
                zoneManager.RemoveZoneBuilders(stratumBelowZoneID, nameof(FactionEncounters));
                zoneManager.ClearZoneBuilders(stratumBelowZoneID);

                // UnityEngine.Debug.LogError($"{nameof(UD_SubStiltBuilderExtension)} > {nameof(i)}: {leadingZeros}{i}");
                // UnityEngine.Debug.LogError($"{MapFileName}");

                // zoneManager.AddZonePostBuilder(stratumBelowZoneID, nameof(MapBuilder), "ID", $"{MapFileName}"); //, "ClearBeforePlace", true);
                zoneManager.AddZonePostBuilder(stratumBelowZoneID, nameof(Trashy)); //, "ClearBeforePlace", true);

                zoneManager.SetZoneProperty(stratumBelowZoneID, "SkipTerrainBuilders", true);
                // zoneManager.SetZoneProperty(stratumBelowZoneID, "NoBiomes", "Yes");

                List<Location2D> previousSolidJunkPileCellsBelow = new();
                if (!solidJunkPileCellsBelow.IsNullOrEmpty())
                {
                    foreach (Location2D location in solidJunkPileCellsBelow)
                    {
                        previousSolidJunkPileCellsBelow.Add(location);
                    }
                }
                /*
                solidJunkPileCellsBelow = new(new UD_SubGrandCathedralBuilder().BuildZone(
                    Z: stratumBelow,
                    StiltWellLocation: StiltWellLocation,
                    SolidJunkPileCellsBelow: previousSolidJunkPileCellsBelow,
                    WantScrappy: i > (cathedralSubStrata.Count / 2) - 1));
                */

                
                zoneManager.AddZonePostBuilder(
                    ZoneID: stratumBelowZoneID, 
                    Class: nameof(UD_SubGrandCathedralBuilder), 
                    Key1: "StiltWellLocation", Value1: StiltWellLocation,
                    Key2: "SolidJunkPileCellsBelow", Value2: solidJunkPileCellsBelow,
                    Key3: "WantScrappy", Value3: i > cathedralSubStrata.Count -1
                    );
                
                string zoneName = $"beneath the {GameObjectFactory.Factory.GetBlueprintIfExists("StiltWell").DisplayName()}";
                zoneManager.SetZoneName(stratumBelowZoneID, zoneName);
                // solidJunkPileCellsBelow = new(stratumBelow.GetZoneProperty(UD_SubGrandCathedralBuilder.JUNKPILE_SOLID_CELLS_PROP).RegionFromString());

                if (false && i > (cathedralSubStrata.Count / 2))
                {
                    stiltWellHoleCell.Clear().AddObject(EmptyMaterial);
                }
                foreach (Cell cell in stiltWellHoleCell.GetAdjacentCells(BuiltOnly: true))
                {
                    // cell?.Clear().AddObject("Sandstone");
                }
            }
        }

        public static Location2D GetStiltWellLocation(Zone Z)
        {
            List<Cell> cellsList = Z?.GetCellsWithObject("StiltWell");
            if (!cellsList.IsNullOrEmpty())
            {
                return cellsList[0].Location;
            }
            return null;
        }

        public static Dictionary<int, string> GetCathedralStrataSubStrataZoneIDs(Zone Z)
        {
            if (Z == null)
            {
                return null;
            }
            Dictionary<int, string> cathedralStrata = new();
            for (int i = 1; i < 11; i++)
            {
                int stratum = Z.Z + i;

                string stratumBelow = ZoneID.Assemble(Z.ZoneWorld, Z.wX, Z.wY, Z.X, Z.Y, stratum);
                if (stratumBelow != null)
                {
                    cathedralStrata.TryAdd(i, stratumBelow);
                }
            }
            return cathedralStrata;
        }

        [WishCommand(Command = "go stilt")]
        public static void GoStilt()
        {
            Zone Z = The.ZoneManager.GetZone(CathedralZoneID);
            The.Player.Physics.CurrentCell.RemoveObject(The.Player.Physics.ParentObject);
            Z.GetEmptyCells().GetRandomElement().AddObject(The.Player);
            The.ZoneManager.SetActiveZone(Z);
            The.ZoneManager.ProcessGoToPartyLeader();
        }
    }
}
