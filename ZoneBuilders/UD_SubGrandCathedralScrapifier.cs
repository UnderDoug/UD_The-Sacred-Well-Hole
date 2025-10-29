using System;
using System.Collections.Generic;

using Genkit;

using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.WorldBuilders;

using UD_SacredWellHole;

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
            int currentStratum = zone.Z;

            Cell stiltWellCell = zone.GetCell(StiltWellLocation);

            Debug.LoopItem(4, $"{nameof(WantScrappy)}", $"{WantScrappy}", Good: WantScrappy, Indent: indent + 1, Toggle: getDoDebug());

            if (WantScrappy)
            {
                List<Cell> itemCells = Event.NewCellList();
                
                Debug.Entry(4, $"Adding Items...", Indent: indent + 1, Toggle: getDoDebug());
                foreach (Cell itemCell in zone.GetCells(c => IsCellEmptyForThePurposeOfHavingScrapPlaced(c, StiltWellLocation)))
                {
                    Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                    Debug.LoopItem(4, $"{nameof(itemCell)}", $"({currentStratum})[{itemCell.Location}]",
                        Indent: indent + 2, Toggle: getDoDebug());

                    if (itemCell == stiltWellCell)
                    {
                        Debug.CheckNah(4, $"{nameof(itemCell)} is {nameof(stiltWellCell)}",
                                Indent: indent + 3, Toggle: getDoDebug());
                        continue;
                    }

                    GameObject placedObject = null;
                    // Debug.CheckYeh(4, $"{nameof(itemCell)} is acceptable to place items in", Indent: indent + 3, Toggle: getDoDebug());
                    if (zone.Z > 17
                        && itemCell.AnyAdjacentCell(c => c.HasObjectWithBlueprintEndsWith("ScrapWall"))
                        && 3.in10())
                    {
                        if (8.in10())
                        {
                            Debug.LoopItem(4, $"Placing", "RandomScrapMound", Indent: indent + 4, Toggle: getDoDebug());
                            placedObject = itemCell.AddObject("RandomScrapMound");
                        }
                        else
                        {
                            Debug.LoopItem(4, $"Placing", "RandomScrapWallSometimesGigantic", Indent: indent + 4, Toggle: getDoDebug());
                            placedObject = itemCell.AddObject("RandomScrapWallSometimesGigantic");
                        }
                    }
                    else
                    {
                        if (Stat.RollCached("1d25") == 1)
                        {
                            Debug.LoopItem(4, $"Placing", "RandomScrapMound", Indent: indent + 4, Toggle: getDoDebug());
                            placedObject = itemCell.AddObject("RandomScrapMound");
                        }
                        else
                        {
                            Debug.LoopItem(4, $"Placing", "JunkOrProbablyGarbageOrNothing", Indent: indent + 4, Toggle: getDoDebug());
                            string populationBlueprint = PopulationManager.GenerateOne("JunkOrProbablyGarbageOrNothing")?.Blueprint;
                            GameObject populationObject = null;
                            if (populationBlueprint != null)
                            {
                                placedObject = populationObject = itemCell.AddObject(populationBlueprint);
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
                                    int setModNumber = (Stat.RollCached("1d4") == 1) ? 2 : 1;
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
                Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
            }
            
            Debug.Entry(4, $"Scrapping Items...", Indent: indent + 1, Toggle: getDoDebug());
            foreach (GameObject item in zone.GetObjectsThatInheritFrom("Item"))
            {
                Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                Debug.LoopItem(4, $"(Z:{currentStratum}) {nameof(item)}", $"{item?.DebugName ?? NULL}",
                    Indent: indent + 2, Toggle: getDoDebug());

                if (item != null && item.Blueprint != "Garbage")
                {
                    bool itemIsFood = item.InheritsFrom("Food") || item.Blueprint == "PersistentPapaya";
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

                    if (itemIsNotWorthySacrifice
                        && !item.InheritsFrom("BaseDataDisk")
                        && !item.InheritsFrom("Scrap")
                        && !item.InheritsFrom("TradeGood")
                        && !item.InheritsFrom("BaseCyberneticsCreditWedge"))
                    {
                        if (item.CurrentCell.Location == stiltWellCell.Location)
                        {
                            Debug.CheckNah(4, $"{nameof(item)}.{nameof(item.CurrentCell)} is {nameof(stiltWellCell)}",
                                Indent: indent + 4, Toggle: getDoDebug());
                        }
                        else
                        {
                            item.ReplaceWith(PopulationManager.GenerateOne("GrimyGarbage")?.Blueprint ?? "Garbage");
                            Debug.CheckYeh(4, $"Replaced {nameof(item)} with {item?.DebugName ?? NULL}",
                                Indent: indent + 4, Toggle: getDoDebug());

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

            static bool IsScrapWallMound(GameObject GO)
            {
                return GO.InheritsFrom("BaseScrapWall")
                    || GO.InheritsFrom("BaseScrapMound");
            }
            Debug.Entry(4, $"Cycling Scrap Walls/Mounds to add mods and make {nameof(Rusted)} or {nameof(Broken)}...",
                Indent: indent + 1, Toggle: getDoDebug());
            foreach (GameObject scrapWall in zone.GetObjects(IsScrapWallMound))
            {
                Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                Debug.LoopItem(4, $"(Z: {currentStratum}) {nameof(scrapWall)}", $"{scrapWall?.DebugName ?? NULL}",
                    Indent: indent + 2, Toggle: getDoDebug());

                if (!scrapWall.HasPart<ModGearbox>() && 3.in10() && scrapWall.ApplyModification(nameof(ModGearbox), Creation: true))
                {
                    Debug.LoopItem(4, $"Added", $"{nameof(ModGearbox)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                if (!scrapWall.HasPart<ModWired>() && 4.in10() && scrapWall.ApplyModification(nameof(ModWired), Creation: true))
                {
                    Debug.LoopItem(4, $"Added", $"{nameof(ModWired)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                if (!scrapWall.HasPart<ModPiping>() && 6.in10())
                {
                    string corpseBlueprintString = scrapWall.GetPart<Corpse>().CorpseBlueprint;
                    GameObjectBlueprint corpseBlueprint = GameObjectFactory.Factory.GetBlueprintIfExists(corpseBlueprintString);
                    string corpseLiquid = corpseBlueprint.GetPartParameter(nameof(LiquidVolume), "InitialLiquid", "oil-600,gel-300,sludge-100");
                    ModPiping modPiping = new()
                    {
                        Liquid = corpseLiquid
                    };
                    if (scrapWall.ApplyModification(modPiping, Creation: true))
                    {
                        Debug.LoopItem(4, $"Added", $"{nameof(ModPiping)}", Indent: indent + 3, Toggle: getDoDebug());
                    }
                }
                if (scrapWall.HasPart<ModWired>() && 4.in10())
                {
                    EnergyCellSocket energyCellSocket = scrapWall.RequirePart<EnergyCellSocket>();
                    GameObject energyCellObject = GameObjectFactory.Factory.CreateObject(PopulationManager.GenerateOne("MiddleWeightedRandomEnergyCell")?.Blueprint);
                    if (energyCellSocket != null && energyCellObject != null)
                    {
                        EnergyCell energyCell = energyCellObject.GetPart<EnergyCell>();
                        if (energyCell != null)
                        {
                            energyCell.Charge = (int)Math.Min(energyCell.Charge * (float)(Stat.RollCached("1d100") / 100), energyCell.MaxCharge);
                            energyCellSocket.Cell = energyCellObject;
                        }
                        Debug.LoopItem(4, $"Added", $"{nameof(EnergyCellSocket)}", Indent: indent + 3, Toggle: getDoDebug());
                    }
                }
                if (scrapWall.HasPart<ModWired>() && !scrapWall.HasPart<FusionReactor>() && 15.in100() && scrapWall.RequirePart<FusionReactor>() != null)
                {
                    scrapWall.RequirePart<DisplayNameAdjectives>().AddAdjective("powered");
                    Debug.LoopItem(4, $"Added", $"{nameof(FusionReactor)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                if (6.in100() && scrapWall.ForceApplyEffect(new Rusted()))
                {
                    Debug.LoopItem(4, $"Applied", $"{nameof(Rusted)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                else if (6.in10() && scrapWall.ForceApplyEffect(new Broken()))
                {
                    Debug.LoopItem(4, $"Applied", $"{nameof(Broken)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                if (scrapWall.HasPart<AnimatedObject>() && scrapWall.TryGetPart(out Leveler leveler))
                {
                    Debug.LoopItem(4, $"Found", $"{nameof(AnimatedObject)}", Indent: indent + 3, Toggle: getDoDebug());
                    int scrapWallLevels = Stat.RollCached("2d6+3");
                    Debug.LoopItem(4, $"Leveling up {scrapWallLevels} times...", Indent: indent + 4, Toggle: getDoDebug());
                    Debug.LoopItem(4, $"-] {nameof(scrapWall)}.{nameof(scrapWall.Level)}", $"{scrapWall.Level}", Indent: indent + 5, Toggle: getDoDebug());
                    for (int i = 0; i < scrapWallLevels; i++)
                    {
                        leveler.LevelUp();
                        Debug.LoopItem(4, $"{i}] {nameof(scrapWall)}.{nameof(scrapWall.Level)}", $"{scrapWall.Level}", Indent: indent + 5, Toggle: getDoDebug());
                    }
                    scrapWall.RandomlySpendPoints();
                    scrapWall.ReceivePopulation("HumanoidEquipment 3");
                }
            }
            Debug.Divider(4, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());

            Debug.Footer(4, $"{nameof(UD_SubGrandCathedralScrapifier)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            return true;
        }

        public static bool IsCellEmptyForThePurposeOfHavingScrapPlaced(Cell Cell, Location2D StillWellLocation)
        {
            int indent = Debug.LastIndent;
            int verbosity = 0;
            bool doDebug = false;

            Debug.Entry(verbosity, 
                nameof(IsCellEmptyForThePurposeOfHavingScrapPlaced), 
                "(" + Cell?.ParentZone?.Z + ") " + Cell?.Location?.ToString(),
                Indent: indent + 1, Toggle: doDebug);

            if (Cell.Location == StillWellLocation)
            {
                Debug.CheckNah(verbosity, nameof(StillWellLocation), Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }
            if (Cell.HasCombatObject())
            {
                Debug.CheckNah(verbosity, nameof(Cell.HasCombatObject), Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }
            if (Cell.HasObject(GO => GO.Physics != null && GO.Physics.Solid))
            {
                Debug.CheckNah(verbosity, nameof(GameObject.Physics.Solid), Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }
            if (Cell.HasObjectInheritsFrom("Wall"))
            {
                Debug.CheckNah(verbosity, "Wall", Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }
            if (Cell.HasObjectWithPart(nameof(Parts.StairsDown)))
            {
                foreach (GameObject stairObject in Cell.GetObjectsWithPart(nameof(Parts.StairsDown)))
                {
                    if (stairObject.TryGetPart(out QuantumAir quantumAir)
                        && !quantumAir.ShouldBeAir()
                        && quantumAir.SolidifyAir())
                    {
                        continue;
                    }
                    if (stairObject.TryGetPart(out Parts.StairsDown stairsDown)
                        && stairsDown.PullDown
                        && !stairObject.HasPart<SolidAir>())
                    {
                        Debug.CheckNah(verbosity, nameof(Parts.StairsDown), Indent: indent + 2, Toggle: doDebug);
                        Debug.LastIndent = indent;
                        return false;
                    }
                }
            }
            /*
            if (Cell.HasObjectWithPart(nameof(QuantumAir))
                && Cell.GetFirstObjectWithPart(nameof(QuantumAir)) is GameObject quantumAirObject
                && quantumAirObject.TryGetPart(out QuantumAir quantumAir))
            {
                Debug.CheckNah(verbosity, nameof(QuantumAir), Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }
            */
            Debug.CheckYeh(verbosity, "ye", Indent: indent + 2, Toggle: doDebug);
            Debug.LastIndent = indent;
            return true;
        }
    }
}
