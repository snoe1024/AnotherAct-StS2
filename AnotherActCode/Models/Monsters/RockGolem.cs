using AnotherAct.AnotherActCode.Powers;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class RockGolem : CustomMonsterModel
{
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 125, 119);
    
    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 129, 123);

    private int TackleDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 3);
    
    private const int PowerUpValue = 2;
    
    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("GOOP_MOVE", GoopMove, new DebuffIntent());
        var moveState2 = new MoveState("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage), new BuffIntent());
        moveState1.FollowUpState = moveState2;
        moveState2.FollowUpState = moveState2;
        var list = new List<MonsterState>() { moveState1, moveState2 };
        return new MonsterMoveStateMachine(list, moveState1);
    }

    private async Task GoopMove(IReadOnlyList<Creature> targets)
    {
        SfxCmd.Play(CastSfx);
        await CreatureCmd.TriggerAnim(Creature, "Cast", 0.4f);
        await PowerCmd.Apply<TouchOfRockPower>(new ThrowingPlayerChoiceContext(), targets, 1m, Creature, null);
    }
    
    private async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_bite")
            .Execute(null);
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, PowerUpValue, Creature, null);
    }
}