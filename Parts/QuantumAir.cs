using System;
using System.Collections.Generic;

using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.ZoneBuilders;

using UD_Modding_Toolbox;

using UD_SacredWellHole;

using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

using Debug = UD_SacredWellHole.Debug;

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
                nameof(SolidifyAir),
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
        public Cell CellBelow => ParentObject?.CurrentCell?.GetCellFromDirection("D", BuiltOnly: false);

        public QuantumAir()
        {
            FloorMaterial = null;
            SolidifyingBlueprint = null;
            DerivativeSolidifies = true;
        }
        public QuantumAir(
            GameObject FloorObject, 
            string FloorMaterial = null, 
            string SolidifyingBlueprint = null, 
            bool DerivativeSolidifies = true)
            : this()
        {
            this.FloorMaterial = FloorMaterial;
            this.FloorObject = FloorObject;
            this.SolidifyingBlueprint = SolidifyingBlueprint;
            this.DerivativeSolidifies = DerivativeSolidifies;
        }
        public QuantumAir(SolidAir SolidAir)
            : this(
                  FloorObject: SolidAir.ParentObject,
                  FloorMaterial: SolidAir.ParentObject.Blueprint,
                  SolidifyingBlueprint: SolidAir.SolidifyingBlueprint,
                  DerivativeSolidifies: SolidAir.DerivativeSolidifies)
        {
        }

        public override void Attach()
        {
            base.Attach();
        }

        static bool HasSolidConnection(Cell Cell)
        {
            return Cell.HasObjectInheritsFrom("Wall")
                || !Cell.HasObjectInheritsFrom("OpenAir")
                || Cell.GetFirstObjectPart<StairsDown>() is not StairsDown stairsdown
                || !stairsdown.PullDown;
        }

        public static bool ShouldBeAir(
            GameObject QuantumAirObject,
            string SolidifyingBlueprint,
            Cell CellBelow,
            bool DerivativeSolidifies = true,
            string Source = null)
        {
            int indent = Debug.LastIndent;
            bool doDebug = getDoDebug('X');

            Cell quantumAirCell = QuantumAirObject?.CurrentCell;

            Debug.Entry(4,
                $"* {nameof(QuantumAir)}."
                + $"{nameof(ShouldBeAir)}("
                + $"{nameof(QuantumAirObject)}: [{QuantumAirObject?.DebugName ?? NULL}], "
                + $"{nameof(Source)}: {Source ?? NULL}) "
                + $"{nameof(Zone.Z)}: {quantumAirCell?.ParentZone?.Z}, "
                + $"{nameof(quantumAirCell)}: [{quantumAirCell?.Location}]",
                Indent: indent + 1, Toggle: doDebug);

            if (quantumAirCell != null)
            {
                Debug.CheckNah(4, $"{nameof(quantumAirCell)} not null", Indent: indent + 2, Toggle: doDebug);
                if (quantumAirCell.HasObject(GO => GO.InheritsFrom("Wall")))
                {
                    if (quantumAirCell.GetCardinalAdjacentCellsWhere(HasSolidConnection).Count > 0)
                    {
                        string wallName = quantumAirCell.GetFirstObjectThatInheritsFrom("Wall")?.DebugName ?? NULL;
                        Debug.CheckNah(4,
                            $"{nameof(quantumAirCell)} Has wall " + wallName + " and at least one connected cardinal direction cell",
                            Indent: indent + 2, Toggle: doDebug);
                        Debug.LastIndent = indent;
                        return false;
                    }
                }
            }

            if (CellBelow == null)
            {
                Debug.CheckNah(4, $"{nameof(CellBelow)} is null", Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }

            GameObject solidifyingObject = CellBelow?.GetFirstObjectThatInheritsFrom(SolidifyingBlueprint);

            if (DerivativeSolidifies
                && CellBelow.HasObjectInheritsFrom(SolidifyingBlueprint)
                && ObjectCanSolidify(solidifyingObject))
            {
                Debug.CheckNah(4,
                    $"{nameof(DerivativeSolidifies)} and {nameof(CellBelow)} contains derivative of " +
                    $"{nameof(SolidifyingBlueprint)}, {SolidifyingBlueprint}",
                    solidifyingObject?.DebugName ?? NULL,
                    Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }

            if (CellBelow.HasObjectWithBlueprint(SolidifyingBlueprint)
                && ObjectCanSolidify(solidifyingObject))
            {
                Debug.CheckNah(4,
                    $"{nameof(CellBelow)} contains {nameof(SolidifyingBlueprint)}, {SolidifyingBlueprint}",
                    solidifyingObject?.DebugName ?? NULL,
                    Indent: indent + 2, Toggle: doDebug);
                Debug.LastIndent = indent;
                return false;
            }

            Debug.CheckYeh(4,
                $"{nameof(CellBelow)} doesn't contain {nameof(SolidifyingBlueprint)}, {SolidifyingBlueprint}",
                Indent: indent + 2, Toggle: doDebug);
            Debug.LastIndent = indent;
            return true;
        }
        public bool ShouldBeAir()
        {
            return ShouldBeAir(
                QuantumAirObject: ParentObject,
                SolidifyingBlueprint: SolidifyingBlueprint,
                CellBelow: CellBelow,
                DerivativeSolidifies: DerivativeSolidifies,
                Source: nameof(QuantumAir));
        }

        public static bool ObjectCanSolidify(GameObject SolidifyingObject)
        {
            return SolidifyingObject != null
                && !SolidifyingObject.HasTag("PaintedFence")
                && !SolidifyingObject.HasTagOrProperty("Flyover");
                // && !SolidifyingObject.IsCreature
                // && !SolidifyingObject.HasPart<AnimatedObject>();
        }

        public bool SolidifyAir(MinEvent FromEvent = null)
        {
            int indent = Debug.LastIndent;
            bool doDebug = getDoDebug(nameof(SolidifyAir));
            if (ParentObject is GameObject quantumAirObject)
            {
                Cell quantumAirCell = quantumAirObject?.CurrentCell;
                Debug.Entry(4,
                    $"* {nameof(QuantumAir)}."
                    + $"{nameof(SolidifyAir)}("
                    + $"{nameof(Zone.Z)}: {quantumAirCell?.ParentZone?.Z}, "
                    + $"{nameof(Cell)}: [{quantumAirCell?.Location}], "
                    + $"{nameof(FromEvent)}: {FromEvent?.GetType()?.Name})",
                    Indent: indent + 1, Toggle: doDebug);

                GameObject wallBelow = CellBelow?.GetFirstObject(GO => GO.InheritsFrom("Wall"));

                if (FloorMaterial.IsNullOrEmpty())
                {
                    FloorMaterial = wallBelow?.GetPropertyOrTag("QuantumAirFloorMaterial", null) ?? "SolidAir";
                }
                Debug.LoopItem(4, $"{nameof(FloorMaterial)}", $"{FloorMaterial}", Indent: indent + 2, Toggle: doDebug);
                FloorObject ??= GameObjectFactory.Factory.CreateUnmodifiedObject(FloorMaterial);

                if (FloorObject != null)
                {
                    Debug.CheckYeh(4, $"Solidifying", Indent: indent + 2, Toggle: doDebug);
                    if (!FloorObject.TryGetPart(out SolidAir solidAir))
                    {
                        solidAir = FloorObject.AddPart(new SolidAir(this));
                    }
                    if (solidAir != null)
                    {
                        quantumAirCell.RemoveObject(quantumAirObject, System: true, Silent: true, ParentEvent: FromEvent);
                        quantumAirCell.AddObject(FloorObject, System: true, Silent: true, ParentEvent: FromEvent);
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
                        Debug.CheckNah(4, $"Solidification failed...", Indent: indent + 2, Toggle: doDebug);
                    }
                }
            }
            Debug.LastIndent = indent;
            return false;
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
    }
}