using Qud.API;
using System;
using XRL;
using XRL.UI;
using XRL.World;

namespace XRL.World.Parts
{
    [Serializable]
    public class SelfSacrifice : IPart
    {
        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == GetInventoryActionsEvent.ID
                || ID == InventoryActionEvent.ID
                || ID == ObjectLeavingCellEvent.ID;
        }
        public override bool HandleEvent(GetInventoryActionsEvent E)
        {
            E.AddAction("Sacrifice Self", "sacrifice self", "Sacrifice Self", null, 'S');
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(InventoryActionEvent E)
        {
            if (E.Command == "Sacrifice Self"
                && Popup.ShowYesNo(
                    $"It looks like an awfully long fall, " +
                    $"are you sure you want to jump into " +
                    $"{ParentObject.DefiniteArticle()}{ParentObject?.Render?.DisplayName}?", 
                    defaultResult: DialogResult.No
                    ) == DialogResult.Yes
                && E.Actor.Move(!E.Actor.IsFlying ? E.Actor.GetDirectionToward(ParentObject) : "D", System: true, DoConfirmations: false))
            {
                MemorializeSelfSacrifice(E.Actor);
                E.RequestInterfaceExit();
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ObjectLeavingCellEvent E)
        {
            if (E.Direction == "D")
            {
                MemorializeSelfSacrifice(E.Actor);
            }
            return base.HandleEvent(E);
        }
        public bool MemorializeSelfSacrifice(GameObject Sacrifice)
        {
            if (Sacrifice != null)
            {
                Sacrifice.SetIntProperty("Might Self-Sacrifice", 0, true);
                if (!Sacrifice.HasPropertyOrTag("Jumped In Stilt Well") && Sacrifice.IsSelfControlledPlayer())
                {
                    Sacrifice.SetIntProperty("Jumped In Stilt Well", 1);
                    JournalAPI.AddAccomplishment(
                    text: $"You proclaimed yourself an artifact and threw yourself into {ParentObject.DefiniteArticle()}{ParentObject?.Render?.DisplayName}",
                    muralText: $"O! Praise the chrome! On the {Calendar.GetDay()} of {Calendar.GetMonth()}, in the year {Calendar.GetYear()} AR, =name= recognised =subject.reflexive= as the priceless relic =pronoun.subjective= =verb:are:afterpronoun= and made an iconic descent into {ParentObject.DefiniteArticle()}{ParentObject?.Render?.DisplayName}.",
                    gospelText: $"<spice.instancesOf.inYear.!random.capitalize> =year=, " +
                        $"<spice.instancesOf.afterTumultuousYears.!random>, =player.possessive= counselors suggested " +
                        $"=player.subjective= <spice.instancesOf.abdicate.!random> as sultan. " +
                        $"Instead, =player.subjective= recognised =subject.reflexive= as the " +
                        $"priceless relic =pronoun.subjective= =verb:are:afterpronoun= and made " +
                        $"an iconic descent into {ParentObject.DefiniteArticle()}{ParentObject?.Render?.DisplayName}.",
                    muralCategory: MuralCategory.DoesSomethingRad,
                    muralWeight: MuralWeight.VeryHigh);

                    The.Game.PlayerReputation.Modify("Mechanimists", 100, "StiltWellSelf");
                    return true;
                }
            }
            return false;
        }
    }
}