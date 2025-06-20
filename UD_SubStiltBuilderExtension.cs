using Genkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRL;
using XRL.Wish;
using XRL.World;
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

        public static Zone CathedralZone => The.ZoneManager?.GetZone(CathedralZoneID);

        public static Location2D StiltWellLocation => GetStiltWellLocation(CathedralZone); // [38,12]

        public static Dictionary<int, string> CathedralSubStrata => GetCathedralStrataSubStrataZoneIDs(CathedralZone);

        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            // Assign the 10 yet-to-be-finished map files for the 10 strata below the stilt.
            ZoneManager zoneManager = The.ZoneManager;
            foreach ((int key, string stratumBelowZoneId) in CathedralSubStrata)
            {
                Zone stratumBelow = zoneManager.GetZone(stratumBelowZoneId);
                Cell pitCell = stratumBelow.GetCell(StiltWellLocation);

                if (key < CathedralSubStrata.Count)
                {
                    pitCell.Clear().AddObject("Air");
                }
                foreach (Cell cell in pitCell.GetAdjacentCells(BuiltOnly: true))
                {
                    // cell?.Clear().AddObject("Sandstone");
                }

                int digitDifference = CathedralSubStrata.Count.ToString().Length - key.ToString().Length;
                string leadingZeros = "";
                for (int i = 0; i < digitDifference; i++)
                {
                    leadingZeros += $"{0}";
                }
                string MapFileName = MAP_PARTIAL + $"{leadingZeros}{key}";

                // zoneManager.RemoveZoneBuilders(stratumBelowZoneId, nameof(Cave));
                // zoneManager.RemoveZoneBuilders(stratumBelowZoneId, nameof(SurfaceCave));
                // zoneManager.RemoveZoneBuilders(stratumBelowZoneId, nameof(Strata));
                zoneManager.RemoveZoneBuilders(stratumBelowZoneId, nameof(FactionEncounters));
                zoneManager.ClearZoneBuilders(stratumBelowZoneId);

                UnityEngine.Debug.LogError($"{nameof(UD_SubStiltBuilderExtension)} > {nameof(key)}: {leadingZeros}{key}");
                UnityEngine.Debug.LogError($"{MapFileName}");

                zoneManager.AddZonePostBuilder(stratumBelow.ZoneID, nameof(MapBuilder), "ID", $"{MapFileName}"); //, "ClearBeforePlace", true);

                // zoneManager.SetZoneProperty(stratumBelowZoneId, "SkipTerrainBuilders", true);
                zoneManager.SetZoneProperty(stratumBelowZoneId, "NoBiomes", "Yes");

                zoneManager.AddZonePostBuilder(stratumBelow.ZoneID, nameof(UD_SubGrandCathedralScrapifier), "WantScrappy", key > 9);

                string zoneName = $"beneath the {GameObjectFactory.Factory.GetBlueprintIfExists("StiltWell").DisplayName()}";
                zoneManager.SetZoneName(stratumBelowZoneId, zoneName);
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
