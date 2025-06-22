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

        public override void OnAfterBuild(JoppaWorldBuilder Builder)
        {
            MetricsManager.rngCheckpoint("sacrifice");
            Builder.BuildStep("Sacrificing artifacts", SacrificeArtifacts);
        }

        public void SacrificeArtifacts(string WorldID)
        {
            if (!(WorldID == "JoppaWorld"))
            {
                return;
            }
            WorldCreationProgress.StepProgress("Sacrificing artifacts...");
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
