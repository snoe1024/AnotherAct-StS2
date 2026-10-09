using AnotherAct.AnotherActCode.Extensions;
using AnotherAct.AnotherActCode.Models.Acts;
using AnotherAct.AnotherActCode.Models.Monsters;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace AnotherAct.AnotherActCode.Models.Encounters;

public class StoneShroomNormal() : CustomEncounterModel(RoomType.Monster, true)
{
    public override bool IsValidForAct(ActModel act) => act is Mine;

    public override string? CustomScenePath => $"encounters/stone_shroom_normal".ScenePath();
    
    public override IReadOnlyList<string> Slots => [ "mini1", "mini2", "mini3", "mini4", "stoneshroom" ];

    // TODO: remove this
    public override bool IsWeak => true;
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<StoneShroom>(),
        ModelDb.Monster<MiniShroom>()
    ];

    public override float GetCameraScaling()
    {
        return 0.9f;
    }

    public override Vector2 GetCameraOffset()
    {
        return Vector2.Down * 32f + Vector2.Left * 64f;
    }

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)> { (ModelDb.Monster<StoneShroom>().ToMutable(), "stoneshroom") };
    }
}