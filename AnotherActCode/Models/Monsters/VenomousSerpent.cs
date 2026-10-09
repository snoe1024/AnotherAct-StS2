using AnotherAct.AnotherActCode.Powers;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class VenomousSerpent : CustomMonsterModel
{
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 79, 76);
    
    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 83, 80);

    private bool _isLonely;

    public bool IsLonely
    {
        get => _isLonely;
        set
        {
            AssertMutable();
            _isLonely = value;
        }
    }

    private int TackleDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);
    
    private const int ToxicAmount = 1;
    
    private int RecklessAmountInitial =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 14);
    
    private int RecklessIncreasing =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<RecklessPower>(new ThrowingPlayerChoiceContext(), Creature, RecklessAmountInitial, Creature, null);
    }
    
    private int _starterMoveIdx;
    
    public int StarterMoveIdx
    {
        get => _starterMoveIdx;
        set
        {
            AssertMutable();
            _starterMoveIdx = value;
        }
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage));
        var moveState2 = new MoveState("POWER_UP_MOVE", PowerUpMove, new StatusIntent(ToxicAmount), new BuffIntent());
        var moveState3 = new MoveState("BITE_MOVE", BiteMove, new SingleAttackIntent(TackleDamage), new StatusIntent(ToxicAmount));
        var conditionOnMoveState1 = new ConditionalBranchState("TackleBranch");
        conditionOnMoveState1.AddState(moveState3, () => _isLonely);
        conditionOnMoveState1.AddState(moveState2, () => !_isLonely);
        var conditionOnMoveState2 = new ConditionalBranchState("PowerUpBranch");
        conditionOnMoveState2.AddState(moveState3, () => _isLonely);
        conditionOnMoveState2.AddState(moveState1, () => !_isLonely);
        moveState1.FollowUpState = conditionOnMoveState1;
        moveState2.FollowUpState = conditionOnMoveState2;
        moveState3.FollowUpState = moveState3;
        
        var list = new List<MonsterState>() { moveState1, moveState2, moveState3, conditionOnMoveState1, conditionOnMoveState2 };
        return new MonsterMoveStateMachine(list, StarterMoveIdx % 2 is 0 ? moveState1 : moveState2);
    }
    
    private async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    private async Task PowerUpMove(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Toxic>(targets, PileType.Discard, ToxicAmount, null);
        await PowerCmd.Apply<RecklessPower>(new ThrowingPlayerChoiceContext(), Creature, RecklessIncreasing, Creature, null);
    }

    private async Task BiteMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_bite")
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Toxic>(targets, PileType.Discard, ToxicAmount, null);
    }
    
    public async Task StunnedMove(IReadOnlyList<Creature> targets)
    {
        // await CreatureCmd.TriggerAnim(Creature, "RecklessTrigger", 0.5f);
        await Task.CompletedTask;
    }
    
    public async Task StunnedMoveWithoutAnim(IReadOnlyList<Creature> targets)
    {
        await Task.CompletedTask;
    }

    public static void EnsureOtherStartWithDifferentMoves(IEnumerable<MonsterModel> monsters, Rng rng)
    {
        var enumerable = monsters.OfType<VenomousSerpent>();
        var num = rng.NextInt(3);
        foreach (var item in enumerable)
        {
            item.StarterMoveIdx = num % 2;
            num++;
        }
    }
}