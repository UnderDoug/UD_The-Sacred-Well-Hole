using System;
using System.Collections.Generic;

using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
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

        public string FloorMaterial;

        public GameObject FloorObject;

        public string SolidifyingBlueprint;

        public bool DerivativeSolidifies;

        public QuantumAir()
        {
            FloorMaterial = null;
            SolidifyingBlueprint = null;
            DerivativeSolidifies = true;
        }
        public QuantumAir(GameObject FloorObject, string FloorMaterial = null, string SolidifyingBlueprint = null, bool DerivativeCollapses = true)
            : this()
        {
            this.FloorMaterial = FloorMaterial;
            this.FloorObject = FloorObject;
            this.SolidifyingBlueprint = SolidifyingBlueprint;
            this.DerivativeSolidifies = DerivativeCollapses;
        }
        public QuantumAir(SolidAir SolidAir)
            : this(SolidAir.ParentObject, SolidAir.ParentObject.Blueprint, SolidAir.SolidifyingBlueprint, SolidAir.DerivativeSolidifies)
        {
        }

        public override void Attach()
        {
            base.Attach();
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register(EnteringCellEvent.ID, EventOrder.VERY_EARLY);
            Registrar.Register(ObjectEnteringCellEvent.ID, EventOrder.VERY_EARLY);
            base.Register(Object, Registrar);
        }
        public override bool WantEvent(int ID, int Cascade)
        {
            return base.WantEvent(ID, Cascade)
                || ID == ZoneActivatedEvent.ID
                || ID == ZoneThawedEvent.ID
                || ID == BeforeZoneBuiltEvent.ID;
        }
        public override bool HandleEvent(EnteringCellEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Cell.ParentZone
                && !ShouldBeAir())
            {
                SolidifyAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ObjectEnteringCellEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Cell.ParentZone
                && !ShouldBeAir())
            {
                SolidifyAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneActivatedEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone
                && !ShouldBeAir())
            {
                SolidifyAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneThawedEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone
                && !ShouldBeAir())
            {
                SolidifyAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(BeforeZoneBuiltEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone
                && !ShouldBeAir())
            {
                SolidifyAir(E);
            }
            return base.HandleEvent(E);
        }

        public static bool ShouldBeAir(GameObject QuantumAirObject, string SolidifyingBlueprint, bool DerivativeSolidifies = true, string Source = null)
        {
            int indent = Debug.LastIndent;

            Cell quantumAirCell = QuantumAirObject?.CurrentCell;

            Debug.Entry(4,
                $"* {nameof(QuantumAir)}."
                + $"{nameof(ShouldBeAir)}("
                + $"{nameof(QuantumAirObject)}: [{QuantumAirObject?.DebugName ?? NULL}], "
                + $"{nameof(Source)}: {Source ?? NULL}) "
                + $"{nameof(Zone.Z)}: {quantumAirCell?.ParentZone?.Z}, "
                + $"{nameof(quantumAirCell)}: [{quantumAirCell?.Location}]",
                Indent: indent + 1, Toggle: getDoDebug('X'));

            if (quantumAirCell != null)
            {
                Debug.CheckNah(4, $"{nameof(quantumAirCell)} not null", Indent: indent + 2, Toggle: getDoDebug('X'));
                if (quantumAirCell.HasObject(GO => GO.InheritsFrom("Wall")))
                {
                    Debug.CheckNah(4, $"{nameof(quantumAirCell)} Has Wall", Indent: indent + 2, Toggle: getDoDebug('X'));
                    Debug.LastIndent = indent;
                    return false;
                }
            }
            Cell cellBelow = quantumAirCell?.GetCellFromDirection("D", BuiltOnly: false);

            Debug.LastIndent = indent;
            if (cellBelow == null)
            {
                Debug.CheckNah(4, $"{nameof(cellBelow)} is null", Indent: indent + 2, Toggle: getDoDebug('X'));
                Debug.LastIndent = indent;
                return false;
            }

            GameObject solidifyingObject = cellBelow.GetFirstObjectThatInheritsFrom(SolidifyingBlueprint);

            if (DerivativeSolidifies
                && cellBelow.HasObjectInheritsFrom(SolidifyingBlueprint)
                && ObjectCanSolidify(solidifyingObject))
            {
                Debug.CheckNah(4, $"{nameof(DerivativeSolidifies)} and {nameof(cellBelow)} contains derivative of {nameof(SolidifyingBlueprint)}, {SolidifyingBlueprint}",
                    Indent: indent + 2, Toggle: getDoDebug('X'));
                Debug.LastIndent = indent;
                return false;
            }

            if (cellBelow.HasObjectWithBlueprint(SolidifyingBlueprint)
                && ObjectCanSolidify(solidifyingObject))
            {
                Debug.CheckNah(4, $"{nameof(cellBelow)} contains {nameof(SolidifyingBlueprint)}, {SolidifyingBlueprint}",
                    Indent: indent + 2, Toggle: getDoDebug('X'));
                Debug.LastIndent = indent;
                return false;
            }

            Debug.CheckYeh(4, $"{nameof(cellBelow)} doesn't contain {nameof(SolidifyingBlueprint)}, {SolidifyingBlueprint}", Indent: indent + 2, Toggle: getDoDebug('X'));
            Debug.LastIndent = indent;
            return true;
        }
        public bool ShouldBeAir()
        {
            return ShouldBeAir(ParentObject, SolidifyingBlueprint, DerivativeSolidifies, nameof(QuantumAir));
        }

        public static bool ObjectCanSolidify(GameObject SolidifyingObject)
        {
            return SolidifyingObject != null
                && !SolidifyingObject.HasTag("PaintedFence")
                && !SolidifyingObject.HasTagOrProperty("Flyover")
                && !SolidifyingObject.IsCreature
                && !SolidifyingObject.HasPart<AnimatedObject>();
        }

        public bool SolidifyAir(MinEvent FromEvent = null)
        {
            int indent = Debug.LastIndent;
            if (ParentObject != null)
            {
                Debug.Entry(4,
                    $"* {nameof(QuantumAir)}."
                    + $"{nameof(SolidifyAir)}("
                    + $"{nameof(Zone.Z)}: {ParentObject?.CurrentCell?.ParentZone?.Z}, "
                    + $"{nameof(Cell)}: [{ParentObject?.CurrentCell?.Location}], "
                    + $"{nameof(FromEvent)}: {FromEvent?.GetType()?.Name})",
                    Indent: indent + 1, Toggle: getDoDebug());

                Cell cellBelow = ParentObject?.CurrentCell?.GetCellFromDirection("D", BuiltOnly: false);
                GameObject wallBelow = cellBelow?.GetFirstObject(GO => GO.InheritsFrom("Wall"));

                if (FloorMaterial.IsNullOrEmpty())
                {
                    FloorMaterial = wallBelow?.GetPropertyOrTag("QuantumAirFloorMaterial", null) ?? "SolidAir";
                }
                Debug.LoopItem(4, $"{nameof(FloorMaterial)}", $"{FloorMaterial}", Indent: indent + 2, Toggle: getDoDebug());
                FloorObject ??= GameObjectFactory.Factory.CreateUnmodifiedObject(FloorMaterial);

                if (FloorObject != null)
                {
                    Debug.CheckYeh(4, $"Solidifying", Indent: indent + 2, Toggle: getDoDebug());
                    if (!FloorObject.TryGetPart(out SolidAir solidAir))
                    {
                        solidAir = FloorObject.AddPart(new SolidAir(this));
                    }
                    if (solidAir != null)
                    {
                        Cell currentCell = ParentObject.CurrentCell;
                        currentCell.RemoveObject(ParentObject, System: true, Silent: true, ParentEvent: FromEvent);
                        currentCell.AddObject(FloorObject, System: true, Silent: true, ParentEvent: FromEvent);
                        if (wallBelow != null && wallBelow.Render != null)
                        {
                            if (FloorObject.TryGetPart(out RandomColors randomColors))
                            {
                                FloorObject.RemovePart(randomColors);
                            }
                            FloorObject.Render.TileColor = wallBelow.Render.TileColor;
                            FloorObject.Render.DetailColor = wallBelow.Render.DetailColor;
                        }
                        Debug.LastIndent = indent;
                        return true;
                    }
                    else
                    {
                        Debug.CheckNah(4, $"Solidification failed...", Indent: indent + 2, Toggle: getDoDebug());
                    }
                }
            }
            Debug.LastIndent = indent;
            return false;
        }

        public void PaintCell(Cell CurrentCell, Cell CellBelow)
        {
            GameObject wallBelow = CellBelow.GetFirstObject(GO => GO.InheritsFrom(SolidifyingBlueprint));

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