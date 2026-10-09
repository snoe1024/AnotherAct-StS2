using AnotherAct.AnotherActCode.Models.Acts;
using AnotherAct.AnotherActCode.Models.Monsters;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace AnotherAct.AnotherActCode.Models.Encounters;

public sealed class MineSlimesNormal() : CustomEncounterModel(RoomType.Monster)
{
    public override bool IsValidForAct(ActModel act) => act is Mine;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Slimes];

    public override float GetCameraScaling()
    {
        return 0.9f;
    }

    public override Vector2 GetCameraOffset()
    {
        return Vector2.Down * 50f;
    }

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<RockSlimeS>(),
        ModelDb.Monster<ClaySlimeS>(),
        ModelDb.Monster<RockSlimeM>(),
        ModelDb.Monster<ClaySlimeM>()
    ];
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var flag = Rng.NextBool();
        MonsterModel monsterModel = flag ? ModelDb.Monster<RockSlimeS>() : ModelDb.Monster<ClaySlimeS>();
        MonsterModel monsterModel2 = flag ? ModelDb.Monster<ClaySlimeS>() : ModelDb.Monster<RockSlimeS>();
        return [
            (ModelDb.Monster<ClaySlimeM>().ToMutable(), null),
            (ModelDb.Monster<RockSlimeM>().ToMutable(), null),
            (monsterModel.ToMutable(), null),
            (monsterModel2.ToMutable(), null)
        ];
    }
}