﻿using System;
using System.Collections.Generic;

using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts.Mutation;

using UD_SacredWellHole;
using XRL.World.ZoneBuilders;
using static UD_SacredWellHole.Const;
using static UD_SacredWellHole.Options;

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

        public SolidAir()
        {
            AirMaterial = null;
            AirObject = null;
            SolidifyingBlueprint = null;
            DerivativeSolidifies = true;
        }
        public SolidAir(GameObject AirObject, string AirMaterial = null, string SolidifyingBlueprint = null, bool DerivativeSolidifies = true)
            : this()
        {
            this.AirMaterial = AirMaterial;
            this.AirObject = AirObject;
            this.SolidifyingBlueprint = SolidifyingBlueprint;
            this.DerivativeSolidifies = DerivativeSolidifies;
        }
        public SolidAir(QuantumAir QuantumAir)
            : this(QuantumAir.ParentObject, QuantumAir.ParentObject.Blueprint, QuantumAir.SolidifyingBlueprint, QuantumAir.DerivativeSolidifies)
        {
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
                && QuantumAir.ShouldBeAir(ParentObject, SolidifyingBlueprint, DerivativeSolidifies, Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ObjectEnteringCellEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Cell.ParentZone 
                && QuantumAir.ShouldBeAir(ParentObject, SolidifyingBlueprint, DerivativeSolidifies, Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneActivatedEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone 
                && QuantumAir.ShouldBeAir(ParentObject, SolidifyingBlueprint, DerivativeSolidifies, Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneThawedEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone 
                && QuantumAir.ShouldBeAir(ParentObject, SolidifyingBlueprint, DerivativeSolidifies, Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(BeforeZoneBuiltEvent E)
        {
            if (ParentObject?.CurrentCell?.ParentZone == E.Zone 
                && QuantumAir.ShouldBeAir(ParentObject, SolidifyingBlueprint, DerivativeSolidifies, Source: nameof(SolidAir)))
            {
                CollapseAir(E);
            }
            return base.HandleEvent(E);
        }

        public bool CollapseAir(MinEvent FromEvent = null)
        {
            int indent = Debug.LastIndent;
            if (ParentObject != null)
            {
                Debug.Entry(4,
                    $"* {nameof(SolidAir)}."
                    + $"{nameof(CollapseAir)}("
                    + $"{nameof(Zone.Z)}: {ParentObject?.CurrentCell?.ParentZone?.Z}, "
                    + $"{nameof(Cell)}: [{ParentObject?.CurrentCell?.Location}], "
                    + $"{nameof(FromEvent)}: {FromEvent?.GetType()?.Name})",
                    Indent: indent + 1, Toggle: getDoDebug());

                if (AirMaterial.IsNullOrEmpty())
                {
                    AirMaterial = "QuantumAir";
                }
                Debug.LoopItem(4, $"{nameof(AirMaterial)}", $"{AirMaterial}", Indent: indent + 2, Toggle: getDoDebug());
                AirObject ??= GameObjectFactory.Factory.CreateUnmodifiedObject(AirMaterial); 
                if (!AirObject.TryGetPart(out QuantumAir quantumAir))
                {
                    quantumAir = AirObject.AddPart(new QuantumAir(this));
                }
                if (AirObject != null)
                {
                    Debug.CheckYeh(4, $"Collapsing...", Indent: indent + 2, Toggle: getDoDebug());
                    Cell currentCell = ParentObject.CurrentCell;
                    currentCell.RemoveObject(ParentObject, System: true, Silent: true, ParentEvent: FromEvent);
                    currentCell.AddObject(AirObject, System: true, Silent: true, ParentEvent: FromEvent);

                    Debug.LastIndent = indent;
                    return true;
                }
                else
                {
                    Debug.CheckNah(4, $"Collapse failed...", Indent: indent + 2, Toggle: getDoDebug());
                }
            }
            Debug.LastIndent = indent;
            return false;
        }
    }
}