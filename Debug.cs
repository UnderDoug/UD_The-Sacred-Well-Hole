using System;

using XRL.Wish;

using UD_Modding_Toolbox;

namespace UD_SacredWellHole
{
    [HasWishCommand]
    public static class Debug
    {
        private static int VerbosityOption => UD_SacredWellHole.Options.DebugVerbosity;
        private static bool IncludeInMessage => UD_SacredWellHole.Options.DebugIncludeInMessage;

        public static UD_Logger Logger = new(
            ThisMod: Const.ThisMod,
            OptionClass: typeof(UD_SacredWellHole.Options),
            VerbosityOptionField: nameof(UD_SacredWellHole.Options.DebugVerbosity),
            IncludeInMessageOptionField: nameof(UD_SacredWellHole.Options.DebugIncludeInMessage));

        private static int _LastIndent = 0;

        [Obsolete(
            "Prefer " + nameof(Debug) + "." + nameof(GetIndent) + " or " +
            nameof(UD_Logger) + "." + nameof(UD_Logger.GetIndent) +
            "; this property will persist for a while (into 2026).")]
        public static int LastIndent
        {
            get
            {
                Logger.GetIndent(out _LastIndent);
                return _LastIndent;
            }
            set
            {
                _LastIndent = value;
                Logger.SetIndent(value);
            }
        }

        public static void ResetIndent()
        {
            Logger.ResetIndent();
        }
        public static void ResetIndent(out int Indent)
        {
            Logger.ResetIndent(out Indent);
        }
        public static void GetIndent(out int Indent)
        {
            Logger.GetIndent(out Indent);
        }
        public static void SetIndent(int Indent)
        {
            Logger.SetIndent(Indent);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Entry((UD_Logger.Verbosity)Verbosity, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(UD_Logger.Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.Entry(Verbosity, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.Entry(Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(int Verbosity, string Label, string Text, int Indent = 0, bool Toggle = true)
        {
            Entry((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Entry) + "; this method will persist for a while (into 2026).")]
        public static void Entry(UD_Logger.Verbosity Verbosity, string Label, string Text, int Indent = 0, bool Toggle = true)
        {
            string output = Label + ": " + Text;
            Logger.Entry(Verbosity, output, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Divider) + "; this method will persist for a while (into 2026).")]
        public static void Divider(int Verbosity = 0, string String = null, int Count = 60, int Indent = 0, bool Toggle = true)
        {
            Divider((UD_Logger.Verbosity)Verbosity, String, Count, Indent, Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Divider) + "; this method will persist for a while (into 2026).")]
        public static void Divider(UD_Logger.Verbosity Verbosity = 0, string String = null, int Count = 60, int Indent = 0, bool Toggle = true)
        {
            Logger.Divider(Verbosity, String, Count, Indent, Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Header) + "; this method will persist for a while (into 2026).")]
        public static void Header(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Header((UD_Logger.Verbosity)Verbosity, ClassName, MethodName, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Header) + "; this method will persist for a while (into 2026).")]
        public static void Header(UD_Logger.Verbosity Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Logger.Header(Verbosity, ClassName, MethodName, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Footer) + "; this method will persist for a while (into 2026).")]
        public static void Footer(int Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Footer((UD_Logger.Verbosity)Verbosity, ClassName, MethodName, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Footer) + "; this method will persist for a while (into 2026).")]
        public static void Footer(UD_Logger.Verbosity Verbosity, string ClassName, string MethodName, bool Toggle = true)
        {
            Logger.Footer(Verbosity, ClassName, MethodName, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveIn) + "; this method will persist for a while (into 2026).")]
        public static void DiveIn(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            DiveIn((UD_Logger.Verbosity)Verbosity, Text, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveIn) + "; this method will persist for a while (into 2026).")]
        public static void DiveIn(UD_Logger.Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.DiveIn(Verbosity, Text, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveOut) + "; this method will persist for a while (into 2026).")]
        public static void DiveOut(int Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            DiveOut((UD_Logger.Verbosity)Verbosity, Text, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.DiveOut) + "; this method will persist for a while (into 2026).")]
        public static void DiveOut(UD_Logger.Verbosity Verbosity, string Text, int Indent = 0, bool Toggle = true)
        {
            Logger.DiveOut(Verbosity, Text, Indent, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Warn) + "; this method will persist for a while (into 2026).")]
        public static void Warn(int Verbosity, string ClassName, string MethodName, string Issue = null, int Indent = 0)
        {
            Warn((UD_Logger.Verbosity)Verbosity, ClassName, MethodName, Issue, Indent);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.Warn) + "; this method will persist for a while (into 2026).")]
        public static void Warn(UD_Logger.Verbosity Verbosity, string ClassName, string MethodName, string Issue = null, int Indent = 0)
        {
            Logger.Warn(Verbosity, ClassName, MethodName, Issue, Indent);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.LoopItem) + "; this method will persist for a while (into 2026).")]
        public static void LoopItem(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = null, bool Toggle = true)
        {
            LoopItem((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.LoopItem) + "; this method will persist for a while (into 2026).")]
        public static void LoopItem(UD_Logger.Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = null, bool Toggle = true)
        {
            Logger.LoopItem(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckYeh) + "; this method will persist for a while (into 2026).")]
        public static void CheckYeh(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            CheckYeh((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckYeh) + "; this method will persist for a while (into 2026).")]
        public static void CheckYeh(UD_Logger.Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = true, bool Toggle = true)
        {
            Logger.CheckYeh(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void CheckNah(int Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = false, bool Toggle = true)
        {
            CheckNah((UD_Logger.Verbosity)Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void CheckNah(UD_Logger.Verbosity Verbosity, string Label, string Text = "", int Indent = 0, bool? Good = false, bool Toggle = true)
        {
            Logger.CheckNah(Verbosity, Label, Text, Indent, Good, Toggle: Toggle);
        }

        // Class Specific Debugs
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void Vomit(int Verbosity, string Source, string Context = null, int Indent = 0, bool Toggle = true)
        {
            Vomit((UD_Logger.Verbosity)Verbosity, Source, Context, Indent, Toggle: Toggle);
        }
        [Obsolete("Prefer " + nameof(UD_Modding_Toolbox.UD_Logger) + "." + nameof(UD_Modding_Toolbox.UD_Logger.CheckNah) + "; this method will persist for a while (into 2026).")]
        public static void Vomit(UD_Logger.Verbosity Verbosity, string Source, string Context = null, int Indent = 0, bool Toggle = true)
        {
            Logger.Vomit(Verbosity, Source, Context, Indent, Toggle: Toggle);
        }
    }
}