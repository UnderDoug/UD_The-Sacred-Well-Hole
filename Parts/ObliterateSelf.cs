using System;
using XRL;
using XRL.World;

namespace XRL.World.Parts
{
    [Serializable]
    public class ObliterateSelf : IPart
    {
        public override bool SameAs(IPart p)
        {
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register(EnteredCellEvent.ID, EventOrder.VERY_EARLY);
            base.Register(Object, Registrar);
        }

        public override bool HandleEvent(EnteredCellEvent E)
        {
            ParentObject?.Obliterate();
            return false;
        }
    }
}