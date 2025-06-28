using System;
using System.Collections.Generic;

using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

using UD_SacredWellHole;

namespace XRL.World.Parts
{
    [Serializable]
    public class ScrambleUp : IPart
    {
        public static bool CanScramble(GameObject Scramblee, GameObject Scrambler, bool Silent = false)
        {
            if (Scramblee == null || Scrambler == null)
            {
                return false;
            }
            Cell scrambleeCell = Scramblee.CurrentCell;
            Cell scramblerCell = Scrambler.CurrentCell;
            if (scrambleeCell == null || scramblerCell == null)
            {
                return false;
            }
            if (!scrambleeCell.GetAdjacentCells().Contains(scramblerCell))
            {
                if (!Silent)
                {
                    Scrambler.Fail("You cannot scramble up something you're not adjacent to!");
                }
                return false;
            }
            if (Scrambler.CurrentCell.HasObject(GO => GO.IsWadingDepthLiquid()))
            {
                if (!Silent)
                {
                    Scrambler.Fail("You cannot scramble up while wading!");
                }
                return false;
            }
            if (!Scrambler.CanChangeBodyPosition("Scramble", !Silent) || !Scrambler.CanChangeMovementMode("Scramble", !Silent))
            {
                if (!Silent)
                {
                    // Scrambler.Fail("You cannot scramble up something if you're unable to move!");
                }
                return false;
            }
            if (Scrambler.IsFrozen())
            {
                if (!Silent)
                {
                    Scrambler.Fail("You are frozen solid!");
                }
                return false;
            }
            Cell cellAbove = scramblerCell.GetCellFromDirection("U", BuiltOnly: false);
            if (cellAbove == null)
            {
                if (!Silent)
                {
                    Scrambler.Fail("There's nowhere to scramble up to!");
                }
                return false;
            }
            if (cellAbove.IsSolidFor(Scrambler) || !cellAbove.HasObjectWithPart(nameof(StairsDown)))
            {
                if (!Silent)
                {
                    Scrambler.Fail("The way up is blocked!");
                }
                return false;
            }
            string scrambleDirection = scramblerCell.GetDirectionFromCell(scrambleeCell);
            List<Cell> possibleDestinations = Event.NewCellList(cellAbove.GetAdjacentCellsInGeneralDirection(scrambleDirection));
            possibleDestinations.RemoveAll(c => c.IsSolidFor(Scrambler));
            if (possibleDestinations.IsNullOrEmpty())
            {
                if (!Silent)
                {
                    Scrambler.Fail("There's nowhere to scramble up to!");
                }
                return false;
            }
            return true;
        }
        public static List<Cell> GetScrableDestinations(Cell ScrambleeCell, Cell ScramblerCell)
        {
            string scrambleDirection = ScramblerCell.GetDirectionFromCell(ScrambleeCell);
            Cell cellAbove = ScramblerCell.GetCellFromDirection("U", BuiltOnly: false);
            if (cellAbove != null)
            {
                return Event.NewCellList(cellAbove.GetAdjacentCellsInGeneralDirection(scrambleDirection));
            }
            return null;
        }
        public bool PerformScramble(GameObject Scramblee, GameObject Scrambler, bool Silent = false)
        {
            if (CanScramble(Scramblee, Scrambler, Silent))
            {
                Cell scrambleeCell = Scramblee.CurrentCell;
                Cell scramblerCell = Scrambler.CurrentCell;
                Cell cellAboveScramblee = scrambleeCell.GetCellFromDirection("U", BuiltOnly: false);
                List<Cell> destinationCells = GetScrableDestinations(scrambleeCell, scramblerCell);
                destinationCells.RemoveAll(c => c.IsSolidFor(Scrambler) || c.HasObjectWithPart(nameof(StairsDown)));
                Cell destinationCell = cellAboveScramblee;
                if (!destinationCells.IsNullOrEmpty())
                {
                    if (!destinationCells.Contains(cellAboveScramblee))
                    {
                        destinationCell = destinationCells.GetRandomElementCosmetic();
                    }
                    if (destinationCell != null)
                    {
                        if (Scrambler.IsPlayerControlled() 
                            && (Scramblee.HasEffect<Rusted>() || Scramblee.HasEffect<Broken>()) 
                            && Popup.ShowYesNo(
                                $"That {Scramblee?.Render?.DisplayName ?? Scramblee?.GetBlueprint()?.DisplayName()} looks like a jagged mess, " +
                                $"there's a good chance you'll hurt yourself scrambling up {Scramblee.it}. " +
                                $"Do you want to try anyway?", 
                                defaultResult: DialogResult.No
                                ) != DialogResult.Yes)
                        {
                            return false;
                        }
                        Scramblee.BodyPositionChanged("Scrambling");
                        Scramblee.MovementModeChanged("Scrambling");
                        if (Scrambler.DirectMoveTo(destinationCell, 1000))
                        {
                            Scrambler.EmitMessage(GameText.VariableReplace($"=subject.T= =verb:scramble:afterpronoun= up =object.t=!", Subject: Scrambler, Object: Scramblee));
                            
                            if (Scramblee.HasEffect<Rusted>() || Scramblee.HasEffect<Broken>() && Stat.RollCached("1d4") > 1)
                            {
                                int damageAmount = Stat.RollCached("1d4+1");
                                if (Scrambler.TakeDamage(
                                    ref damageAmount, 
                                    Attributes: "Bleed Unavoidable",
                                    DeathReason: "You were shredded to ribbons on %t jagged edges", 
                                    ThirdPersonDeathReason: Scrambler.It + Scrambler.GetVerb("were") + " shredded to ribbons on %t jagged edges", 
                                    Attacker: Scramblee, 
                                    Message: "from %t jagged edges", 
                                    Accidental: true, 
                                    Environmental: true, 
                                    IgnoreVisibility: true) && Stat.RollCached("1d3") > 1)
                                {
                                    Scrambler.ApplyEffect(new Bleeding("1d3", 20, Scramblee));
                                }
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == GetInventoryActionsEvent.ID
                || ID == InventoryActionEvent.ID
                || ID == CanSmartUseEvent.ID
                || ID == CommandSmartUseEvent.ID;
        }
        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            E.AddAction("Scramble Up", "scramble up", "Scramble Up", null, 's', Default: 5);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Command == "Scramble Up" && PerformScramble(E.Item, E.Actor, E.Silent))
            {
                E.RequestInterfaceExit();
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(CanSmartUseEvent E)
        {
            bool canSmartUse =
                E.Item == ParentObject
             && !E.Item.IsCreature
             && !E.Item.HasPart<AnimatedObject>()
             && !E.Item.IsHostileTowards(E.Actor)
             && CanScramble(E.Item, E.Actor, Silent: true);

            if (canSmartUse)
            {
                return false; // not sure the logic, but this one is a "false means yes"
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(CommandSmartUseEvent E)
        {
            if (!E.Item.HasTagOrProperty("ForceSmartUse") && PerformScramble(E.Item, E.Actor))
            {
                // return false;
            }
            return base.HandleEvent(E);
        }
    }
}