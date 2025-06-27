using System.Collections.Generic;

using UD_SacredWellHole;
using static UD_SacredWellHole.Options;
using static UD_SacredWellHole.Const;

namespace XRL.World.ZoneBuilders
{
    public class UDSW_MostlySolidMaterial : UDSW_SolidMaterial
    {
        private static bool doDebug => getClassDoDebug(nameof(UDSW_MostlySolidMaterial));
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

        public List<Cell> Cells;

        public UDSW_MostlySolidMaterial()
            : base()
        {
            Cells ??= Event.NewCellList();
        }
        public UDSW_MostlySolidMaterial(string Material = null, List<string> Materials = null, bool ClearFirst = true) 
            : base(Material, Materials, ClearFirst)
        {
            Cells ??= Event.NewCellList();
        }
        public UDSW_MostlySolidMaterial(List<Cell> Cells) 
            : base()
        {
            if (Cells != null)
            {
                this.Cells ??= Event.NewCellList(Cells);
            }
        }
        public UDSW_MostlySolidMaterial(string Material = null, List<string> Materials = null, bool ClearFirst = true, List<Cell> Cells = null) 
            : base(Material, Materials, ClearFirst)
        {
            if (Cells != null)
            {
                this.Cells ??= Event.NewCellList(Cells);
            }
        }

        public override bool BuildZone(Zone Z)
        {
            return BuildZone(Z, Material, Materials, Cells, ClearFirst);
        }
        public override bool BuildZone(Zone Z, string Material = null, List<string> Materials = null, List<Cell> Cells = null, bool ClearFirst = true)
        {
            return base.BuildZone(Z, Material, Materials, Cells, ClearFirst);
        }
    }
}