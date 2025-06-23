using HarmonyLib;

using System;

using XRL.World;
using XRL.World.Parts;

namespace UD_SacredWellHole.Harmony
{
    [HarmonyPatch]
    public static class StiltWell_Patches
    {
        [HarmonyPatch(
            declaringType: typeof(StiltWell), 
            methodName: nameof(StiltWell.GetArtifactReputationValue),
            argumentTypes: new Type[] { typeof(GameObject) },
            argumentVariations: new ArgumentType[] { ArgumentType.Normal })]
        [HarmonyPostfix]
        public static void GetArtifactReputationValue_RejectAlreadySacrificed_Postfix(GameObject obj, ref int __result)
        {
            // invert the rep value for any objects found in the layers below the well.
            // this effectively blocks them from being sacrificed but anything else that
            // might query this number will now get a negative result.
            if (obj != null && obj.HasPropertyOrTag("Stilt Well Sacrifice") && __result > 0)
            {
                __result = -__result;
            }
        }
    }
}
