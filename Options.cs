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
    [HasOptionFlagUpdate(Prefix = "Option_UD_SacredWellHole_")]
    public static class Options
    {
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

        public static bool getDoDebug(object what = null, List<object> DoList = null, List<object> DontList = null, bool? DoDebug = null)
        {
            DoList ??= new();
            DontList ??= new();

            if (what != null && !DoList.IsNullOrEmpty() && DoList.Contains(what))
            {
                return true;
            }

            if (what != null && !DontList.IsNullOrEmpty() && DontList.Contains(what))
            {
                return false;
            }

            return DoDebug ?? doDebug;
        }

        public static bool getClassDoDebug(string Class) => classDoDebug.ContainsKey(Class) ? classDoDebug[Class] : doDebug;

        // Debug Settings
        [OptionFlag] public static int DebugVerbosity;
        [OptionFlag] public static bool DebugIncludeInMessage;

        // Checkbox settings
        // [OptionFlag] public static bool ExampleOption;
    }
}
