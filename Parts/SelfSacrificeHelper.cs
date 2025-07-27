using Qud.API;
using System;
using XRL;
using XRL.UI;
using XRL.World;

namespace XRL.World.Parts
{
    [Serializable]
    public class SelfSacrificeHelper : IScribedPart
    {
        public override bool WantEvent(int ID, int cascade)
        {
            return base.WantEvent(ID, cascade)
                || ID == ObjectEnteringCellEvent.ID
                || ID == ObjectEnteredCellEvent.ID
                || ID == ObjectLeavingCellEvent.ID;
        }
        public override bool HandleEvent(ObjectEnteringCellEvent E)
        {
            if (E.Object != ParentObject)
            {
                HelpSelfSacrifice(E.Object);
            }
            if (E.Actor != ParentObject)
            {
                HelpSelfSacrifice(E.Actor);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ObjectEnteredCellEvent E)
        {
            if (E.Object != ParentObject)
            {
                HelpSelfSacrifice(E.Object);
            }
            if (E.Actor != ParentObject)
            {
                HelpSelfSacrifice(E.Actor);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ObjectLeavingCellEvent E)
        {
            if (E.Object != ParentObject)
            {
                HelpSelfSacrifice(E.Object);
            }
            if (E.Actor != ParentObject)
            {
                HelpSelfSacrifice(E.Actor);
            }
            return base.HandleEvent(E);
        }

        public bool HelpSelfSacrifice(GameObject Sacrifice)
        {
            if (Sacrifice != null)
            {
                string stiltWellWithDefiniteArticle = Sacrifice.GetStringProperty("Might Self-Sacrifice");
                Sacrifice.SetStringProperty("Might Self-Sacrifice", null, true);
                return !Sacrifice.HasStringProperty("Jumped In Stilt Well") 
                    && !stiltWellWithDefiniteArticle.IsNullOrEmpty() 
                    && SelfSacrifice.MemorializeSelfSacrifice(Sacrifice, stiltWellWithDefiniteArticle);
            }
            return false;
        }
    }
}