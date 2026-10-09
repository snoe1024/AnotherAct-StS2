using System.Runtime.InteropServices;
using AnotherAct.AnotherActCode.Models.Acts;
using AnotherAct.AnotherActCode.Models.Monsters;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace AnotherAct.AnotherActCode.Models.Encounters;

public class BoulderMitesNormal() : CustomEncounterModel(RoomType.Monster, true)
{
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        const int size = 2;
        var list = new List<(MonsterModel, string?)>(size);
        CollectionsMarshal.SetCount(list, size);
        var span = CollectionsMarshal.AsSpan(list);
        span[0] = (ModelDb.Monster<BoulderMite>().ToMutable(), null);
        span[1] = (ModelDb.Monster<BoulderMite>().ToMutable(), null);
        IReadOnlyList<(MonsterModel, string?)> list2 = list;
        BoulderMite.EnsureOtherStartWithDifferentMoves(list2.Select(kvp => kvp.Item1), Rng);
        return list2;
    }

    public override bool IsValidForAct(ActModel act) => act is Mine;

    public override IEnumerable<EncounterTag> Tags => [(EncounterTag)nameof(BoulderMite).GetHashCode()];
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<BoulderMite>()];
}