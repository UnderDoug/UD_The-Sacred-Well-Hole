using System.Collections.Generic;
using System.Linq;

using HistoryKit;

using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;

using UD_SacredWellHole;

namespace XRL.World.ZoneBuilders
{
    public class UD_SubGrandCathedralScrapifier : ZoneBuilderSandbox
    {
        public bool WantScrappy;
        public UD_SubGrandCathedralScrapifier()
        {
            WantScrappy = false;
        }
        public UD_SubGrandCathedralScrapifier(bool? WantScrappy = null)
        {
            this.WantScrappy = WantScrappy ?? false;
        }

        public bool BuildZone(Zone Z, bool? WantScrappy = null)
        {
            this.WantScrappy = WantScrappy ?? this.WantScrappy;

            return BuildZone(Z);
        }
        public bool BuildZone(Zone Z)
        {
            zone = Z;

            UnityEngine.Debug.LogError($"{nameof(UD_SubGrandCathedralScrapifier)}, {nameof(Z)}: {Z.ZoneID}");
            UnityEngine.Debug.LogError($"    {nameof(WantScrappy)}: {WantScrappy}");

            if (WantScrappy)
            {
                foreach (Cell emptyCell in zone.GetEmptyCellsShuffled())
                {
                    if (!emptyCell.HasObject(GO => GO.GetBlueprint().InheritsFrom("Stairs")))
                    {
                        if (zone.Z > 17 && emptyCell.AnyAdjacentCell(c => c.HasObjectWithBlueprintEndsWith("ScrapWall")) 
                            && 3.in10())
                        {
                            if (8.in10())
                            {
                                emptyCell.AddObject("RandomScrapMound");
                            }
                            else
                            {
                                emptyCell.AddObject("RandomScrapWall");
                            }
                        }
                        else
                        {
                            if (Stat.RollCached("1d25") == 1)
                            {
                                emptyCell.AddObject("RandomScrapMound");
                            }
                            else
                            {
                                List<GameObject> populationObjects = Event.NewGameObjectList(emptyCell.AddPopulation("JunkOrProbablyGarbageOrNothing"));
                                foreach (GameObject item in populationObjects)
                                {
                                    ModificationFactory.ApplyModifications(item, item.GetBlueprint(), 100, Stat.RollCached("1d2"), "Creation");
                                }
                            }
                        }
                    }
                }
            }
            List<Cell> foodItemCells = Event.NewCellList();
            foreach (GameObject item in zone.GetObjectsThatInheritFrom("Item"))
            {
                item.ModIntProperty("Stilt Well Sacrifice", 1);
                if (item != null && item.Blueprint != "Garbage")
                {
                    UnityEngine.Debug.LogError($"    {nameof(item)}: {item?.DebugName ?? "\"null\""}");
                    if (item.GetBlueprint().InheritsFrom("Food") || item.Blueprint == "PersistentPapaya")
                    {
                        UnityEngine.Debug.LogError($"        Food item, replcaing with Garbage.");
                        foodItemCells.Add(item.CurrentCell);
                        continue;
                    }

                    if (StiltWell.GetArtifactReputationValue(item) < 1)
                    {
                        item.ReplaceWith("Garbage");
                        continue;
                    }

                    if (item.Physics != null)
                    {
                        item.Physics.Owner = "Mechanimists";
                    }

                    int high = 120;
                    // Damage, Rust, or Break most of the items in the well.
                    string randomSeed = $"{zone.ZoneID}-[{item.CurrentCell.Location}]-{item.ID}";
                    int roll = Stat.SeededRandom(randomSeed, 0, 7000) % high;
                    UnityEngine.Debug.LogError($"        {nameof(randomSeed)}: {randomSeed}");
                    UnityEngine.Debug.LogError($"        {nameof(roll)}: {roll}");

                    bool rusted = false;
                    bool busted = false;
                    if (roll == 0)
                    {
                        UnityEngine.Debug.LogError($"            Safe");
                        continue;
                    }
                    if (roll < (high/3))
                    {
                        rusted = item.ApplyEffect(new Rusted());
                        if (rusted)
                        {
                            UnityEngine.Debug.LogError($"        {nameof(Rusted)}");
                            continue;
                        }
                        UnityEngine.Debug.LogError($"        failed {nameof(Rusted)}");
                    }
                    if (roll < (high / 3)*2 || !rusted)
                    {
                        busted = item.ApplyEffect(new Broken());
                        if (busted)
                        {
                            UnityEngine.Debug.LogError($"        {nameof(Broken)}");
                            continue;
                        }
                        UnityEngine.Debug.LogError($"        failed {nameof(Broken)}");
                    }
                    if (roll < high || !busted)
                    {
                        Statistic hitpoints = item.GetStat("Hitpoints");
                        hitpoints.Penalty = hitpoints.BaseValue - 1;
                        UnityEngine.Debug.LogError($"        Damaged");
                    }
                }
            }
            UnityEngine.Debug.LogError($"    Deleting {foodItemCells.Count} food items from {nameof(foodItemCells)}");
            foreach (Cell foodItemCell in foodItemCells)
            {
                UnityEngine.Debug.LogError($"        {nameof(foodItemCell)}: [{foodItemCell.Location}] {foodItemCell.GetObjectsThatInheritFrom("Food")?.FirstOrDefault()?.DebugName ?? "\"null\""}");
                foodItemCell.Clear(alsoExclude: GO => !GO.GetBlueprint().InheritsFrom("Food")).AddObject("Garbage");
            }

            return true;
        }
    }
}
