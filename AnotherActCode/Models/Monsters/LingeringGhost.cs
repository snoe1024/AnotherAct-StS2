using AnotherAct.AnotherActCode.Powers;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace AnotherAct.AnotherActCode.Models.Monsters;

public class LingeringGhost : CustomMonsterModel
{
    public override int MinInitialHp =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 13, 12);
    
    public override int MaxInitialHp => MinInitialHp;

    private int TackleDamage =>
        AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    
    private const int DamageRepeat = 3;

    private int LimitTurn => 6;
    private int VoidAmount => 1;
    private int DrawMinusAmount => 1;
    
    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Creature, 99, Creature, null);
        await PowerCmd.Apply<PossessingPower>(new ThrowingPlayerChoiceContext(), Creature, LimitTurn, Creature, null);
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var moveState1 = new MoveState("SCREAM_MOVE", ScreamMove, new StatusIntent(VoidAmount));
        var moveState2 = new MoveState("TACKLE_MOVE", TackleMove, new MultiAttackIntent(TackleDamage, DamageRepeat));
        var moveState3 = new MoveState("SCARE_MOVE", ScareMove, new DebuffIntent());
        var moveState4 = new MoveState("FLEE_MOVE", FleeMove, new EscapeIntent());
        var conditionalBranchState = new ConditionalBranchState("FleeBranch");
        moveState1.FollowUpState = moveState2;
        moveState2.FollowUpState = moveState3;
        moveState3.FollowUpState = conditionalBranchState;
        moveState4.FollowUpState = moveState4;
        conditionalBranchState.AddState(moveState1, () => CombatState.RoundNumber < LimitTurn);
        conditionalBranchState.AddState(moveState4, () => CombatState.RoundNumber >= LimitTurn);
        
        var list = new List<MonsterState>() { moveState1, moveState2, moveState3, moveState4, conditionalBranchState };
        return new MonsterMoveStateMachine(list, moveState1);
    }

    private async Task ScreamMove(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Void>(targets, PileType.Discard, VoidAmount, null);
    }

    private async Task TackleMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(TackleDamage).WithHitCount(DamageRepeat).FromMonster(this)
            .WithAttackerAnim("Attack", 0.15f)
            .WithAttackerFx(null, AttackSfx)
            .WithHitFx("vfx/vfx_bite")
            .Execute(null);
    }

    private async Task ScareMove(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<DrawCardsNextTurnPower>(new ThrowingPlayerChoiceContext(), targets, -DrawMinusAmount, Creature, null);
    }

    private async Task FleeMove(IReadOnlyList<Creature> targets)
    {
        NCombatRoom.Instance?.GetCreatureNode(base.Creature)?.ToggleIsInteractable(on: false);
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/gremlin_merc/fat_gremlin_escape");
        await CreatureCmd.TriggerAnim(base.Creature, "FleeTrigger", 0f);
        await Cmd.Wait(1.25f);
        await CreatureCmd.Escape(base.Creature);
    }
}