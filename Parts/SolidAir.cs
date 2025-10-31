using System;
using System.Collections.Generic;

using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts.Mutation;
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
    public class SolidAir : IScribedPart
    {
        private static bool doDebug => getClassDoDebug(nameof(SolidAir));
        private static bool getDoDebug(object what = null)
        {
            List<object> doList = new()
            {
                'V',    // Vomit
            };
            List<object> dontList = new()
            {
                nameof(CollapseAir),
                'X',    // Trace
            };

            if (what != null && doList.Contains(what))
                return true;

            if (what != null && dontList.Contains(what))
                return false;

            return doDebug;
        }

        public string AirMaterial;

        public GameObject AirObject;

        public string SolidifyingBlueprint;

        public bool DerivativeSolidifies;
        public Cell CellBelow => ParentObject?.CurrentCell?.GetCellFromDirection("D", BuiltOnly: false);

        [SerializeField]
        private bool CurrentlyLazy;

        public SolidAir()
        {
            AirMaterial = null;
            AirObject = null;
            SolidifyingBlueprint = null;
            DerivativeSolidifies = true;

            CurrentlyLazy = false;
        }
        public SolidAir(
            GameObject AirObject,
            string AirMaterial = null,
            string SolidifyingBlueprint = null,
            bool DerivativeSolidifies = true)
            : this()
        {
            this.AirMaterial = AirMaterial;
            this.AirObject = AirObject;
            this.SolidifyingBlueprint = SolidifyingBlueprint;
            this.DerivativeSolidifies = DerivativeSolidifies;
        }
        public SolidAir(QuantumAir QuantumAir)
            : this(
                  AirObject: QuantumAir.ParentObject,
                  AirMaterial: QuantumAir.ParentObject.Blueprint,
                  SolidifyingBlueprint: QuantumAir.SolidifyingBlueprint,
                  DerivativeSolidifies: QuantumAir.DerivativeSolidifies)
        {
        }

        public bool CollapseAir(MinEvent FromEvent = null)
        {
            int indent = Debug.LastIndent;
            bool doDebug = getDoDebug(nameof(CollapseAir));
            if (ParentObject is GameObject solidAirObject)
            {
                Cell solidAirCell = solidAirObject.CurrentCell;
                Debug.Entry(4,
                    $"* {nameof(SolidAir)}."
                    + $"{nameof(CollapseAir)}("
                    + $"{nameof(Zone.Z)}: {solidAirCell?.ParentZone?.Z}, "
                    + $"{nameof(Cell)}: [{solidAirCell?.Location}], "
                    + $"{nameof(FromEvent)}: {FromEvent?.GetType()?.Name})",
                    Indent: indent + 1, Toggle: doDebug);

                if (AirMaterial.IsNullOrEmpty())
                {
                    AirMaterial = "QuantumAir";
                }
                Debug.LoopItem(4, $"{nameof(AirMaterial)}", $"{AirMaterial}", Indent: indent + 2, Toggle: doDebug);
                AirObject ??= GameObjectFactory.Factory.CreateUnmodifiedObject(AirMaterial);
                if (!AirObject.TryGetPart(out QuantumAir quantumAir))
                {
                    quantumAir = AirObject.AddPart(new QuantumAir(this));
                }
                if (AirObject != null)
                {
                    Debug.CheckYeh(4, $"Collapsing...", Indent: indent + 2, Toggle: doDebug);
                    solidAirCell.RemoveObject(solidAirObject, System: true, Silent: true, ParentEvent: FromEvent);
                    solidAirCell.AddObject(AirObject, System: true, Silent: true, ParentEvent: FromEvent);

                    Debug.LastIndent = indent;
                    return true;
                }
                else
                {
                    Debug.CheckNah(4, $"Collapse failed...", Indent: indent + 2, Toggle: doDebug);
                }
            }
            Debug.LastIndent = indent;
            return false;
        }

        public override bool WantTurnTick()
        {
            return true;
        }
        public override void TurnTick(long TimeTick, int Amount)
        {
            base.TurnTick(TimeTick, Amount);
            if (The.CurrentTurn % 30L == 0)
            {
                CurrentlyLazy = false;
            }
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
                || (!CurrentlyLazy && ID == ZoneActivatedEvent.ID)
                || (!CurrentlyLazy && ID == ZoneThawedEvent.ID)
                || (!CurrentlyLazy && ID == BeforeZoneBuiltEvent.ID);
        }
        public override bool HandleEvent(EnteringCellEvent E)
        {
            if (!CurrentlyLazy
                && ParentObject?.CurrentCell?.ParentZone == E.Cell.ParentZone
                && QuantumAir.ShouldBeAir(
                    QuantumAirObject: ParentObject,
                    SolidifyingBlueprint: SolidifyingBlueprint,
                    CellBelow: CellBelow,
                 CurrentlyLazy: ref CurrentlyLazy,
                    DerivativeSolidifies: DerivativeSolidifies,
                    Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ObjectEnteringCellEvent E)
        {
            if (!CurrentlyLazy
                && ParentObject?.CurrentCell?.ParentZone == E.Cell.ParentZone
                && QuantumAir.ShouldBeAir(
                    QuantumAirObject: ParentObject,
                    SolidifyingBlueprint: SolidifyingBlueprint,
                    CellBelow: CellBelow,
                    CurrentlyLazy: ref CurrentlyLazy,
                    DerivativeSolidifies: DerivativeSolidifies,
                    Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneActivatedEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone
                && QuantumAir.ShouldBeAir(
                    QuantumAirObject: ParentObject,
                    SolidifyingBlueprint: SolidifyingBlueprint,
                    CellBelow: CellBelow,
                    CurrentlyLazy: ref CurrentlyLazy,
                    DerivativeSolidifies: DerivativeSolidifies,
                    Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneThawedEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone
                && QuantumAir.ShouldBeAir(
                    QuantumAirObject: ParentObject,
                    SolidifyingBlueprint: SolidifyingBlueprint,
                    CellBelow: CellBelow,
                    CurrentlyLazy: ref CurrentlyLazy,
                    DerivativeSolidifies: DerivativeSolidifies,
                    Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(BeforeZoneBuiltEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone
                && QuantumAir.ShouldBeAir(
                    QuantumAirObject: ParentObject,
                    SolidifyingBlueprint: SolidifyingBlueprint,
                    CellBelow: CellBelow,
                    CurrentlyLazy: ref CurrentlyLazy,
                    DerivativeSolidifies: DerivativeSolidifies,
                    Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
    }
}