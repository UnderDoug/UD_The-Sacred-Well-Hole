using System;
using System.Collections.Generic;

using XRL;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.ObjectBuilders;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;
using XRL.World.WorldBuilders;
using XRL.World.ZoneBuilders;

namespace UD_SacredWellHole
{
    [HasModSensitiveStaticCache]
    public static class Options
    {
        private static string GetOption(string ID, string Default = "")
        {
            return XRL.UI.Options.GetOption(ID, Default);
        }

        private static string Label(string Option = null)
        {
            string Label = "Option_UD_SacredWellHole";
            if (Option == null)
                return Label;
            return $"{Label}_{Option}";
        }
        private static Dictionary<string, string> Directory => new()
        {
            { nameof(DebugVerbosity), Label("DebugVerbosity") },
            { nameof(DebugIncludeInMessage), Label("DebugIncludeInMessage") },
        };

        private static string GetStringOption(string ID, string Default = "")
        {
            if (Directory.ContainsKey(ID))
            {
                return XRL.UI.Options.GetOption(Directory[ID], Default: Default);
            }
            return Default;
        }
        private static bool GetBoolOption(string ID, bool Default = false)
        {
            return GetStringOption(ID, Default ? "Yes" : "No").EqualsNoCase("Yes");
        }
        private static int GetIntOption(string ID, int Default = 0)
        {
            return int.Parse(GetStringOption(ID, $"{Default}"));
        }

        private static void SetBoolOption(string ID, bool Value)
        {
            if (Directory.ContainsKey(ID))
                XRL.UI.Options.SetOption(Directory[ID], Value);
        }
        private static void SetStringOption(string ID, string Value)
        {
            if (Directory.ContainsKey(ID))
                XRL.UI.Options.SetOption(Directory[ID], Value);
        }
        private static void SetIntOption(string ID, int Value)
        {
            SetStringOption(Directory[ID], $"{Value}");
        }

        public static bool doDebug = true;
        public static Dictionary<string, bool> classDoDebug = new()
        {
            // Helper
            { nameof(Extensions), true },

            // General
            { nameof(UD_SubStiltWorldBuilderExtension), true },
            { nameof(UD_SubGrandCathedralBuilder), true },
            { nameof(UD_SubGrandCathedralScrapifier), true },
            { nameof(UDSW_SolidMaterial), true },
            { nameof(UDSW_MostlySolidMaterial), true },
            { nameof(QuantumAir), true },
            { nameof(SolidAir), true },
        };

        public static bool getClassDoDebug(string Class)
        {
            if (classDoDebug.ContainsKey(Class)) 
                return classDoDebug[Class];

            return doDebug;
        }

        // Debug Settings
        public static int DebugVerbosity
        {
            get => GetIntOption(nameof(DebugVerbosity), 0);
            set => SetIntOption(nameof(DebugVerbosity), value);
        }
        public static bool DebugIncludeInMessage
        {
            get => GetBoolOption(nameof(DebugIncludeInMessage), false);
            set => SetBoolOption(nameof(DebugIncludeInMessage), value);
        }

        /*
        // OnClick Handlers
        public static bool OnOptionManagedVanilla()
        {
            Debug.Entry(4, $"@ {nameof(Options)}.{nameof(OnOptionManagedVanilla)}", Indent: 0);
            ManagedVanillaMutationOptionHandler();
            Debug.Entry(4, $"x {nameof(Options)}.{nameof(OnOptionManagedVanilla)} @//", Indent: 0);
            return true;
        }
        */

    } //!-- public static class Options
}
