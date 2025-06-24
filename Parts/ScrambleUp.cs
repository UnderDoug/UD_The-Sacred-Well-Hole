using System;
using System.Collections.Generic;

using UD_SacredWellHole;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;

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
            if (!Scrambler.CanChangeBodyPosition() || !Scrambler.CanChangeMovementMode())
            {
                if (!Silent)
                {
                    Scrambler.Fail("You cannot scramble up something if you're unable to move!");
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
            if (cellAbove.IsSolidFor(Scrambler))
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
                destinationCells.RemoveAll(c => c.IsSolidFor(Scrambler));
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
                                $"there's a good chance you'll hurt yourself scrambling up it. " +
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
                            DidXToY("scramble", Scrambler, "up " + Scramblee.DefiniteArticle() + "=subject.t=", "!");
                            if (Scramblee.HasEffect<Rusted>() || Scramblee.HasEffect<Broken>() && Stat.RollCached("1d4") > 1)
                            {
                                int damageAmount = Stat.RollCached("1d4+1");
                                if (Scrambler.TakeDamage(
                                    ref damageAmount, 
                                    Attributes: "bleed",
                                    DeathReason: "shredded", 
                                    ThirdPersonDeathReason: "shredded", 
                                    Attacker: Scramblee, 
                                    Message: "from %t jagged edges", 
                                    Accidental: true, 
                                    Environmental: true, 
                                    IgnoreVisibility: true))
                                {
                                    Scrambler.ApplyEffect(new Bleeding("1d3", 22, Scramblee));
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