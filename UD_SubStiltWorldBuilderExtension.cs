using System.Collections.Generic;

using Genkit;

using XRL.UI;
using XRL.Wish;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

namespace XRL.World.WorldBuilders
{
    [HasWishCommand]
    [JoppaWorldBuilderExtension]
    public class UD_SubStiltWorldBuilderExtension : IJoppaWorldBuilderExtension
    {
        private static bool doDebug => getClassDoDebug(nameof(UD_SubStiltWorldBuilderExtension));
        private static bool getDoDebug(object what = null)
        {
            List<object> doList = new()
            {
                'V',    // Vomit
            };
            List<object> dontList = new()
            {
                'X',    // Trace
            };

            if (what != null && doList.Contains(what))
                return true;

            if (what != null && dontList.Contains(what))
                return false;

            return doDebug;
        }

        public static Zone CathedralZone => The.ZoneManager?.GetZone(CATHEDRAL_ZONE_ID);

        public static Location2D StiltWellLocation => GetStiltWellLocation(CathedralZone); // [38,12]

        public static Dictionary<int, string> CathedralSubStrata => GetCathedralStrataSubStrataZoneIDs(CathedralZone);

        public override void OnAfterBuild(JoppaWorldBuilder Builder)
        {
            Debug.Header(4, $"{nameof(UD_SubStiltWorldBuilderExtension)}", $"{nameof(OnAfterBuild)}", Toggle: doDebug);
            MetricsManager.rngCheckpoint("sacrifice");
            Builder.BuildStep("Sacrificing artifacts", SacrificeArtifacts);
            Debug.Footer(4, $"{nameof(UD_SubStiltWorldBuilderExtension)}", $"{nameof(OnAfterBuild)}", Toggle: doDebug);
        }

        public void SacrificeArtifacts(string WorldID)
        {
            int indent = Debug.LastIndent;
            Debug.Entry(4,
                $"* {nameof(UD_SubStiltWorldBuilderExtension)}."
                + $"{nameof(SacrificeArtifacts)}("
                + $"{nameof(WorldID)}: {WorldID})",
                Indent: indent + 1, Toggle: doDebug);

            if (WorldID != "JoppaWorld")
            {
                Debug.LastIndent = indent;
                return;
            }
            WorldCreationProgress.StepProgress("Sacrificing artifacts...");

            Debug.Entry(4,
                $"x {nameof(UD_SubStiltWorldBuilderExtension)}."
                + $"{nameof(SacrificeArtifacts)}("
                + $"{nameof(WorldID)}: {WorldID})"
                + $" *//",
                Indent: indent + 1, Toggle: doDebug);
            Debug.LastIndent = indent;
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
            Zone Z = The.ZoneManager.GetZone(CATHEDRAL_ZONE_ID);
            The.Player.Physics.CurrentCell.RemoveObject(The.Player.Physics.ParentObject);
            Cell landingCell = Z.GetCell(StiltWellLocation).GetCellFromDirection("S") ?? Z.GetEmptyCells().GetRandomElement();
            landingCell.AddObject(The.Player);
            The.ZoneManager.SetActiveZone(Z);
            The.ZoneManager.ProcessGoToPartyLeader();
        }
    }
}
