using AnotherAct.AnotherActCode.Models.Monsters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnotherAct.AnotherActCode.Powers;

public class WithdrawnPower : AnotherActPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    
    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0 && Owner.IsAlive &&
            Owner.Monster is RockSlimeS or RockSlimeM)
        {
            var monster = Owner.Monster;
            var replacementId = monster.NextMove.Id switch
            {
                "GOOP_TACKLE_MOVE" => "TACKLE_MOVE",
                "GOOP_SHIELD_MOVE" => "SHIELD_MOVE",
                _ => null
            };
            if (replacementId != null && monster.MoveStateMachine != null)
            {
                monster.SetMoveImmediate((MoveState)monster.MoveStateMachine.States[replacementId]);
            }
        }
        return Task.CompletedTask;
    }
}
