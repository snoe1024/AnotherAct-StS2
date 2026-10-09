using System.Reflection;
using Godot;
using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace AnotherAct.AnotherActCode;

//You're recommended but not required to keep all your code in this package and all your assets in the AnotherAct folder.
[ModInitializer(nameof(Initialize))]
public partial class AnotherActMainFile : Node
{
    public const string ModId = "AnotherAct"; //At the moment, this is used only for the Logger and harmony names.
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);

        var assembly = Assembly.GetExecutingAssembly();
        ScriptManagerBridge.LookupScriptsInAssembly(assembly);
        harmony.PatchAll();
    }
}