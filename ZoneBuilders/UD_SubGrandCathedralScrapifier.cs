using System;
using System.Collections.Generic;

using Genkit;

using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.WorldBuilders;

using Verbosity = UD_Modding_Toolbox.UD_Logger.Verbosity;

using UD_Modding_Toolbox;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

using Debug = UD_SacredWellHole.Debug;

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
        public static int LowestWellStratum => UD_SubStiltWorldBuilderExtension.LowestWellStratum;

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
            Debug.Logger.ResetIndent(out int indent);
            Debug.Logger.Header(Verbosity.Max, $"{nameof(UD_SubGrandCathedralScrapifier)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);

            zone = Z;
            int currentStratum = zone.Z;

            Cell stiltWellCell = zone.GetCell(StiltWellLocation);

            Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(WantScrappy)}", $"{WantScrappy}", Good: WantScrappy, Indent: indent + 1, Toggle: getDoDebug());

            if (WantScrappy)
            {
                List<Cell> itemCells = Event.NewCellList();
                
                Debug.Logger.Entry(Verbosity.Max, $"Adding Items...", Indent: indent + 1, Toggle: getDoDebug());
                foreach (Cell itemCell in zone.GetCells(c => IsCellEmptyForThePurposeOfHavingScrapPlaced(c, StiltWellLocation)))
                {
                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(itemCell)}", $"({currentStratum})[{itemCell.Location}]",
                        Indent: indent + 2, Toggle: getDoDebug());

                    if (itemCell == stiltWellCell)
                    {
                        Debug.Logger.CheckNah(Verbosity.Max, $"{nameof(itemCell)} is {nameof(stiltWellCell)}",
                                Indent: indent + 3, Toggle: getDoDebug());
                        continue;
                    }

                    GameObject placedObject = null;
                    // Debug.Logger.CheckYeh(Verbosity.Max, $"{nameof(itemCell)} is acceptable to place items in", Indent: indent + 3, Toggle: getDoDebug());
                    if (zone.Z > 17
                        && itemCell.AnyAdjacentCell(c => c.HasObjectWithBlueprintEndsWith("ScrapWall"))
                        && 3.in10())
                    {
                        if (8.in10())
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, $"Placing", "RandomScrapMound", Indent: indent + 4, Toggle: getDoDebug());
                            placedObject = itemCell.AddObject("RandomScrapMound");
                        }
                        else
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, $"Placing", "RandomScrapWallSometimesGigantic", Indent: indent + 4, Toggle: getDoDebug());
                            placedObject = itemCell.AddObject("RandomScrapWallSometimesGigantic");
                        }
                    }
                    else
                    {
                        if (Stat.RollCached("1d25") == 1)
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, $"Placing", "RandomScrapMound", Indent: indent + 4, Toggle: getDoDebug());
                            placedObject = itemCell.AddObject("RandomScrapMound");
                        }
                        else
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, $"Placing", "JunkOrProbablyGarbageOrNothing", Indent: indent + 4, Toggle: getDoDebug());
                            string populationBlueprint = PopulationManager.GenerateOne("JunkOrProbablyGarbageOrNothing")?.Blueprint;
                            GameObject populationObject = null;
                            if (populationBlueprint != null)
                            {
                                placedObject = populationObject = itemCell.AddObject(populationBlueprint);
                            }
                            else
                            {
                                Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(populationObject)} rolled \"nothing\"", Indent: indent + 5, Toggle: getDoDebug());
                            }
                            if (populationObject != null)
                            {
                                Debug.Logger.LoopItem(Verbosity.Max, $"{populationObject?.DebugName ?? NULL}", Indent: indent + 5, Toggle: getDoDebug());
                                if (!populationObject.GetBlueprint().InheritsFrom("TradeGood") || Stat.RollCached("1d4") == 1)
                                {
                                    int setModNumber = (Stat.RollCached("1d4") == 1) ? 2 : 1;
                                    Debug.Logger.LoopItem(Verbosity.Max, $"Modding {populationObject?.DebugName ?? NULL}", $"{nameof(setModNumber)} ({setModNumber})",
                                        Indent: indent + 6, Toggle: getDoDebug());
                                    int appliedMods = ModificationFactory.ApplyModifications(
                                        GO: populationObject,
                                        Blueprint: populationObject.GetBlueprint(),
                                        BonusModChance: 100,
                                        SetModNumber: setModNumber,
                                        Context: "Creation");
                                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(appliedMods)}", $"{appliedMods}", Indent: indent + 6, Toggle: getDoDebug());
                                }
                                else
                                {
                                    Debug.Logger.LoopItem(Verbosity.Max, $"Modding {nameof(populationObject)}", $"Skipped", Indent: indent + 6, Toggle: getDoDebug());
                                }
                            }
                        }
                    }
                }
                Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
            }
            
            Debug.Logger.Entry(Verbosity.Max, $"Scrapping Items...", Indent: indent + 1, Toggle: getDoDebug());
            foreach (GameObject item in zone.GetObjectsThatInheritFrom("Item"))
            {
                Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                Debug.Logger.LoopItem(Verbosity.Max, $"(Z:{currentStratum}) {nameof(item)}", $"{item?.DebugName ?? NULL}",
                    Indent: indent + 2, Toggle: getDoDebug());

                if (item != null && item.Blueprint != "Garbage")
                {
                    bool itemIsFood = item.InheritsFrom("Food") || item.Blueprint == "PersistentPapaya";
                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(itemIsFood)}", $"{itemIsFood}",
                        Good: itemIsFood, Indent: indent + 3, Toggle: getDoDebug());

                    if (itemIsFood)
                    {
                        item.ReplaceWith(PopulationManager.GenerateOne("GrimyGarbage")?.Blueprint ?? "Garbage");
                        continue;
                    }

                    bool itemIsNotWorthySacrifice = StiltWell.GetArtifactReputationValue(item) < 1;
                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(itemIsNotWorthySacrifice)}", $"{itemIsNotWorthySacrifice}",
                        Good: itemIsNotWorthySacrifice, Indent: indent + 3, Toggle: getDoDebug());

                    if (itemIsNotWorthySacrifice
                        && !item.InheritsFrom("BaseDataDisk")
                        && !item.InheritsFrom("Scrap")
                        && !item.InheritsFrom("TradeGood")
                        && !item.InheritsFrom("BaseCyberneticsCreditWedge"))
                    {
                        if (item.CurrentCell.Location == stiltWellCell.Location)
                        {
                            Debug.Logger.CheckNah(Verbosity.Max, $"{nameof(item)}.{nameof(item.CurrentCell)} is {nameof(stiltWellCell)}",
                                Indent: indent + 4, Toggle: getDoDebug());
                        }
                        else
                        {
                            item.ReplaceWith(PopulationManager.GenerateOne("GrimyGarbage")?.Blueprint ?? "Garbage");
                            Debug.Logger.CheckYeh(Verbosity.Max, $"Replaced {nameof(item)} with {item?.DebugName ?? NULL}",
                                Indent: indent + 4, Toggle: getDoDebug());

                            continue;
                        }
                    }

                    item.ModIntProperty("Stilt Well Sacrifice", 1);

                    if (item.Physics != null)
                    {
                        item.Physics.Owner = "Mechanimists";
                        Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(item.Physics.Owner)}", $"{item.Physics.Owner}",
                            Good: item.Physics.Owner == "Mechanimists", Indent: indent + 3, Toggle: getDoDebug());
                    }

                    int high = 120;
                    // Damage, Rust, or Break most of the items in the well.
                    string randomSeed = $"{zone.ZoneID}-[{item.CurrentCell.Location}]-{item.ID}";
                    int roll = Stat.SeededRandom(randomSeed, 0, 7000) % high;

                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(randomSeed)}", $"{randomSeed}", Indent: indent + 3, Toggle: getDoDebug());
                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(roll)}", $"{roll}", Indent: indent + 3, Toggle: getDoDebug());

                    bool rusted = false;
                    bool busted = false;
                    if (roll == 0)
                    {
                        Debug.Logger.LoopItem(Verbosity.Max, $"Safe", Indent: indent + 4, Toggle: getDoDebug());
                        continue;
                    }
                    if (roll < (high / 3))
                    {
                        rusted = item.ApplyEffect(new Rusted());
                        if (rusted)
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(Rusted)}", Indent: indent + 4, Toggle: getDoDebug());
                            continue;
                        }
                        Debug.Logger.LoopItem(Verbosity.Max, $"failed {nameof(Rusted)}", Indent: indent + 4, Toggle: getDoDebug());
                    }
                    if (roll < (high / 3) * 2 || !rusted)
                    {
                        busted = item.ApplyEffect(new Broken());
                        if (busted)
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(Broken)}", Indent: indent + 4, Toggle: getDoDebug());
                            continue;
                        }
                        Debug.Logger.LoopItem(Verbosity.Max, $"failed {nameof(Broken)}", Indent: indent + 4, Toggle: getDoDebug());
                    }
                    if (roll < high || !busted)
                    {
                        Statistic hitpoints = item.GetStat("Hitpoints");
                        hitpoints.Penalty = hitpoints.BaseValue - 1;
                        Debug.Logger.LoopItem(Verbosity.Max, $"Damaged", Indent: indent + 4, Toggle: getDoDebug());
                    }
                }
            }
            Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());

            static bool IsScrapWallMound(GameObject GO)
            {
                return GO.InheritsFrom("BaseScrapWall")
                    || GO.InheritsFrom("BaseScrapMound");
            }
            Debug.Logger.Entry(Verbosity.Max, $"Cycling Scrap Walls/Mounds to add mods and make {nameof(Rusted)} or {nameof(Broken)}...",
                Indent: indent + 1, Toggle: getDoDebug());
            foreach (GameObject scrapWall in zone.GetObjects(IsScrapWallMound))
            {
                Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());
                Debug.Logger.LoopItem(Verbosity.Max, $"(Z:{currentStratum}) {nameof(scrapWall)}", $"{scrapWall?.DebugName ?? NULL}",
                    Indent: indent + 2, Toggle: getDoDebug());
                if (zone.Z == LowestWellStratum && scrapWall.CurrentCell == stiltWellCell)
                {
                    Debug.Logger.CheckNah(Verbosity.Max, $"{nameof(scrapWall)}.{nameof(scrapWall.CurrentCell)} is {nameof(stiltWellCell)}");
                    scrapWall.Obliterate();
                    continue;
                }
                if (!scrapWall.HasPart<ModGearbox>() && 3.in10() && scrapWall.ApplyModification(nameof(ModGearbox), Creation: true))
                {
                    Debug.Logger.LoopItem(Verbosity.Max, $"Added", $"{nameof(ModGearbox)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                if (!scrapWall.HasPart<ModWired>() && 4.in10() && scrapWall.ApplyModification(nameof(ModWired), Creation: true))
                {
                    Debug.Logger.LoopItem(Verbosity.Max, $"Added", $"{nameof(ModWired)}", Indent: indent + 3, Toggle: getDoDebug());
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
                        Debug.Logger.LoopItem(Verbosity.Max, $"Added", $"{nameof(ModPiping)}", Indent: indent + 3, Toggle: getDoDebug());
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
                        Debug.Logger.LoopItem(Verbosity.Max, $"Added", $"{nameof(EnergyCellSocket)}", Indent: indent + 3, Toggle: getDoDebug());
                    }
                }
                if (scrapWall.HasPart<ModWired>() && !scrapWall.HasPart<FusionReactor>() && 15.in100() && scrapWall.RequirePart<FusionReactor>() != null)
                {
                    scrapWall.RequirePart<DisplayNameAdjectives>().AddAdjective("powered");
                    Debug.Logger.LoopItem(Verbosity.Max, $"Added", $"{nameof(FusionReactor)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                if (6.in100() && scrapWall.ForceApplyEffect(new Rusted()))
                {
                    Debug.Logger.LoopItem(Verbosity.Max, $"Applied", $"{nameof(Rusted)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                else if (6.in10() && scrapWall.ForceApplyEffect(new Broken()))
                {
                    Debug.Logger.LoopItem(Verbosity.Max, $"Applied", $"{nameof(Broken)}", Indent: indent + 3, Toggle: getDoDebug());
                }
                if (scrapWall.HasPart<AnimatedObject>() && scrapWall.TryGetPart(out Leveler leveler))
                {
                    Debug.Logger.LoopItem(Verbosity.Max, $"Found", $"{nameof(AnimatedObject)}", Indent: indent + 3, Toggle: getDoDebug());
                    int scrapWallLevels = Stat.RollCached("2d6+3");
                    Debug.Logger.LoopItem(Verbosity.Max, $"Leveling up {scrapWallLevels} times...", Indent: indent + 4, Toggle: getDoDebug());
                    Debug.Logger.LoopItem(Verbosity.Max, $"-] {nameof(scrapWall)}.{nameof(scrapWall.Level)}", $"{scrapWall.Level}", Indent: indent + 5, Toggle: getDoDebug());
                    for (int i = 0; i < scrapWallLevels; i++)
                    {
                        leveler.LevelUp();
                        Debug.Logger.LoopItem(Verbosity.Max, $"{i}] {nameof(scrapWall)}.{nameof(scrapWall.Level)}", $"{scrapWall.Level}", Indent: indent + 5, Toggle: getDoDebug());
                    }
                    scrapWall.RandomlySpendPoints();
                    scrapWall.ReceivePopulation("HumanoidEquipment 3");
                }
            }
            Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: getDoDebug());

            Debug.Logger.Footer(Verbosity.Max, $"{nameof(UD_SubGrandCathedralScrapifier)}", $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);
            return true;
        }

        public static bool IsCellEmptyForThePurposeOfHavingScrapPlaced(Cell Cell, Location2D StillWellLocation)
        {
            Debug.Logger.GetIndent(out int indent);
            Verbosity verbosity = Verbosity.Critical;
            bool doDebug = false;

            Debug.Logger.Entry(verbosity, 
                nameof(IsCellEmptyForThePurposeOfHavingScrapPlaced), 
                "(" + Cell?.ParentZone?.Z + ") " + Cell?.Location?.ToString(),
                Indent: indent + 1, Toggle: doDebug);

            if (Cell.Location == StillWellLocation)
            {
                Debug.Logger.CheckNah(verbosity, nameof(StillWellLocation), Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasCombatObject())
            {
                Debug.Logger.CheckNah(verbosity, nameof(Cell.HasCombatObject), Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasObject(GO => GO.Physics != null && GO.Physics.Solid))
            {
                Debug.Logger.CheckNah(verbosity, nameof(GameObject.Physics.Solid), Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasObjectInheritsFrom("Wall"))
            {
                Debug.Logger.CheckNah(verbosity, "Wall", Indent: indent + 2, Toggle: doDebug);
                Debug.Logger.SetIndent(indent);
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
                        Debug.Logger.CheckNah(verbosity, nameof(Parts.StairsDown), Indent: indent + 2, Toggle: doDebug);
                        Debug.Logger.SetIndent(indent);
                        return false;
                    }
                }
            }
            Debug.Logger.CheckYeh(verbosity, "ye", Indent: indent + 2, Toggle: doDebug);
            Debug.Logger.SetIndent(indent);
            return true;
        }
    }
}
