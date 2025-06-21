using System;
using XRL;
using XRL.World;

namespace XRL.World.Parts
{
    [Serializable]
    public class DaylightCell : Daylight
    {
        public int MaxRadius;

        public DaylightCell()
        {
            MaxRadius = 4;
        }

        public override bool SameAs(IPart p)
        {
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register(BeforeRenderEvent.ID, EventOrder.VERY_EARLY);
            base.Register(Object, Registrar);
        }

        public override bool HandleEvent(BeforeRenderEvent E)
        {
            Cell currentCell = ParentObject.CurrentCell;
            int hour = Calendar.CurrentDaySegment / 500;
            int minute = (int)((Calendar.CurrentDaySegment - 500 * hour) / 8.33333f);
            int lightRadius;
            MaxRadius = MaxRadius == 0 ? 1 : MaxRadius;
            if (hour < 5)
            {
                lightRadius = 0;
            }
            else if (hour >= 5 && (hour < 18 || (hour == 18 && minute < 15)))
            {
                lightRadius = (Calendar.CurrentDaySegment - 2500) / 10;
            }
            else
            {
                lightRadius = MaxRadius - (Calendar.CurrentDaySegment - 9124) / (200 * (4 / MaxRadius));
                if (lightRadius < 0)
                {
                    lightRadius = 0;
                }
            }
            lightRadius = Math.Min(MaxRadius, lightRadius);
            LightLevel Light = LightLevel.Light;
            GetAmbientLightEvent.Send(this, "Daylight", ref Light, ref lightRadius);
            if (lightRadius > 0)
            {
                currentCell.ParentZone.AddLight(currentCell.X, currentCell.Y, lightRadius, Light);
            }
            return true;
        }
    }
}