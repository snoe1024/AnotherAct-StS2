using System.Runtime.InteropServices;
using AnotherAct.AnotherActCode.Models.Acts;
using AnotherAct.AnotherActCode.Models.Monsters;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace AnotherAct.AnotherActCode.Models.Encounters;

public class RockGolemNormal() : CustomEncounterModel(RoomType.Monster)
{
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return [(ModelDb.Monster<RockGolem>().ToMutable(), null)];
    }

    public override bool IsValidForAct(ActModel act) => act is Mine;
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<RockGolem>()];
}