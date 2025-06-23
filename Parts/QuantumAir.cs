using System;
using System.Collections.Generic;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.GoalHandlers;
using XRL.World.ZoneBuilders;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

using SerializeField = UnityEngine.SerializeField;

namespace XRL.World.Parts
{
    [Serializable]
    public class QuantumAir : IScribedPart
    {
        private static bool doDebug => getClassDoDebug(nameof(QuantumAir));
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

        public string CollapsingBlueprint;

        public bool DerivativeCollapses;

        public StairsDown StairsDown;

        public string FloorMaterial;

        [SerializeField]
        private string OriginalBlueprint = null;

        public QuantumAir()
        {
            CollapsingBlueprint = null;
            DerivativeCollapses = true;
            StairsDown = null;
            FloorMaterial = null;
        }

        public override void Attach()
        {
            base.Attach();
            OriginalBlueprint = ParentObject.Blueprint;
        }
        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register(EnteringCellEvent.ID, EventOrder.VERY_EARLY);
            Registrar.Register(ZoneActivatedEvent.ID, EventOrder.VERY_EARLY);
            Registrar.Register(ZoneThawedEvent.ID, EventOrder.VERY_EARLY);
            Registrar.Register(BeforeZoneBuiltEvent.ID, EventOrder.VERY_EARLY);
            base.Register(Object, Registrar);
        }
        public override bool HandleEvent(EnteringCellEvent E)
        {
            if (ParentObject != null)
            {
                CollapseAir();
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneActivatedEvent E)
        {
            if (ParentObject != null)
            {
                CollapseAir();
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneThawedEvent E)
        {
            if (ParentObject != null)
            {
                CollapseAir();
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(BeforeZoneBuiltEvent E)
        {
            if (ParentObject != null)
            {
                CollapseAir();
            }
            return base.HandleEvent(E);
        }

        public bool CollapseAir()
        {
            if (ParentObject != null && !CollapsingBlueprint.IsNullOrEmpty())
            {
                int indent = Debug.LastIndent;

                Debug.Entry(4,
                    $"* {nameof(QuantumAir)}."
                    + $"{nameof(CollapseAir)}("
                    + $"{nameof(Cell)}: [{ParentObject.CurrentCell.Location}])",
                    Indent: indent + 1, Toggle: getDoDebug());

                Cell cellBelow = ParentObject.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
                if (cellBelow.HasObjectWithBlueprint(CollapsingBlueprint)
                    || (DerivativeCollapses && cellBelow.HasObject(GO => GO.InheritsFrom(CollapsingBlueprint))))
                {
                    Debug.CheckYeh(4, $"Collapsing", Indent: indent + 2, Toggle: getDoDebug());
                    if (ParentObject.TryGetPart(out StairsDown))
                    {
                        ParentObject.RemovePart(StairsDown);
                    }
                    if (ParentObject.Render != null)
                    {
                        ParentObject.Render.Visible = false;
                    }
                    ParentObject.Blueprint = null;
                    if (FloorMaterial != null)
                    {
                        PaintCell(ParentObject.CurrentCell, cellBelow);
                    }
                }
                else
                {
                    Debug.CheckNah(4, $"Constructing", Indent: indent + 2, Toggle: getDoDebug());
                    if (StairsDown == null)
                    {
                        GameObject quantumAir = GameObjectFactory.Factory.CreateUnmodifiedObject(OriginalBlueprint);
                        if (quantumAir != null && quantumAir.TryGetPart(out StairsDown stairsDown))
                        {
                            StairsDown = stairsDown.DeepCopy(ParentObject) as StairsDown;
                        }
                    }
                    else
                    {
                        ParentObject.AddPart(StairsDown);
                    }
                    if (ParentObject.Render != null)
                    {
                        ParentObject.Render.Visible = true;
                    }
                    ParentObject.Blueprint = OriginalBlueprint;
                }
                Debug.LastIndent = indent;
                return true;
            }
            return false;
        }

        public void PaintCell(Cell CurrentCell, Cell CellBelow)
        {
            GameObject wallBelow = CellBelow.GetFirstObject(GO => GO.InheritsFrom(CollapsingBlueprint));

            string TileColor = Stat.RollCached("1d6") switch
            {
                5 => "w",
                4 => "r",
                3 => "c",
                2 => "g",
                1 => "K",
                _ => "y",
            };
            string DetailColor = Stat.RollCached("1d6") switch
            {
                5 => "w",
                4 => "r",
                3 => "c",
                2 => "g",
                1 => "K",
                _ => "y",
            };
            if (wallBelow != null && wallBelow.Render != null)
            {
                TileColor = wallBelow.Render.TileColor;
                DetailColor = wallBelow.Render.DetailColor;
            }
            UD_SubGrandCathedralBuilder.PaintCell(CurrentCell, FloorMaterial, TileColor, DetailColor, Overwrite: true, OverrideFloorColors: true);
        }
    }
}