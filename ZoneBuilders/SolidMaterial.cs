using System.Collections.Generic;

namespace XRL.World.ZoneBuilders
{
    public class SolidMaterial
    {
        public string Material;

        public SolidMaterial() 
        {
            Material = "Shale";
        }
        public SolidMaterial(string Material) 
            : this()
        {
            this.Material = Material;
        }

        public virtual bool BuildZone(Zone Z)
        {
            return BuildZone(Z, Material);
        }
        public virtual bool BuildZone(Zone Z, string Material, List<Cell> Cells = null)
        {
            Cells ??= Event.NewCellList(Z.GetCells());
            foreach (Cell cell in Cells)
            {
                cell.Clear(Combat: true);
                cell.AddObject(GameObjectFactory.Factory.CreateObject(Material));
            }
            return true;
        }
    }
}