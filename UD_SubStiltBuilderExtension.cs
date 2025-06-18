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

        public static Zone CathedralZone => The.ZoneManager.GetZone(CathedralZoneID);

        public static Location2D StiltWellLocation => GetStiltWellLocation(CathedralZone); // [38,12]

        public static Dictionary<int, Zone> CathedralSubStrata => GetCathedralStrataSubStrata(CathedralZone);

        public override void OnAfterBuild(JoppaWorldBuilder builder)
        {
            // Assign the 10 yet-to-be-created map files for the 10 strata below the stilt.
            ZoneManager zoneManager = The.ZoneManager;
            foreach ((int key, Zone stratumBelow) in CathedralSubStrata)
            {
                Cell pitCell = stratumBelow.GetCell(StiltWellLocation);
                
                pitCell.Clear().AddObject("Air");
                foreach (Cell cell in pitCell.GetAdjacentCells(BuiltOnly: true))
                {
                    // cell?.Clear().AddObject("Sandstone");
                }

                string MapFileName = MAP_PARTIAL + $"{key}".PadLeft(CathedralSubStrata.Count.ToString().Length, '0');

                if (key == 1)
                {
                    zoneManager.AddZonePostBuilder(stratumBelow.ZoneID, nameof(MapBuilder), "FileName", $"{MapFileName}", "ClearBeforePlace", false);
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

        public static Dictionary<int, Zone> GetCathedralStrataSubStrata(Zone Z)
        {
            if (Z == null)
            {
                return null;
            }
            Dictionary<int, Zone> cathedralStrata = new();
            for (int i = 1; i < 11; i++)
            {
                int stratum = Z.Z + i;
                Zone stratumBelow = The.ZoneManager.GetZone(Z.ZoneWorld, Z.wX, Z.wY, Z.X, Z.Y, stratum);
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
