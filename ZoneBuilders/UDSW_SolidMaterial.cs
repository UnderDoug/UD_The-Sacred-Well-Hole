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
        public List<string> Materials;
        public bool ClearFirst;

        public UDSW_SolidMaterial() 
        {
            Material = null;
            ClearFirst = true;
            Materials = new();
        }
        public UDSW_SolidMaterial(string Material = null, List<string> Materials = null, bool ClearFirst = true) 
            : this()
        {
            this.Material = Material;
            this.Materials = Materials ?? new();
            if (!Material.IsNullOrEmpty())
            {
                Materials.Add(Material);
            }
            this.ClearFirst = ClearFirst;
        }

        public virtual bool BuildZone(Zone Z)
        {
            return BuildZone(Z, Material, Materials, null, ClearFirst);
        }
        public virtual bool BuildZone(Zone Z, string Material, List<string> Materials = null, List<Cell> Cells = null, bool ClearFirst = true)
        {
            Cells ??= Event.NewCellList(Z.GetCells());
            Materials ??= new();
            if (!Material.IsNullOrEmpty())
            {
                Materials.Add(Material);
            }
            foreach (Cell cell in Cells)
            {
                if (ClearFirst)
                {
                    cell.Clear(Combat: true, alsoExclude: GO => GO.InheritsFrom("Widget"));
                }
                foreach (string material in Materials)
                {
                    cell.AddObject(material);
                }
            }
            return true;
        }
    }
}