using AnotherAct.AnotherActCode.Powers;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class StoneShroom : CustomMonsterModel
{
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 55, 51);
    
    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 58, 54);

    private int TackleDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 11, 9);

    private int BlockValue =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 9, 8);
    
    private const int SummonAmount = 2;
    private const int GrowAmount = 2;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage));
        var moveState2 = new MoveState("SUMMON_SHIELD_MOVE", SummonShieldMove, new DefendIntent(), new SummonIntent());
        var moveState3 = new MoveState("POWER_UP_MOVE", PowerUpMove, new BuffIntent());
        moveState1.FollowUpState = moveState2;
        moveState2.FollowUpState = moveState3;
        moveState3.FollowUpState = moveState1;
        
        var list = new List<MonsterState>() { moveState1, moveState2, moveState3 };
        return new MonsterMoveStateMachine(list, moveState1);
    }
    
    private async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    private async Task SummonShieldMove(IReadOnlyList<Creature> targets)
    {
        for (var i = 0; i < SummonAmount; i++)
        {
            var text = base.CombatState.Encounter?.Slots.LastOrDefault(s => CombatState.Enemies.All(c => c.SlotName != s));
            if (text == null) continue;
            
            var target = await CreatureCmd.Add<MiniShroom>(CombatState, text);
            await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), target, 1m, Creature, null);
        }
        await CreatureCmd.GainBlock(Creature, BlockValue, ValueProp.Move, null);
    }

    private async Task PowerUpMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, GrowAmount, Creature, null);
    }
}