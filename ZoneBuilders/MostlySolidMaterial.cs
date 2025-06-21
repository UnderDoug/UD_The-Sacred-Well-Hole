using System.Collections.Generic;

namespace XRL.World.ZoneBuilders
{
    public class MostlySolidMaterial : SolidMaterial
    {
        public List<Cell> Cells;

        public MostlySolidMaterial()
            : base()
        {
            Cells ??= Event.NewCellList();
        }
        public MostlySolidMaterial(string Material) 
            : base(Material)
        {
            Cells ??= Event.NewCellList();
        }
        public MostlySolidMaterial(List<Cell> Cells) 
            : base()
        {
            if (Cells != null)
            {
                this.Cells ??= Event.NewCellList(Cells);
            }
        }
        public MostlySolidMaterial(string Material, List<Cell> Cells) 
            : base(Material)
        {
            if (Cells != null)
            {
                this.Cells ??= Event.NewCellList(Cells);
            }
        }

        public override bool BuildZone(Zone Z)
        {
            return BuildZone(Z, Material, Cells);
        }
        public override bool BuildZone(Zone Z, string Material, List<Cell> Cells = null)
        {
            return base.BuildZone(Z, Material, Cells);
        }
    }
}