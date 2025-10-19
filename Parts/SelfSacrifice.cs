using Qud.API;
using System;
using XRL;
using XRL.UI;
using XRL.World;

namespace XRL.World.Parts
{
    [Serializable]
    public class SelfSacrifice : IScribedPart
    {
        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == GetInventoryActionsEvent.ID
                || ID == InventoryActionEvent.ID
                || ID == ObjectLeavingCellEvent.ID
                || ID == ObjectEnteredCellEvent.ID;
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
        public override bool HandleEvent(ObjectEnteredCellEvent E)
        {
            if (!E.IgnoreGravity)
            {
                MemorializeSelfSacrifice(E.Actor);
            }
            return base.HandleEvent(E);
        }
        public static bool MemorializeSelfSacrifice(GameObject Sacrifice, string StiltWellWithDefiniteArticle)
        {
            if (Sacrifice != null)
            {
                Sacrifice.SetStringProperty("Might Self-Sacrifice", StiltWellWithDefiniteArticle);
                if (!Sacrifice.HasPropertyOrTag("Jumped In Stilt Well") && Sacrifice.IsSelfControlledPlayer())
                {
                    Sacrifice.SetStringProperty("Might Self-Sacrifice", null, true);
                    Sacrifice.SetIntProperty("Jumped In Stilt Well", 1);
                    JournalAPI.AddAccomplishment(
                    text: $"You proclaimed yourself an artifact and threw yourself into {StiltWellWithDefiniteArticle}",
                    muralText: $"O! Praise the chrome! On the {Calendar.GetDay()} of {Calendar.GetMonth()}, in the year {Calendar.GetYear()} AR, =name= recognised =player.reflexive= as the priceless relic =player.subjective= =verb:are:afterpronoun= and made an iconic descent into {StiltWellWithDefiniteArticle}.",
                    gospelText: $"<spice.instancesOf.inYear.!random.capitalize> =year=, " +
                        $"<spice.instancesOf.afterTumultuousYears.!random>, =player.possessive= counselors suggested " +
                        $"=player.subjective= <spice.instancesOf.abdicate.!random> as sultan. " +
                        $"Instead, =player.subjective= recognised =player.reflexive= as the " +
                        $"priceless relic =player.subjective= =verb:are:afterpronoun= and made " +
                        $"an iconic descent into {StiltWellWithDefiniteArticle}.",
                    muralCategory: MuralCategory.DoesSomethingRad,
                    muralWeight: MuralWeight.VeryHigh);

                    The.Game.PlayerReputation.Modify("Mechanimists", 100, "StiltWellSelf");
                    return true;
                }
            }
            return false;
        }
        public bool MemorializeSelfSacrifice(GameObject Sacrifice)
        {
            return MemorializeSelfSacrifice(Sacrifice, $"{ParentObject.DefiniteArticle()}{ParentObject?.Render?.DisplayName}");
        }
    }
}