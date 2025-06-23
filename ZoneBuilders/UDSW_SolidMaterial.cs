using System.Collections.Generic;

using UD_SacredWellHole;
using static UD_SacredWellHole.Options;
using static UD_SacredWellHole.Const;

namespace XRL.World.ZoneBuilders
{
    public class UDSW_SolidMaterial
    {
        private static bool doDebug => getClassDoDebug(nameof(UDSW_SolidMaterial));
        private static bool getDoDebug(object what = null)
        {
            List<object> doList = new()
            {
                'V',    // Vomit
            };
            List<object> dontList = new()
            {
                'X',    // Trace
            };

            if (what != null && doList.Contains(what))
                return true;

            if (what != null && dontList.Contains(what))
                return false;

            return doDebug;
        }

        public string Material;

        public UDSW_SolidMaterial() 
        {
            Material = "Shale";
        }
        public UDSW_SolidMaterial(string Material) 
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