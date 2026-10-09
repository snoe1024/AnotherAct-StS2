using AnotherAct.AnotherActCode.Powers;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class BoulderMite : CustomMonsterModel
{
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 23, 21);
    
    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 26, 24);

    private int TackleDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 9);

    private int BlockValue =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 9, 8);
    
    private const int WeakAmount = 1;
    private const int FrailAmount = 1;
    
    private int StrengthAmount =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
    
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
        var moveState2 = new MoveState("POWER_UP_MOVE", PowerUpMove, new DefendIntent(), new BuffIntent());
        var moveState3 = new MoveState("SHELL_MOVE", ShellMove, new DefendIntent(), new DebuffIntent());
        moveState1.FollowUpState = moveState2;
        moveState2.FollowUpState = moveState3;
        moveState3.FollowUpState = moveState1;
        
        var list = new List<MonsterState>() { moveState1, moveState2, moveState3 };
        return new MonsterMoveStateMachine(list, (StarterMoveIdx % 3) switch
        {
            0 => moveState1, 
            1 => moveState2, 
            _ => moveState3, 
        });
    }
    
    private async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_bite")
            .Execute(null);
        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, WeakAmount, Creature, null);
    }

    private async Task PowerUpMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockValue, ValueProp.Move, null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmount, Creature, null);
    }
    
    private async Task ShellMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, BlockValue, ValueProp.Move, null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, FrailAmount, Creature, null);
    }

    public static void EnsureOtherStartWithDifferentMoves(IEnumerable<MonsterModel> monsters, Rng rng)
    {
        var enumerable = monsters.OfType<BoulderMite>();
        var num = rng.NextInt(3);
        foreach (var item in enumerable)
        {
            item.StarterMoveIdx = num % 3;
            num++;
        }
    }
}