using AnotherAct.AnotherActCode.Extensions;
using AnotherAct.AnotherActCode.Models.Acts;
using AnotherAct.AnotherActCode.Models.Monsters;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;

namespace AnotherAct.AnotherActCode.Patch;

// ReSharper disable InconsistentNaming
// ReSharper disable SuspiciousTypeConversion.Global

/*
[HarmonyPatch(typeof(MonsterModel), "VisualsPath", MethodType.Getter)]
public class MonsterModelVisualsPathPatch
{
    [HarmonyPrefix]
    public static bool Prefix(MonsterModel __instance, ref string __result)
    {
        if (__instance is AnotherActMonsterModel monster)
        {
            __result = ("creature_visuals/" + __instance.Id.Entry.ToLowerInvariant()).ScenePath();
            
            return false;
        }
        return true;
    }
}
*/