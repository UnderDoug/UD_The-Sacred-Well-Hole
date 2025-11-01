using System.Collections.Generic;
using System.Diagnostics;

using Genkit;

using XRL.UI;
using XRL.Wish;
using XRL.World.ZoneBuilders;

using Verbosity = UD_Modding_Toolbox.UD_Logger.Verbosity;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;
using Debug = UD_SacredWellHole.Debug;
using System;

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

        public static List<string> CathedralSubStrata => GetCathedralStrataSubStrataZoneIDs(CathedralZone);
        public static int LowestWellStratum => 25;

        private JoppaWorldBuilder Builder = null;

        public override void OnAfterBuild(JoppaWorldBuilder Builder)
        {
            Debug.Logger.Header(Verbosity.Max, $"{nameof(UD_SubStiltWorldBuilderExtension)}", $"{nameof(OnAfterBuild)}", Toggle: doDebug);
            MetricsManager.rngCheckpoint("sacrifice");
            this.Builder = Builder;
            Builder.BuildStep("Sacrificing artifacts", SacrificeArtifacts);
            Debug.Logger.Footer(Verbosity.Max, $"{nameof(UD_SubStiltWorldBuilderExtension)}", $"{nameof(OnAfterBuild)}", Toggle: doDebug);
        }

        public void SacrificeArtifacts(string WorldID)
        {
            Debug.Logger.GetIndent(out int indent);
            Debug.Logger.Entry(Verbosity.Max,
                $"* {nameof(UD_SubStiltWorldBuilderExtension)}."
                + $"{nameof(SacrificeArtifacts)}("
                + $"{nameof(WorldID)}: {WorldID})",
                Indent: indent + 1, Toggle: doDebug);

            if (WorldID != "JoppaWorld")
            {
                Debug.Logger.SetIndent(indent);
                return;
            }
            WorldCreationProgress.StepProgress("Sacrificing artifacts...");

            if (The.ZoneManager is ZoneManager zoneManager)
            {
                UnityEngine.Debug.Log("Adjusting builders...");
                foreach (string zID in CathedralSubStrata)
                {
                    int stratum = CathedralSubStrata.IndexOf(zID) + 1;
                    UnityEngine.Debug.Log("    [" + stratum + "] " + zID);
                    zoneManager.SetZoneProperty(zID, "NoBiomes", "Yes");
                    zoneManager.SetZoneProperty(zID, "ZoneTierOverride", 5.ToString());
                    zoneManager.SetZoneProperty(zID, "DisableForcedConnections", "Yes");
                    // zoneManager.RemoveZoneBuilders(zID, nameof(FactionEncounters));
                    zoneManager.RemoveZoneBuilders(zID, nameof(Strata));
                }

                if (Preload_WellHoleZones_DuringWorldgen)
                {
                    Stopwatch sw = new();
                    sw.Start();
                    bool prebuildSuccess = false;
                    try
                    {
                        if (!CathedralSubStrata.IsNullOrEmpty())
                        {
                            int strataCount = CathedralSubStrata.Count;

                            UnityEngine.Debug.Log("Attempting Pre-build Bottom-Up...");
                            for (int i = strataCount - 1; i >= 0; i--)
                            {
                                if (CathedralSubStrata[i] is string stratum)
                                {
                                    UnityEngine.Debug.Log("    Pre-build " + stratum + "...");
                                    zoneManager.GenerateZone(stratum);
                                    prebuildSuccess = true;
                                }
                            }
                            if (!prebuildSuccess
                                && CathedralSubStrata[0] is string firstStratum)
                            {
                                UnityEngine.Debug.Log("Attempting Pre-build First Straum (cascade)...");
                                UnityEngine.Debug.Log("    Pre-build " + firstStratum + "...");
                                zoneManager.GenerateZone(firstStratum);
                                prebuildSuccess = true;
                            }
                        }
                    }
                    catch (Exception x)
                    {
                        prebuildSuccess = false;
                        MetricsManager.LogException(nameof(SacrificeArtifacts), x, "game_mod_exception");
                    }
                    finally
                    {
                        sw.Stop();
                        UnityEngine.Debug.Log("..." + nameof(SacrificeArtifacts) + " pre-build took " +
                            sw.Elapsed.TotalSeconds.Things("second") + " | " +
                            (prebuildSuccess ? "success" : "failed"));
                    }
                }
            }

            Debug.Logger.Entry(Verbosity.Max,
                $"x {nameof(UD_SubStiltWorldBuilderExtension)}."
                + $"{nameof(SacrificeArtifacts)}("
                + $"{nameof(WorldID)}: {WorldID})"
                + $" *//",
                Indent: indent + 1, Toggle: doDebug);
            Debug.Logger.SetIndent(indent);
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

        public static List<string> GetCathedralStrataSubStrataZoneIDs(Zone Z)
        {
            if (Z == null)
            {
                return null;
            }
            List<string> cathedralStrata = new();

            int stratum = Z.Z;
            for (int i = 0; i < LowestWellStratum - 10; i++)
            {
                string stratumBelow = ZoneID.Assemble(Z.ZoneWorld, Z.wX, Z.wY, Z.X, Z.Y, ++stratum);
                if (stratumBelow != null
                    && !cathedralStrata.Contains(stratumBelow))
                {
                    cathedralStrata.Add(stratumBelow);
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
