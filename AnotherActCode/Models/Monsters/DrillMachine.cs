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

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class DrillMachine : CustomMonsterModel
{
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 59, 56);
    
    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 61, 58);

    private int DrillDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    
    private const int VulnerableAmount = 2;
    private const int GrowAmount = 2;

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<ImpactDrillPower>(new ThrowingPlayerChoiceContext(), Creature, VulnerableAmount, Creature, null);
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("DRILL_MOVE", DrillMove, new SingleAttackIntent(DrillDamage));
        var moveState2 = new MoveState("DRILL_MOVE_2", DrillMove, new SingleAttackIntent(DrillDamage));
        var moveState3 = new MoveState("POWER_UP_MOVE", PowerUpMove, new BuffIntent());
        moveState1.FollowUpState = moveState2;
        moveState2.FollowUpState = moveState3;
        moveState3.FollowUpState = moveState1;
        
        var list = new List<MonsterState>() { moveState1, moveState2, moveState3 };
        return new MonsterMoveStateMachine(list, moveState1);
    }

    private async Task DrillMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(DrillDamage).FromMonster(this).WithAttackerAnim("Attack", 0.6f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task PowerUpMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, GrowAmount, Creature, null);
    }
}