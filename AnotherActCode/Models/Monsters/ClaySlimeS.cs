using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class ClaySlimeS : CustomMonsterModel
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

    // public override async Task AfterAddedToRoom() { }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("GOOP_MOVE", GoopMove, new StatusIntent(GoopAmount));
        var moveState2 = new MoveState("TACKLE_SHIELD_MOVE", TackleShieldMove, new SingleAttackIntent(TackleDamage), new DefendIntent());
        var randomBranchState = (RandomBranchState)(moveState2.FollowUpState = moveState1.FollowUpState = new RandomBranchState("RAND"));
        randomBranchState.AddBranch(moveState1, MoveRepeatType.CannotRepeat);
        randomBranchState.AddBranch(moveState2, MoveRepeatType.CannotRepeat);
        var list = new List<MonsterState>() { moveState1, moveState2, randomBranchState };
        return new MonsterMoveStateMachine(list, randomBranchState);
    }

    private async Task GoopMove(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Slimed>(targets, PileType.Discard, GoopAmount, null);
    }

    private async Task TackleShieldMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_slime_impact")
            .Execute(null);
        await CreatureCmd.GainBlock(base.Creature, BlockValue, ValueProp.Move, null);
    }
}