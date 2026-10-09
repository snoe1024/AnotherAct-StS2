using AnotherAct.AnotherActCode.Models.Monsters;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace AnotherAct.AnotherActCode.Models.Encounters;

// Console-only: deliberately excluded from every act's encounter pool.
public sealed class RockSlimeVisualTest() : CustomEncounterModel(RoomType.Monster, false)
{
    public override bool IsValidForAct(ActModel act) => false;
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<RockSlimeM>()];
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [(ModelDb.Monster<RockSlimeM>().ToMutable(), null)];
}
