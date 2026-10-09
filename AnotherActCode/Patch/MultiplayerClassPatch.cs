using AnotherAct.AnotherActCode.Models.Acts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;

namespace AnotherAct.AnotherActCode.Patch;

// ReSharper disable InconsistentNaming
// ReSharper disable SuspiciousTypeConversion.Global

[HarmonyPatch(typeof(StartRunLobby), "GetAct")]
public static class StartRunLobbyGetActPatch
{
    [HarmonyPrefix]
    public static bool Prefix(ref string act1Key, ref ActModel? __result)
    {
        if (act1Key == "mine")
        {
            __result = ModelDb.Act<Mine>();
            return false;
        }

        return true;
    }
}