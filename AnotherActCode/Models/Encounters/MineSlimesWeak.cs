using AnotherAct.AnotherActCode.Models.Acts;
using AnotherAct.AnotherActCode.Models.Monsters;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace AnotherAct.AnotherActCode.Models.Encounters;

public sealed class MineSlimesWeak() : CustomEncounterModel(RoomType.Monster, true)
{
    private static readonly MonsterModel[] _smallSlimes =
    [
        ModelDb.Monster<RockSlimeS>(),
        ModelDb.Monster<ClaySlimeS>()
    ];

    private static readonly MonsterModel[] _mediumSlimes =
    [
        ModelDb.Monster<RockSlimeM>(),
        ModelDb.Monster<ClaySlimeM>()
    ];

    public override bool IsValidForAct(ActModel act) => act is Mine;

    public override IEnumerable<EncounterTag> Tags => [EncounterTag.Slimes];
    
    public override bool IsWeak => true;
    
    public override IEnumerable<MonsterModel> AllPossibleMonsters
    {
        get
        {
            var smallSlimes = _smallSlimes;
            var mediumSlimes = _mediumSlimes;
            var num = 0;
            var array = new MonsterModel[smallSlimes.Length + mediumSlimes.Length];
            var readOnlySpan = new ReadOnlySpan<MonsterModel>(smallSlimes);
            readOnlySpan.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan.Length));
            num += readOnlySpan.Length;
            var readOnlySpan2 = new ReadOnlySpan<MonsterModel>(mediumSlimes);
            readOnlySpan2.CopyTo(new Span<MonsterModel>(array).Slice(num, readOnlySpan2.Length));
            num += readOnlySpan2.Length;
            return new List<MonsterModel>(array).AsReadOnly();
        }
    }
    
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        var list = new List<(MonsterModel, string?)>();
        var list2 = _smallSlimes.ToList();
        MonsterModel monsterModel = base.Rng.NextItem(list2)
            ?? throw new InvalidOperationException("MineSlimesWeak requires a small slime candidate.");
        list2.Remove(monsterModel);
        MonsterModel monsterModel2 = base.Rng.NextItem(list2)
            ?? throw new InvalidOperationException("MineSlimesWeak requires two distinct small slime candidates.");
        MonsterModel mediumSlime = base.Rng.NextItem(_mediumSlimes)
            ?? throw new InvalidOperationException("MineSlimesWeak requires a medium slime candidate.");
        list.Add((monsterModel.ToMutable(), null));
        list.Add((mediumSlime.ToMutable(), null));
        list.Add((monsterModel2.ToMutable(), null));
        return list;
    }
}
