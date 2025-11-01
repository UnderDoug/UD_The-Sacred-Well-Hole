using System;
using System.Collections.Generic;

using Genkit;

using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.WorldBuilders;
using XRL.World.Tinkering;

using static XRL.World.ZoneBuilders.UD_SubGrandCathedralBuilder;

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

        public enum Disrepair : int
        {
            Fine = 0,
            Scuff = 1,
            Rust = 2,
            Bust = 3,
        }

        public static Raffle<Disrepair> DisrepairBag => new()
        {
            { Disrepair.Fine, 1 },
            { Disrepair.Scuff, 30 },
            { Disrepair.Rust, 30 },
            { Disrepair.Bust, 30 },
        };

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

            Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(WantScrappy)}", $"{WantScrappy}",
                Good: WantScrappy, Indent: indent + 1, Toggle: getDoDebug());

            if (WantScrappy)
            {
                bool doDebug = getDoDebug(nameof(WantScrappy));
                
                Debug.Logger.Entry(Verbosity.Max, $"Adding Items...", Indent: indent + 1, Toggle: doDebug);
                foreach (Cell emptyCell in zone.GetCells(c => IsCellEmptyForThePurposeOfHavingScrapPlaced(c, StiltWellLocation)))
                {
                    /*
                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebug);
                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(emptyCell)}", $"({currentStratum})[{emptyCell.Location}]",
                        Indent: indent + 2, Toggle: doDebug);
                    */
                    if (emptyCell == stiltWellCell)
                    {
                        Debug.Logger.CheckNah(Verbosity.Max, $"{nameof(emptyCell)} is {nameof(stiltWellCell)}",
                                Indent: indent + 3, Toggle: doDebug);
                        continue;
                    }

                    string randomScrapMoundBlueprint = "RandomScrapMound";
                    string randomScrapWallBlueprint = "RandomScrapWallOrUnlikelyScrapCorpseSometimesGigantic";
                    string sacrificialJunkTable = "JunkOrGarbageButProbablyGarbage";

                    GameObject placedObject = null;
                    string placementContext = "nothing";
                    if (currentStratum > 19
                        && emptyCell.AnyAdjacentCell(c => c.HasObjectWithBlueprintEndsWith("ScrapWall"))
                        && 3.in10())
                    {
                        placedObject = PlaceRandomScrapWallMoundInCell(
                            Cell: emptyCell,
                            ChanceScrapMound: 80,
                            RandomScrapMoundBlueprint: randomScrapMoundBlueprint,
                            RandomScrapWallBlueprint: randomScrapWallBlueprint,
                            doDebugOverride: false);
                        placementContext = nameof(PlaceRandomScrapWallMoundInCell);
                    }
                    else
                    {
                        if (Stat.RollCached("1d25") == 1
                            && GameObject.CreateUnmodified(randomScrapMoundBlueprint) is GameObject randomScrapMound)
                        {
                            placedObject = randomScrapMound;
                            emptyCell.AddObject(placedObject);
                            placementContext = randomScrapMoundBlueprint;
                        }
                        else
                        {
                            if (PopulationManager.CreateOneFrom(
                                PopulationName: sacrificialJunkTable,
                                BonusModChance: 50,
                                SetModNumber: 2) is GameObject populationObject)
                            {
                                placedObject = populationObject;
                                emptyCell.AddObject(placedObject);

                                placementContext = sacrificialJunkTable;
                            }
                        }
                    }
                    Debug.Logger.LoopItem(Verbosity.Max,
                        "(" + currentStratum + ")[" + emptyCell.Location +"] " + 
                        "Placing " + placementContext, placedObject?.DebugName ?? NULL,
                        Indent: indent + 2, Toggle: doDebug);
                }
                Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebug);
            }

            bool doDebugScrapping = getDoDebug("Scrapping");
            Debug.Logger.Entry(Verbosity.Max, $"Scrapping Items...", Indent: indent + 1, Toggle: doDebugScrapping);
            foreach (GameObject item in zone.GetObjectsThatInheritFrom("Item"))
            {
                Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugScrapping);
                Debug.Logger.LoopItem(Verbosity.Max, 
                    $"(Z:{currentStratum})[{item?.CurrentCell?.DebugName}] {nameof(item)}", $"{item?.DebugName ?? NULL}",
                    Indent: indent + 2, Toggle: doDebugScrapping);

                if (item?.Blueprint != "Garbage")
                {
                    bool itemIsFood = item.InheritsFrom("Food") || item.Blueprint == "PersistentPapaya";

                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(itemIsFood)}", $"{itemIsFood}",
                        Good: itemIsFood, Indent: indent + 3, Toggle: doDebugScrapping);

                    if (itemIsFood)
                    {
                        item.ReplaceWith(PopulationManager.GenerateOne("GrimyGarbage")?.Blueprint ?? "Garbage");
                        continue;
                    }

                    bool isItemWorthySacrifice = StiltWell.GetArtifactReputationValue(item) > 0;

                    Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(isItemWorthySacrifice)}", $"{isItemWorthySacrifice}",
                        Good: isItemWorthySacrifice, Indent: indent + 3, Toggle: doDebugScrapping);

                    if (!isItemWorthySacrifice
                        && !item.InheritsFrom("BaseDataDisk")
                        && !item.InheritsFrom("Scrap")
                        && !item.InheritsFrom("TradeGood")
                        && !item.InheritsFrom("BaseCurrency")
                        && !item.InheritsFrom("BaseCyberneticsCreditWedge")
                        && !item.HasPart("SSR_BoosterPack"))
                    {
                        if (item.CurrentCell.Location == stiltWellCell.Location)
                        {
                            Debug.Logger.CheckNah(Verbosity.Max, $"{nameof(item)}.{nameof(item.CurrentCell)} is {nameof(stiltWellCell)}",
                                Indent: indent + 4, Toggle: doDebugScrapping);
                        }
                        else
                        {
                            string newBlueprint = PopulationManager.GenerateOne("GrimyGarbage")?.Blueprint ?? "Garbage";
                            GameObject newItem = item.ReplaceWith(newBlueprint);
                            Debug.Logger.CheckYeh(Verbosity.Max, $"Replaced {nameof(item)} with {newItem?.DebugName ?? NULL}",
                                Indent: indent + 4, Toggle: doDebugScrapping);

                            continue;
                        }
                    }

                    if (isItemWorthySacrifice)
                    {
                        item.ModIntProperty("Stilt Well Sacrifice", 1);
                    }

                    if (item.Physics != null)
                    {
                        item.Physics.Owner = "Mechanimists";
                        Debug.Logger.LoopItem(Verbosity.Max, $"{nameof(item.Physics.Owner)}", $"{item.Physics.Owner}",
                            Good: item.Physics.Owner == "Mechanimists", Indent: indent + 3, Toggle: doDebugScrapping);
                    }

                    // Damage, Rust, or Break most of the items in the well.

                    Disrepair disrepairTicket = DisrepairBag.SampleCosmetic();

                    Debug.Logger.LoopItem(Verbosity.Max, nameof(disrepairTicket), (int)disrepairTicket + ":" + disrepairTicket,
                        Indent: indent + 3, Toggle: doDebugScrapping);

                    bool rusted = false;
                    bool busted = false;
                    if (disrepairTicket == Disrepair.Fine)
                    {
                        Debug.Logger.LoopItem(Verbosity.Max, Disrepair.Fine.ToString(),
                            Indent: indent + 4, Toggle: doDebugScrapping);

                        continue;
                    }
                    if (disrepairTicket == Disrepair.Rust)
                    {
                        if (rusted = item.ApplyEffect(new Rusted()))
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, Disrepair.Rust.ToString(),
                                Indent: indent + 4, Toggle: doDebugScrapping);
                            continue;
                        }
                        Debug.Logger.LoopItem(Verbosity.Max, "failed " + Disrepair.Rust.ToString(),
                            Indent: indent + 4, Toggle: doDebugScrapping);
                    }
                    if (disrepairTicket == Disrepair.Bust || !rusted)
                    {
                        if (busted = item.ApplyEffect(new Broken()))
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, Disrepair.Bust.ToString(),
                                Indent: indent + 4, Toggle: doDebugScrapping);
                            continue;
                        }
                        Debug.Logger.LoopItem(Verbosity.Max, "failed " + Disrepair.Bust.ToString(),
                            Indent: indent + 4, Toggle: doDebugScrapping);
                    }
                    if (disrepairTicket == Disrepair.Scuff || !busted)
                    {
                        if (item.GetStat("Hitpoints") is Statistic hitpoints)
                        {
                            hitpoints.Penalty = hitpoints.BaseValue - 1;
                            Debug.Logger.LoopItem(Verbosity.Max, Disrepair.Scuff.ToString(),
                                Indent: indent + 4, Toggle: doDebugScrapping);
                        }
                        else
                        {
                            Debug.Logger.LoopItem(Verbosity.Max, "failed " + Disrepair.Scuff.ToString(),
                                Indent: indent + 4, Toggle: doDebugScrapping);
                        }
                    }
                }
            }
            Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugScrapping);

            static bool IsScrapWallMound(GameObject GO)
            {
                return GO.InheritsFrom("BaseScrapWall")
                    || GO.InheritsFrom("BaseScrapMound");
            }

            bool doDebugScrapWalls = getDoDebug("ScrapWalls");
            Debug.Logger.Entry(Verbosity.Max,
                $"Cycling Scrap Walls/Mounds to add mods and make {nameof(Rusted)} or {nameof(Broken)}...",
                Indent: indent + 1, Toggle: doDebugScrapWalls);

            foreach (GameObject scrapWallMound in zone.GetObjects(IsScrapWallMound))
            {
                if (currentStratum == LowestWellStratum
                    && scrapWallMound.CurrentCell == stiltWellCell)
                {
                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugScrapWalls);
                    Debug.Logger.LoopItem(Verbosity.Max,
                        $"(Z:{currentStratum})[{scrapWallMound.CurrentCell.DebugName}] {nameof(scrapWallMound)}",
                        $"{scrapWallMound?.DebugName ?? NULL}",
                        Indent: indent + 2, Toggle: doDebugScrapWalls);

                    Debug.Logger.CheckNah(Verbosity.Max,
                        $"{nameof(scrapWallMound)}.{nameof(scrapWallMound.CurrentCell)} is {nameof(stiltWellCell)}",
                        Indent: indent + 3, Toggle: doDebugScrapWalls);
                    scrapWallMound.Obliterate();
                    continue;
                }

                if (scrapWallMound.HasPart<AnimatedObject>()
                    && scrapWallMound.TryGetPart(out Leveler leveler))
                {
                    Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugScrapWalls);
                    Debug.Logger.LoopItem(Verbosity.Max,
                        $"(Z:{currentStratum})[{scrapWallMound.CurrentCell.DebugName}] {nameof(scrapWallMound)}",
                        $"{scrapWallMound?.DebugName ?? NULL}",
                        Indent: indent + 2, Toggle: doDebugScrapWalls);

                    Debug.Logger.LoopItem(Verbosity.Max, $"Found", $"{nameof(AnimatedObject)}",
                        Indent: indent + 3, Toggle: doDebugScrapWalls);

                    int scrapWallLevels = Stat.RollCached("2d6+3");
                    Debug.Logger.LoopItem(Verbosity.Max,
                        $"Leveling up {scrapWallLevels} times...",
                        Indent: indent + 4, Toggle: doDebugScrapWalls);

                    Debug.Logger.LoopItem(Verbosity.Max,
                        $"-] {nameof(scrapWallMound)}.{nameof(scrapWallMound.Level)}", $"{scrapWallMound.Level}",
                        Indent: indent + 5, Toggle: doDebugScrapWalls);

                    for (int i = 0; i < scrapWallLevels; i++)
                    {
                        leveler.LevelUp();
                        Debug.Logger.LoopItem(Verbosity.Max,
                            $"{i}] {nameof(scrapWallMound)}.{nameof(scrapWallMound.Level)}", $"{scrapWallMound.Level}",
                            Indent: indent + 5, Toggle: doDebugScrapWalls);
                    }
                    scrapWallMound.RandomlySpendPoints();
                    scrapWallMound.ReceivePopulation("HumanoidEquipment 3");
                    if (scrapWallMound.IsGiganticCreature || scrapWallMound.IsGiganticEquipment)
                    {
                        scrapWallMound.ForeachInventoryAndEquipment(delegate (GameObject item)
                        {
                            if (ItemModding.ModificationApplicable(nameof(ModGigantic), item))
                            {
                                item.ApplyModification(nameof(ModGigantic));
                            }
                        });
                    }
                }
            }
            Debug.Logger.Divider(Verbosity.Max, HONLY, 40, Indent: indent + 2, Toggle: doDebugScrapWalls);

            Debug.Logger.Footer(Verbosity.Max,
                $"{nameof(UD_SubGrandCathedralScrapifier)}",
                $"{nameof(BuildZone)}({nameof(Z)}: {Z.ZoneID})", Toggle: doDebug);

            return true;
        }

        public static bool MaybeApplyGearbox(GameObject ScrapWall)
        {
            return !ScrapWall.HasPart<ModGearbox>()
                && 3.in10()
                && ScrapWall.ApplyModification(nameof(ModGearbox), Creation: true);
        }
        public static bool MaybeApplyWired(GameObject ScrapWall)
        {
            return !ScrapWall.HasPart<ModWired>()
                && 4.in10()
                && ScrapWall.ApplyModification(nameof(ModWired), Creation: true);
        }
        public static bool MaybeApplyPiped(GameObject ScrapWall)
        {
            if (!ScrapWall.HasPart<ModPiping>()
                && 6.in10())
            {
                string corpseBlueprintString = ScrapWall.GetPart<Corpse>().CorpseBlueprint;
                GameObjectBlueprint corpseBlueprint = GameObjectFactory.Factory.GetBlueprintIfExists(corpseBlueprintString);
                string corpseLiquid = corpseBlueprint.GetPartParameter(nameof(LiquidVolume), "InitialLiquid", "oil-600,gel-300,sludge-100");
                ModPiping modPiping = new()
                {
                    Liquid = corpseLiquid
                };
                return ScrapWall.ApplyModification(modPiping, Creation: true);
            }
            return false;
        }
        public static bool MaybeApplyEnergyCellSocket(GameObject ScrapWall)
        {
            if (ScrapWall.HasPart<ModWired>()
                && 4.in10()
                && ScrapWall.RequirePart<EnergyCellSocket>() is EnergyCellSocket energyCellSocket)
            {
                if (3.in10()
                    && PopulationManager.GenerateOne("Middle Weighted Random EnergyCell")?.Blueprint is string energyCellBlueprint
                    && GameObject.Create(energyCellBlueprint) is GameObject energyCellObject)
                {
                    float rechargePercent = Stat.RollCached("1d100") / 100f;
                    if (energyCellObject.TryGetPart(out LiquidFueledPowerPlant liquidFueledPowerPlant)
                        && energyCellObject.TryGetPart(out LiquidVolume liquidVolume))
                    {
                        Dictionary<string, int> liquidContents = new(liquidVolume.ComponentLiquids);
                        liquidVolume.UseDrams(liquidContents);
                        liquidVolume.AddDrams(
                            Liquid: liquidFueledPowerPlant.Liquid,
                            Drams: (int)Math.Min(liquidVolume.MaxVolume * rechargePercent, liquidVolume.MaxVolume));
                    }
                    else
                    if (energyCellObject.GetPart<EnergyCell>() is EnergyCell energyCell)
                    {
                        energyCell.Charge = (int)Math.Min(energyCell.MaxCharge * rechargePercent, energyCell.MaxCharge);
                        energyCellSocket.Cell = energyCellObject;
                    }
                }
                return true;
            }
            return false;
        }
        public static bool MaybeApplyFusionPower(GameObject ScrapWall)
        {
            if (ScrapWall.HasPart<ModWired>()
                && 15.in100()
                && ScrapWall.RequirePart<FusionReactor>() != null)
            {
                ScrapWall.RequirePart<DisplayNameAdjectives>().AddAdjective("powered");
            }
            return ScrapWall.HasPart<FusionReactor>();
        }
        public static bool MaybeApplyRustedOrBusted(GameObject ScrapWall, out string Applied)
        {
            Applied = null;
            if (6.in100() && ScrapWall.ForceApplyEffect(new Rusted()))
            {
                Applied = nameof(Rusted);
            }
            else
            if (6.in10() && ScrapWall.ForceApplyEffect(new Broken()))
            {
                Applied = nameof(Broken);
            }
            return ScrapWall.HasEffect<Rusted>() || ScrapWall.HasEffect<Broken>();
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
                Debug.Logger.CheckNah(verbosity, nameof(StillWellLocation),
                    Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasCombatObject())
            {
                Debug.Logger.CheckNah(verbosity, nameof(Cell.HasCombatObject),
                    Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasObject(GO => GO.Physics != null && GO.Physics.Solid))
            {
                Debug.Logger.CheckNah(verbosity, nameof(GameObject.Physics) + "." + nameof(GameObject.Physics.Solid),
                    Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasObjectInheritsFrom("Wall"))
            {
                Debug.Logger.CheckNah(verbosity, "Wall",
                    Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.SetIndent(indent);
                return false;
            }
            if (Cell.HasObjectWithPart((LiquidVolume l) => l.Volume > 9))
            {
                Debug.Logger.CheckNah(verbosity, nameof(Cell.HasObjectWithPart) + " " + nameof(LiquidVolume) + "." +
                    nameof(LiquidVolume.Volume) + " is GTE" + 10,
                    Indent: indent + 2, Toggle: doDebug);

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
                        Debug.Logger.CheckNah(verbosity, nameof(Parts.StairsDown),
                            Indent: indent + 2, Toggle: doDebug);

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
