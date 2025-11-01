using System;
using System.Collections.Generic;

using XRL.Rules;
using XRL.World.Effects;

using Verbosity = UD_Modding_Toolbox.UD_Logger.Verbosity;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using Debug = UD_SacredWellHole.Debug;

namespace XRL.World.Parts
{
    public class UD_ScrapWallHelper : IScribedPart
    {
        public override bool AllowStaticRegistration()
        {
            return true;
        }
        public static bool MaybeApplyGearbox(GameObject ScrapWall)
        {
            return ScrapWall != null
                && !ScrapWall.HasPart<ModGearbox>()
                && 3.in10()
                && ScrapWall.ApplyModification(nameof(ModGearbox), Creation: true);
        }
        public bool MaybeApplyGearbox()
        {
            return MaybeApplyGearbox(ParentObject);
        }

        public static bool MaybeApplyWired(GameObject ScrapWall)
        {
            return ScrapWall != null
                && !ScrapWall.HasPart<ModWired>()
                && 4.in10()
                && ScrapWall.ApplyModification(nameof(ModWired), Creation: true);
        }
        public bool MaybeApplyWired()
        {
            return MaybeApplyWired(ParentObject);
        }

        public static bool MaybeApplyPiped(GameObject ScrapWall)
        {
            if (ScrapWall != null
                && !ScrapWall.HasPart<ModPiping>()
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
        public bool MaybeApplyPiped()
        {
            return MaybeApplyPiped(ParentObject);
        }

        public static bool MaybeApplyEnergyCellSocket(GameObject ScrapWall)
        {
            if (ScrapWall != null
                && ScrapWall.HasPart<ModWired>()
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
        public bool MaybeApplyEnergyCellSocket()
        {
            return MaybeApplyEnergyCellSocket(ParentObject);
        }

        public static bool MaybeApplyFusionPower(GameObject ScrapWall)
        {
            if (ScrapWall == null)
            {
                return false;
            }
            if (ScrapWall.HasPart<ModWired>()
                && 15.in100()
                && ScrapWall.RequirePart<FusionReactor>() != null)
            {
                ScrapWall.RequirePart<DisplayNameAdjectives>().AddAdjective("powered");
            }
            return ScrapWall.HasPart<FusionReactor>();
        }
        public bool MaybeApplyFusionPower()
        {
            return MaybeApplyFusionPower(ParentObject);
        }

        public static bool MaybeApplyRustedOrBusted(GameObject ScrapWall, out string Applied)
        {
            Applied = null;
            if (ScrapWall == null)
            {
                return false;
            }
            if (6.in100() && ScrapWall.ForceApplyEffect(new Rusted()))
            {
                Applied = nameof(Rusted);
            }
            else
            if (6.in10() && ScrapWall.ForceApplyEffect(new Broken()))
            {
                Applied = nameof(Broken);
            }
            return !Applied.IsNullOrEmpty();
        }
        public bool MaybeApplyRustedOrBusted(out string Applied)
        {
            return MaybeApplyRustedOrBusted(ParentObject, out Applied);
        }
        public bool MaybeApplyRustedOrBusted()
        {
            return MaybeApplyRustedOrBusted(out _);
        }

        public void ScrapMeUpBaby(IZoneEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone
                && E.Zone.IsPlayerWithinOneZone())
            {
                Debug.Logger.GetIndent(out int indent);
                Verbosity v4 = Verbosity.Max;
                bool doDebug = true;

                Debug.Logger.LoopItem(v4, E.GetType().Name, ParentObject?.DebugName ?? NULL,
                    Indent: indent + 1, Toggle: doDebug);

                Debug.Logger.LoopItem(v4, nameof(ModGearbox),
                    Good: MaybeApplyGearbox(), Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.LoopItem(v4, nameof(ModWired),
                    Good: MaybeApplyWired(), Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.LoopItem(v4, nameof(ModPiping),
                    Good: MaybeApplyPiped(), Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.LoopItem(v4, nameof(EnergyCellSocket),
                    Good: MaybeApplyEnergyCellSocket(), Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.LoopItem(v4, nameof(FusionReactor),
                    Good: MaybeApplyFusionPower(), Indent: indent + 2, Toggle: doDebug);

                bool maybeApplyRustedOrBusted = MaybeApplyRustedOrBusted(out string appliedRustedOrBusted);
                Debug.Logger.LoopItem(v4, appliedRustedOrBusted ?? nameof(appliedRustedOrBusted),
                    Good: maybeApplyRustedOrBusted, Indent: indent + 2, Toggle: doDebug);

                Debug.Logger.SetIndent(indent);
                ParentObject.RemovePart(this);
            }
        }

        public override bool WantEvent(int ID, int Cascade)
        {
            return base.WantEvent(ID, Cascade)
                || ID == ZoneActivatedEvent.ID
                || ID == ZoneThawedEvent.ID;
        }
        public override bool HandleEvent(ZoneActivatedEvent E)
        {
            ScrapMeUpBaby(E);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneThawedEvent E)
        {
            ScrapMeUpBaby(E);
            return base.HandleEvent(E);
        }
    }
}
