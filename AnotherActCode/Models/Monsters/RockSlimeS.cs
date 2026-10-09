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
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class RockSlimeS : CustomMonsterModel
{
    private const int GoopAmount = 1;

    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 11);
    
    public override int MaxInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 16, 15);

    private int TackleDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);

    private int BlockValue =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 3, 2);

    public override DamageSfxType TakeDamageSfxType => DamageSfxType.Slime;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<WithdrawnPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("GOOP_TACKLE_MOVE", GoopTackleMove, new SingleAttackIntent(TackleDamage), new StatusIntent(GoopAmount));
        var moveState2 = new MoveState("GOOP_SHIELD_MOVE", GoopShieldMove, new DefendIntent(), new StatusIntent(GoopAmount));
        var moveState3 = new MoveState("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage));
        var moveState4 = new MoveState("SHIELD_MOVE", ShieldMove, new DefendIntent());
        // Randomize only the opening move; subsequent moves alternate attack and defense.
        moveState1.FollowUpState = moveState2;
        moveState2.FollowUpState = moveState1;
        var randomBranchState = new RandomBranchState("RAND");
        randomBranchState.AddBranch(moveState1, MoveRepeatType.CannotRepeat);
        randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
        moveState3.FollowUpState = moveState2;
        moveState4.FollowUpState = moveState1;
        var list = new List<MonsterState>() { moveState1, moveState2, moveState3, moveState4, randomBranchState };
        return new MonsterMoveStateMachine(list, randomBranchState);
    }

    private async Task GoopTackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, GoopAmount, null);
    }

    private async Task GoopShieldMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(base.Creature, BlockValue, ValueProp.Move, null);
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, GoopAmount, null);
    }

    public async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
    }

    public async Task ShieldMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(base.Creature, BlockValue, ValueProp.Move, null);
    }
}