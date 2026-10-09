using AnotherAct.AnotherActCode.Models.Acts;
using AnotherAct.AnotherActCode.Models.Monsters;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace AnotherAct.AnotherActCode.Models.Encounters;

public class DrillMachineWeak() : CustomEncounterModel(RoomType.Monster)
{
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return [(ModelDb.Monster<DrillMachine>().ToMutable(), null)];
    }

    public override bool IsValidForAct(ActModel act) => act is Mine;

    public override IEnumerable<EncounterTag> Tags => [(EncounterTag)nameof(DrillMachine).GetHashCode()];

    public override bool IsWeak => true;
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<DrillMachine>()];
}