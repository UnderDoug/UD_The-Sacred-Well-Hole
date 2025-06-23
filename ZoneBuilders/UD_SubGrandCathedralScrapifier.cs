using Genkit;
using HistoryKit;
using System;
using System.Collections.Generic;
using System.Linq;
using UD_SacredWellHole;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.WorldBuilders;
using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

namespace XRL.World.ZoneBuilders
{
    public class UD_SubGrandCathedralScrapifier : ZoneBuilderSandbox
    {
        private static bool doDebug => getClassDoDebug(nameof(UD_SubGrandCathedralScrapifier));
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

        public Location2D StiltWellLocation => UD_SubStiltWorldBuilderExtension.StiltWellLocation;
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
            Debug.Header(4, $"{nameof(UD_SubGrandCathedralScrapifier)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            int indent = Debug.LastIndent;

            zone = Z;

            Cell stiltWellCell = zone.GetCell(StiltWellLocation);

            Debug.LoopItem(4, $"{nameof(WantScrappy)}", $"{WantScrappy}", Good: WantScrappy, Indent: indent + 1, Toggle: getDoDebug());

            if (WantScrappy)
            {
                List<Cell> emptyCells = Event.NewCellList(zone.GetEmptyCells(c => !c.HasObject(GO => GO.GetBlueprint().InheritsFrom("Stairs"))));
                if (!emptyCells.IsNullOrEmpty())
                {
                    foreach (Cell emptyCell in emptyCells)
                    {
                        Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                        Debug.LoopItem(4, $"{nameof(emptyCell)}", $"{emptyCell.Location}", Indent: indent + 2, Toggle: getDoDebug());
                        if (emptyCell.Location == stiltWellCell.Location)
                        {
                            Debug.CheckYeh(4, $"{nameof(emptyCell)} is {nameof(stiltWellCell)}", Indent: indent + 3, Toggle: getDoDebug());
                            continue;
                        }
                        if (!emptyCell.HasObject(GO => GO.GetBlueprint().InheritsFrom("Stairs")))
                        {
                            Debug.CheckYeh(4, $"{nameof(emptyCell)} has no Stairs derivative", Indent: indent + 3, Toggle: getDoDebug());
                            if (zone.Z > 17 && emptyCell.AnyAdjacentCell(c => c.HasObjectWithBlueprintEndsWith("ScrapWall"))
                                && 3.in10())
                            {
                                if (8.in10())
                                {
                                    Debug.LoopItem(4, $"Placing", "RandomScrapMound", Indent: indent + 4, Toggle: getDoDebug());
                                    emptyCell.AddObject("RandomScrapMound");
                                }
                                else
                                {
                                    Debug.LoopItem(4, $"Placing", "RandomScrapWall", Indent: indent + 4, Toggle: getDoDebug());
                                    emptyCell.AddObject("RandomScrapWall");
                                }
                            }
                            else
                            {
                                if (Stat.RollCached("1d25") == 1)
                                {
                                    Debug.LoopItem(4, $"Placing", "RandomScrapMound", Indent: indent + 4, Toggle: getDoDebug());
                                    emptyCell.AddObject("RandomScrapMound");
                                }
                                else
                                {
                                    Debug.LoopItem(4, $"Placing", "JunkOrProbablyGarbageOrNothing", Indent: indent + 4, Toggle: getDoDebug());
                                    string populationBlueprint = PopulationManager.GenerateOne("JunkOrProbablyGarbageOrNothing")?.Blueprint;
                                    GameObject populationObject = null;
                                    if (populationBlueprint != null)
                                    {
                                        populationObject = emptyCell.AddObject(populationBlueprint);
                                    }
                                    else
                                    {
                                        Debug.LoopItem(4, $"{nameof(populationObject)} rolled \"nothing\"", Indent: indent + 5, Toggle: getDoDebug());
                                    }
                                    if (populationObject != null)
                                    {
                                        Debug.LoopItem(4, $"{populationObject?.DebugName ?? NULL}", Indent: indent + 5, Toggle: getDoDebug());
                                        if (!populationObject.GetBlueprint().InheritsFrom("TradeGood") || Stat.RollCached("1d4") == 1)
                                        {
                                            int setModNumber = (Stat.RollCached("1d4") == 1 ? 2 : 1);
                                            Debug.LoopItem(4, $"Modding {populationObject?.DebugName ?? NULL}", $"{nameof(setModNumber)} ({setModNumber})", 
                                                Indent: indent + 6, Toggle: getDoDebug());
                                            int appliedMods = ModificationFactory.ApplyModifications(
                                                GO: populationObject,
                                                Blueprint: populationObject.GetBlueprint(),
                                                BonusModChance: 100,
                                                SetModNumber: setModNumber,
                                                Context: "Creation");
                                            Debug.LoopItem(4, $"{nameof(appliedMods)}", $"{appliedMods}", Indent: indent + 6, Toggle: getDoDebug());
                                        }
                                        else
                                        {
                                            Debug.LoopItem(4, $"Modding {nameof(populationObject)}", $"Skipped", Indent: indent + 6, Toggle: getDoDebug());
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.CheckNah(4, $"{nameof(emptyCell)} contains Stairs derivative", Indent: indent + 3, Toggle: getDoDebug());
                        }
                    }
                    Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                }
            }
            List<GameObject> itemsList = Event.NewGameObjectList(zone.GetObjectsThatInheritFrom("Item"));
            if (!itemsList.IsNullOrEmpty())
            {
                foreach (GameObject item in itemsList)
                {
                    Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                    Debug.LoopItem(4, $"{nameof(item)}", $"{item?.DebugName ?? NULL}", Indent: indent + 2, Toggle: getDoDebug());
                    if (item != null && item.Blueprint != "Garbage" && !item.GetBlueprint().InheritsFrom("BaseDataDisk") && !item.GetBlueprint().InheritsFrom("Scrap") && !item.GetBlueprint().InheritsFrom("TradeGood"))
                    {
                        bool itemIsFood = item.GetBlueprint().InheritsFrom("Food") || item.Blueprint == "PersistentPapaya";
                        Debug.LoopItem(4, $"{nameof(itemIsFood)}", $"{itemIsFood}", 
                            Good: itemIsFood, Indent: indent + 3, Toggle: getDoDebug());

                        if (itemIsFood)
                        {
                            item.ReplaceWith(PopulationManager.GenerateOne("GrimyGarbage")?.Blueprint ?? "Garbage");
                            continue;
                        }

                        bool itemIsNotWorthySacrifice = StiltWell.GetArtifactReputationValue(item) < 1;
                        Debug.LoopItem(4, $"{nameof(itemIsNotWorthySacrifice)}", $"{itemIsNotWorthySacrifice}", 
                            Good: itemIsNotWorthySacrifice, Indent: indent + 3, Toggle: getDoDebug());

                        if (itemIsNotWorthySacrifice)
                        {
                            if (item.CurrentCell.Location == stiltWellCell.Location)
                            {
                                Debug.CheckNah(4, $"{nameof(item)}.{nameof(item.CurrentCell)} is {nameof(stiltWellCell)}", Indent: indent + 4, Toggle: getDoDebug());
                            }
                            else
                            {
                                item.ReplaceWith(PopulationManager.GenerateOne("GrimyGarbage")?.Blueprint ?? "Garbage");
                                Debug.CheckYeh(4, $"Replaced {nameof(item)} with {item?.DebugName ?? NULL}", Indent: indent + 4, Toggle: getDoDebug());
                                continue;
                            }
                        }

                        item.ModIntProperty("Stilt Well Sacrifice", 1);

                        if (item.Physics != null)
                        {
                            item.Physics.Owner = "Mechanimists";
                            Debug.LoopItem(4, $"{nameof(item.Physics.Owner)}", $"{item.Physics.Owner}",
                                Good: item.Physics.Owner == "Mechanimists", Indent: indent + 3, Toggle: getDoDebug());
                        }

                        int high = 120;
                        // Damage, Rust, or Break most of the items in the well.
                        string randomSeed = $"{zone.ZoneID}-[{item.CurrentCell.Location}]-{item.ID}";
                        int roll = Stat.SeededRandom(randomSeed, 0, 7000) % high;

                        Debug.LoopItem(4, $"{nameof(randomSeed)}", $"{randomSeed}", Indent: indent + 3, Toggle: getDoDebug());
                        Debug.LoopItem(4, $"{nameof(roll)}", $"{roll}", Indent: indent + 3, Toggle: getDoDebug());

                        bool rusted = false;
                        bool busted = false;
                        if (roll == 0)
                        {
                            Debug.LoopItem(4, $"Safe", Indent: indent + 4, Toggle: getDoDebug());
                            continue;
                        }
                        if (roll < (high / 3))
                        {
                            rusted = item.ApplyEffect(new Rusted());
                            if (rusted)
                            {
                                Debug.LoopItem(4, $"{nameof(Rusted)}", Indent: indent + 4, Toggle: getDoDebug());
                                continue;
                            }
                            Debug.LoopItem(4, $"failed {nameof(Rusted)}", Indent: indent + 4, Toggle: getDoDebug());
                        }
                        if (roll < (high / 3) * 2 || !rusted)
                        {
                            busted = item.ApplyEffect(new Broken());
                            if (busted)
                            {
                                Debug.LoopItem(4, $"{nameof(Broken)}", Indent: indent + 4, Toggle: getDoDebug());
                                continue;
                            }
                            Debug.LoopItem(4, $"failed {nameof(Broken)}", Indent: indent + 4, Toggle: getDoDebug());
                        }
                        if (roll < high || !busted)
                        {
                            Statistic hitpoints = item.GetStat("Hitpoints");
                            hitpoints.Penalty = hitpoints.BaseValue - 1;
                            Debug.LoopItem(4, $"Damaged", Indent: indent + 4, Toggle: getDoDebug());
                        }
                    }
                }
                Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
            }

            static bool IsScrapWallMound(GameObject GO)
            {
                return GO.InheritsFrom("BaseScrapWall") || GO.InheritsFrom("BaseScrapMound");
            }
            List<GameObject> scrapWallMoundList = Event.NewGameObjectList(zone.GetObjects(GO => IsScrapWallMound(GO)));
            if (!scrapWallMoundList.IsNullOrEmpty())
            {
                foreach (GameObject scrapWall in scrapWallMoundList)
                {
                    if (!scrapWall.HasPart<ModGearbox>() && 3.in10())
                    {
                        scrapWall.ApplyModification(nameof(ModGearbox), Creation: true);
                    }
                    if (!scrapWall.HasPart<ModWired>() && 3.in10())
                    {
                        scrapWall.ApplyModification(nameof(ModWired), Creation: true);
                        if (1.in10())
                        {
                            EnergyCellSocket energyCellSocket = scrapWall.RequirePart<EnergyCellSocket>();
                            GameObject energyCellObject = GameObjectFactory.Factory.CreateObject(PopulationManager.GenerateOne("MiddleWeightedRandomEnergyCell")?.Blueprint);
                            if (energyCellObject != null)
                            {
                                EnergyCell energyCell = energyCellObject.GetPart<EnergyCell>();
                                if (energyCell != null)
                                {
                                    energyCell.Charge = Math.Min(energyCell.Charge * Stat.RollCached("1d100"), energyCell.MaxCharge);
                                    energyCellSocket.Cell = energyCellObject;
                                }
                            }
                        }
                    }
                    if (!scrapWall.HasPart<ModPiping>() && 7.in10())
                    {
                        string corpseBlueprintString = scrapWall.GetPart<Corpse>().CorpseBlueprint;
                        GameObjectBlueprint corpseBlueprint = GameObjectFactory.Factory.GetBlueprintIfExists(corpseBlueprintString);
                        string corpseLiquid = corpseBlueprint.GetPartParameter(nameof(LiquidVolume), "InitialLiquid", "oil-600,gel-300,sludge-100");
                        ModPiping modPiping = new()
                        {
                            Liquid = corpseLiquid
                        };
                        scrapWall.ApplyModification(modPiping, Creation: true);
                        if (15.in100())
                        {
                            scrapWall.ApplyEffect(new Broken());
                        }
                    }
                    if (scrapWall.HasPart<ModWired>() && !scrapWall.HasPart<FusionReactor>() && 15.in100())
                    {
                        scrapWall.AddPart<FusionReactor>();
                    }
                    if (15.in100())
                    {
                        scrapWall.ApplyEffect(new Rusted());
                    }
                }
            }

            Debug.Footer(4, $"{nameof(UD_SubGrandCathedralScrapifier)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            return true;
        }
    }
}
