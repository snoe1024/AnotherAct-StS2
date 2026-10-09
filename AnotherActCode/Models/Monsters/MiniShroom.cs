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

public class MiniShroom : CustomMonsterModel
{
    public override int MinInitialHp => 6;
    
    public override int MaxInitialHp => MinInitialHp;

    private int TackleDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);

    private const int DazedAmount = 1;
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("TACKLE_MOVE", TackleMove, new SingleAttackIntent(TackleDamage));
        var moveState2 = new MoveState("DISTRACT_MOVE", DistractMove, new StatusIntent(DazedAmount));
        
        var randomBranchState = (RandomBranchState)(moveState2.FollowUpState = moveState1.FollowUpState = new RandomBranchState("RAND"));
        randomBranchState.AddBranch(moveState1, MoveRepeatType.CannotRepeat);
        randomBranchState.AddBranch(moveState2, MoveRepeatType.CanRepeatForever);
        var list = new List<MonsterState>() { moveState1, moveState2, randomBranchState };
        return new MonsterMoveStateMachine(list, randomBranchState);
    }
    
    private async Task DistractMove(IReadOnlyList<Creature> targets)
    {
        SfxCmd.Play(CastSfx);
        await CreatureCmd.TriggerAnim(base.Creature, "Attack", 0.6f);
        VfxCmd.PlayOnCreatureCenters(targets, "vfx/vfx_attack_blunt");
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, DazedAmount, null);
    }
    
    private async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).FromMonster(this).WithAttackerAnim("Attack", 0.3f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }
}