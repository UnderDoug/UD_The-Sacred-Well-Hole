﻿using System;
using System.Collections.Generic;

using Qud.API;

using XRL;
using XRL.UI;
using XRL.Core;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Skill;
using XRL.World.Parts.Mutation;
using XRL.World.ObjectBuilders;
using XRL.Wish;

using UD_SacredWellHole;
using static UD_SacredWellHole.Const;
using Debug = UD_SacredWellHole.Debug;
using Options = UD_SacredWellHole.Options;

namespace UD_SacredWellHole
{
    [HasWishCommand]
    public static class Debug
    {
        private static int VerbosityOption => Options.DebugVerbosity;
        // Verbosity translates in roughly the following way:
        // 0 : Critical. Use sparingly, if at all, as they show up without the option. Move these to 1 when pushing to main.
        // 1 : Show. Initial debugging entries. Broad, general "did it happen?" style entries for basic trouble-shooting.
        // 2 : Verbose. Entries have more information in them, indicating how values are passed around and changed.
        // 3 : Very Verbose. Entries in more locations, or after fewer steps. These contribute to tracing program flow.
        // 4 : Maximally Verbose. Just like, all of it. Every step of a process, as much detail as possible.

        private static bool IncludeInMessage => Options.DebugIncludeInMessage;

        public static int LastIndent = 0;

        private static void Message(string Text)
        {
            XRL.Messages.MessageQueue.AddPlayerMessage("{{Y|" + Text + "}}");
        }

        private static void Log(string Text)
        {
            UnityEngine.Debug.LogError(Text);
        }

        public static void Entry(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Debug.Indent(Verbosity, Text, Indent, Toggle: Toggle);
        }

        public static void Entry(string Text, int Indent = 0, bool Toggle = true)
        {
            int Verbosity = 0;
            Debug.Indent(Verbosity, Text, Indent, Toggle: Toggle);
        }

        public static void Entry(int Verbosity, string Label, string Text, int Indent = 0, bool Toggle = true)
        {
            string output = Label + ": " + Text;
            Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        public static void Indent(int Verbosity, string Text, int Spaces = 0, bool Toggle = true)
        {
            if (Verbosity > VerbosityOption || !Toggle) return;
            int factor = 4;
            // NBSP  \u00A0
            // Space \u0020
            string space = "\u0020";
            string indent = "";
            for (int i = 0; i < Spaces * factor; i++)
            {
                indent += space;
            }
            LastIndent = Spaces;
            string output = indent + Text;
            Log(output);
            if (IncludeInMessage)
                Message(output);
        }

        public static void Divider(int Verbosity = 0, string String = null, int Count = 60, int Indent = 0, bool Toggle = true)
        {
            string output = "";
            if (String == null) String = "\u003D"; // =
            else String = String[..1];
            for (int i = 0; i < Count; i++)
            {
                output += String;
            }
            Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        public static void Header(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            string divider = "\u2550"; // ═ (box drawing, double horizontal)
            Divider(Verbosity, divider, Toggle: Toggle);
            string output = "@START: " + ClassName + "." + MethodName;
            Entry(Verbosity, output, Toggle: Toggle);
        }
        public static void Footer(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            string divider = "\u2550"; // ═ (box drawing, double horizontal)
            string output = "///END: " + ClassName + "." + MethodName + " !//";
            Entry(Verbosity, output, Toggle: Toggle);
            Divider(Verbosity, divider, Toggle: Toggle);
        }

        public static void DiveIn(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Divider(Verbosity, HONLY, 25, Indent + 1, Toggle: Toggle); // > "\u003E"
            Entry(Verbosity, Text, Indent + 1, Toggle: Toggle);
        }
        public static void DiveOut(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Entry(Verbosity, Text, Indent + 1, Toggle: Toggle);
            // Divider(Verbosity, "\u003C", 25, Indent, Toggle: Toggle); // <
        }

        public static void Warn(int Verbosity, string ClassName, string MethodName, string Issue = null, int Indent = 0)
        {
            string noIssue = "Something didn't go as planned";
            string output = $">!< WARN | {ClassName}.{MethodName}: {Issue ?? noIssue}";
            Entry(Verbosity, output, Indent, Toggle: true);
        }

        public static void LoopItem(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = null, bool Toggle = true)
        {
            string good = TICK;  // √
            string bad = CROSS;  // X
            string goodOrBad = string.Empty;
            if (Good != null) goodOrBad = ((bool)Good ? good : bad) + "\u005D "; // ]
            string output = Text != string.Empty ? Label + ": " + Text : Label;
            Entry(Verbosity, "\u005B" + goodOrBad + output, Indent, Toggle: Toggle);
        }
        public static void CheckYeh(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            LoopItem(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        public static void CheckNah(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = false, bool Toggle = true)
        {
            LoopItem(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        public static void TreeItem(int Verbosity, string Label, string Text = "", bool Last = false, int Branch = 0, int Distance = 0, int Indent = 0, bool Toggle = true)
        {
            // ITEM: "├── "
            // BRAN: "│   "
            // LAST: "└── "
            // DIST: "    "

            string Output = string.Empty;
            for (int i = 0; i < Branch; i++)
            {
                Output += BRAN;
            }
            for (int i = 0; i < Distance; i++)
            {
                Output += DIST;
            }
            Output += Last ? LAST : ITEM;

            Output += Text != string.Empty ? Label + ": " + Text : Label;

            Entry(Verbosity, Output, Indent, Toggle: Toggle);
        }
        public static void TreeLast(int Verbosity, string Label, string Text = "", int Branch = 0, int Distance = 0, int Indent = 0, bool Toggle = true)
        {
            TreeItem(Verbosity, Label, Text, true, Branch, Distance, Indent, Toggle: Toggle);
        }

        // Class Specific Debugs
        public static void Vomit(int Verbosity, string Source, string Context = null, int Indent = 0, bool Toggle = true)
        {
            string context = Context == null ? "" : $"{Context}:";
            Entry(Verbosity, $"% Vomit: {Source} {context}", Indent, Toggle: Toggle);
        }

        public static MeleeWeapon Vomit(this MeleeWeapon MeleeWeapon, int Verbosity, string Title = null, List<string> Categories = null, int Indent = 0, bool Toggle = true)
        {
            int indent = Indent;
            Vomit(Verbosity, MeleeWeapon.ParentObject.DebugName, Title, Indent, Toggle);
            List<string> @default = new()
            {
                "Damage",
                "Combat",
                "Render",
                "etc"
            };
            Categories ??= @default;
            indent++;
            foreach (string category in Categories)
            {
                if (@default.Contains(category)) Entry(Verbosity, $"{category}", Indent: indent, Toggle: Toggle);
                indent++;
                switch (category)
                {
                    case "Damage":
                        LoopItem(Verbosity, "BaseDamage", $"{MeleeWeapon.BaseDamage}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "MaxStrengthBonus", $"{MeleeWeapon.MaxStrengthBonus}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "HitBonus", $"{MeleeWeapon.HitBonus}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "PenBonus", $"{MeleeWeapon.PenBonus}", Indent: indent, Toggle: Toggle);
                        break;
                    case "Combat":
                        LoopItem(Verbosity, "Stat", $"{MeleeWeapon.Stat}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "Skill", $"{MeleeWeapon.Skill}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "Slot", $"{MeleeWeapon.Slot}", Indent: indent, Toggle: Toggle);
                        break;
                    case "Render":
                        Render Render = MeleeWeapon.ParentObject.Render;
                        LoopItem(Verbosity, "DisplayName", $"{Render.DisplayName}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "Tile", $"{Render.Tile}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "ColorString", $"{Render.ColorString}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "DetailColor", $"{Render.DetailColor}", Indent: indent, Toggle: Toggle);
                        break;
                    case "etc":
                        LoopItem(Verbosity, "Ego", $"{MeleeWeapon.Ego}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "IsEquippedOnPrimary", $"{MeleeWeapon.IsEquippedOnPrimary()}", Indent: indent, Toggle: Toggle);
                        LoopItem(Verbosity, "IsImprovisedWeapon", $"{MeleeWeapon.IsImprovisedWeapon()}", Indent: indent, Toggle: Toggle);
                        break;
                }
                indent--;
            }
            return MeleeWeapon;
        }

        public static bool WasEventHandlerRegistered<H, E>(this XRLGame Game, bool Toggle = true)
            where H : IEventHandler
            where E : MinEvent, new()
        {
            bool flag = false;
            E e = new();
            if (Game != null && Game.RegisteredEvents.ContainsKey(e.ID))
            {
                Entry(2, $"Registered", $"{typeof(H).Name} ({typeof(E).Name}.ManagerID: {e.ID})", Indent: 2, Toggle: Toggle);
                flag = true;
            }
            else if (Game != null)
            {
                Entry(2, $"Failed to register {typeof(H).Name} ({typeof(E).Name}.ManagerID: {e.ID})", Indent: 2, Toggle: Toggle);
            }
            else
            {
                Entry(2, $"The.Game null, couldn't register {typeof(H).Name} ({typeof(E).Name}.ManagerID: {e.ID})", Indent: 2, Toggle: Toggle);
            }
            return flag;
        }
        public static bool WasModEventHandlerRegistered<H, E>(this XRLGame Game, bool Toggle = true)
            where H : IEventHandler, IModEventHandler<E>
            where E : MinEvent, new()
        {
            return Game.WasEventHandlerRegistered<H, E>(Toggle: Toggle);
        }

        public static string Vomit(this string @string, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, int Indent = 0, bool Toggle = true)
        {
            string Output = Label != "" ? $"{Label}: {@string}" : @string;
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            return @string;
        }
        public static int Vomit(this int @int, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, int Indent = 0, bool Toggle = true)
        {
            string Output = Label != "" ? $"{Label}: {@int}" : $"{@int}";
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            return @int;
        }
        public static bool Vomit(this bool @bool, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, int Indent = 0, bool Toggle = true)
        {
            string Output = Label != "" ? $"{Label}: {@bool}" : $"{@bool}";
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            return @bool;
        }
        public static List<T> Vomit<T>(this List<T> List, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, string DivAfter = "", int Indent = 0, bool Toggle = true)
            where T : Type
        {
            string Output = Label != "" ? $"{Label}: {nameof(List)}" : $"{nameof(List)}";
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            foreach (T item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, item.ToString(), Good: Good, Indent: Indent+1, Toggle: Toggle);
                else Entry(Verbosity, item.ToString(), Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<object> Vomit(this List<object> List, int Verbosity, string Label, bool LoopItem = false, bool? Good = null, string DivAfter = "", int Indent = 0, bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (object item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static DieRoll Vomit(this DieRoll DieRoll, int Verbosity, string Label = "", bool LoopItem = false, bool? Good = null, int Indent = 0, bool Toggle = true)
        {
            string dieRoll = $"{DieRoll} ({DieRoll.Min()}, {DieRoll.Average()}, {DieRoll.Max()})";
            string Output = Label != "" ? $"{Label}: {dieRoll}" : dieRoll;
            if (LoopItem) Debug.LoopItem(Verbosity, Output, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Output, Indent: Indent, Toggle: Toggle);
            return DieRoll;
        }
        public static List<MutationEntry> Vomit(this List<MutationEntry> List, int Verbosity, string Label, bool LoopItem = false, bool? Good = null, string DivAfter = "", int Indent = 0, bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (MutationEntry item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.Mutation.Name}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.Mutation.Name}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<BaseMutation> Vomit(this List<BaseMutation> List, int Verbosity, string Label, bool LoopItem = false, bool? Good = null, string DivAfter = "", int Indent = 0, bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (BaseMutation item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.GetMutationClass()}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.GetMutationClass()}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<MutationCategory> Vomit(this List<MutationCategory> List, int Verbosity, string Label, bool LoopItem = false, bool? Good = null, string DivAfter = "", int Indent = 0, bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (MutationCategory item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.Name}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.Name}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }
        public static List<GameObject> Vomit(this List<GameObject> List, int Verbosity, string Label, bool LoopItem = false, bool? Good = null, string DivAfter = "", int Indent = 0, bool Toggle = true)
        {
            if (LoopItem) Debug.LoopItem(Verbosity, Label, Good: Good, Indent: Indent, Toggle: Toggle);
            else Entry(Verbosity, Label, Indent: Indent, Toggle: Toggle);
            foreach (GameObject item in List)
            {
                if (LoopItem) Debug.LoopItem(Verbosity, $"{item.DebugName}", Good: Good, Indent: Indent + 1, Toggle: Toggle);
                else Entry(Verbosity, $"{item.DebugName}", Indent: Indent + 1, Toggle: Toggle);
            }
            if (DivAfter != "") Divider(4, DivAfter, 25, Indent: Indent + 1, Toggle: Toggle);
            return List;
        }

        public static void InheritanceTree(GameObject Object, bool Toggle = true)
        {
            GameObjectBlueprint objectBlueprint = Object.GetBlueprint();

            Entry(4, $"objectBlueprint: {objectBlueprint.Name}", Indent: 0, Toggle: Toggle);
            GameObjectBlueprint shallowParent = objectBlueprint.ShallowParent;
            while (shallowParent != null)
            {
                Entry(4, $"shallowParent: {shallowParent.Name}", Indent: 0, Toggle: Toggle);
                shallowParent = shallowParent.ShallowParent;
            }
        }

        [WishCommand]
        public static void ToggleCellHighlighting()
        {
            The.Game.SetBooleanGameState(DEBUG_HIGHLIGHT_CELLS, !The.Game.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS));
        }
        [WishCommand]
        public static void debug_ToggleCH()
        {
            ToggleCellHighlighting();
        }

        [WishCommand]
        public static void RemoveCellHighlighting()
        {
            foreach (GameObject @object in The.ActiveZone.GetObjects())
            {
                UDSW_CellHighlighter highlighter = @object.RequirePart<UDSW_CellHighlighter>();
                @object.RemovePart(highlighter);
            }
        }
        public static Cell HighlightColor(this Cell Cell, string TileColor, string DetailColor, string BackgroundColor = "^k", int Priority = 0)
        {
            if (!The.Game.HasBooleanGameState(DEBUG_HIGHLIGHT_CELLS))
                The.Game.SetBooleanGameState(DEBUG_HIGHLIGHT_CELLS, Options.DebugVerbosity > 3);
            if (Cell.IsEmpty() && Cell.GetFirstVisibleObject() == null && Cell.GetHighestRenderLayerObject() == null)
                Cell.AddObject("Cell Highlighter");

            GameObject gameObject = null;
            foreach (GameObject Object in Cell.GetObjects())
            {
                gameObject ??= Object;
                if (Object.Render.RenderLayer >= gameObject.Render.RenderLayer)
                    gameObject = Object;
            }
            gameObject = Cell.GetHighestRenderLayerObject();
            UDSW_CellHighlighter highlighter = gameObject.RequirePart<UDSW_CellHighlighter>();
            if (Priority >= highlighter.HighlightPriority)
            {
                highlighter.HighlightPriority = Priority;
                highlighter.TileColor = TileColor;
                highlighter.DetailColor = DetailColor;
                highlighter.BackgroundColor = BackgroundColor;
            }
            return Cell;
        }
        public static Cell HighlightRed(this Cell Cell, int Priority = 0)
        {
            return Cell.HighlightColor(TileColor: "&r", DetailColor: "R", BackgroundColor: "^k", Priority);
        }
        public static Cell HighlightGreen(this Cell Cell, int Priority = 0)
        {
            return Cell.HighlightColor(TileColor: "&g", DetailColor: "G", BackgroundColor: "^k", Priority);
        }
        public static Cell HighlightYellow(this Cell Cell, int Priority = 0)
        {
            return Cell.HighlightColor(TileColor: "&w", DetailColor: "W", BackgroundColor: "^k", Priority);
        }
        public static Cell HighlightPurple(this Cell Cell, int Priority = 0)
        {
            return Cell.HighlightColor(TileColor: "&m", DetailColor: "M", BackgroundColor: "^k", Priority);
        }
        public static Cell HighlightBlue(this Cell Cell, int Priority = 0)
        {
            return Cell.HighlightColor(TileColor: "&b", DetailColor: "B", BackgroundColor: "^k", Priority);
        }
        public static Cell HighlightCyan(this Cell Cell, int Priority = 0)
        {
            return Cell.HighlightColor(TileColor: "&c", DetailColor: "C", BackgroundColor: "^k", Priority);
        }

        [WishCommand]
        public static void ToggleObjectCreationAnalysis()
        {
            The.Game.SetBooleanGameState(DEBUG_OBJECT_CREATION_ANALYSIS, !The.Game.GetBooleanGameState(DEBUG_OBJECT_CREATION_ANALYSIS));
        }

        [WishCommand]
        public static void debug_ToggleOCA()
        {
            ToggleObjectCreationAnalysis();
        }

    } //!-- public static class Debug
}

namespace XRL.World.Parts
{
    [Serializable]
    public class UDSW_CellHighlighter : IScribedPart
    {
        public static readonly int ICON_COLOR_PRIORITY = 999;

        public string TileColor;
        public string DetailColor;
        public string BackgroundColor;

        public int HighlightPriority;

        public bool DoHighlight;

        public UDSW_CellHighlighter()
        {
            BackgroundColor = "k";
            DoHighlight = 
                Options.DebugVerbosity > 3
             && The.Game.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS);
            HighlightPriority = 0;
        }

        public override bool Render(RenderEvent E)
        {
            if ((XRLCore.FrameTimer.ElapsedMilliseconds & 0x7F) == 0L)
            {
                DoHighlight =
                    Options.DebugVerbosity > 3 
                 && The.Game.GetBooleanGameState(DEBUG_HIGHLIGHT_CELLS);
            }
            if (DoHighlight)
            {
                if (ParentObject.InheritsFrom(nameof(UDSW_CellHighlighter)))
                    ParentObject.Render.Visible = true;

                E.ApplyColors(
                    Foreground: TileColor ?? E.DetailColor, 
                    Background: BackgroundColor, 
                    Detail: DetailColor ?? E.DetailColor,
                    ICON_COLOR_PRIORITY, 
                    ICON_COLOR_PRIORITY, 
                    ICON_COLOR_PRIORITY);
            }
            else
            {
                if (ParentObject.InheritsFrom(nameof(UDSW_CellHighlighter)))
                    ParentObject.Render.Visible = false;
            }
            return base.Render(E);
        }

        public override void Remove()
        {
            if (ParentObject != null && ParentObject.InheritsFrom(nameof(UDSW_CellHighlighter)))
            {
                ParentObject.Obliterate();
            }
            base.Remove();
        }
    }
}