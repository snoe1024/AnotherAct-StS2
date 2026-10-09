using System.Text.Json;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace AnotherAct.AnotherActCode.Animation;

public static class RockSlimeAnimationTiming
{
    public const string AssetPath = "res://AnotherAct/animations/monsters/rock_slime/motions_trial_06/";
    public const double FastBaseSpeed = 2.4;
    public const double ThrowSpeed = 0.6;
    public const double FastRushSpeed = 1.10;
    private static readonly Dictionary<string, Dictionary<string, double>> Events = new();

    public static double SpeedFor(FastModeType mode, string motion)
    {
        if (motion == "idle_loop") return 1.0;
        if (mode == FastModeType.Instant) return 0.0;
        var speed = mode == FastModeType.Fast ? FastBaseSpeed : 1.0;
        if (motion == "attack") speed *= ThrowSpeed;
        if (motion == "cast" && mode == FastModeType.Fast) speed *= FastRushSpeed;
        return speed;
    }

    public static double GetPlaybackSpeed(string motion) => SpeedFor(SaveManager.Instance.PrefsSave.FastMode, motion);

    public static double EventSeconds(string motion, string eventName)
    {
        if (!Events.TryGetValue(motion, out var events))
        {
            using var json = JsonDocument.Parse(Godot.FileAccess.GetFileAsString(AssetPath + "baked/" + motion + ".json"));
            events = json.RootElement.GetProperty("events").EnumerateObject()
                .ToDictionary(entry => entry.Name, entry => entry.Value.GetDouble());
            Events[motion] = events;
        }
        return events[eventName];
    }

    public static float WaitFor(FastModeType mode, string motion, double assetSeconds)
    {
        var speed = SpeedFor(mode, motion);
        return speed > 0 ? (float)(assetSeconds / speed) : 0f;
    }

    public static float EventWait(FastModeType mode, string motion, string eventName) =>
        WaitFor(mode, motion, EventSeconds(motion, eventName));

    public static float DeathWaitSeconds => EventWait(SaveManager.Instance.PrefsSave.FastMode, "die", "finished");
}
