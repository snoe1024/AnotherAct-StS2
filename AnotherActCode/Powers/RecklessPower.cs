using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnotherAct.AnotherActCode.Models.Monsters;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnotherAct.AnotherActCode.Powers;

public class RecklessPower : AnotherActPower
{
    private class Data
    {
        public decimal damageReceivedThisTurn;
    }
    
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override bool ShouldScaleInMultiplayer => true;

    public override int DisplayAmount => (int)Math.Max(0m, (decimal)base.Amount - GetInternalData<Data>().damageReceivedThisTurn);
    
    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner)
        {
            return Task.CompletedTask;
        }
        if (result.WasFullyBlocked)
        {
            return Task.CompletedTask;
        }
        var internalData = GetInternalData<Data>();
        if (internalData.damageReceivedThisTurn >= Amount)
        {
            internalData.damageReceivedThisTurn += (decimal)result.UnblockedDamage;
            return Task.CompletedTask;
        }
        
        internalData.damageReceivedThisTurn += (decimal)result.UnblockedDamage;
        InvokeDisplayAmountChanged();
        if (internalData.damageReceivedThisTurn >= (decimal)base.Amount)
        {
            Flash();
            if (Owner.Monster is VenomousSerpent serpent)
            {
                if (serpent.NextMove.Id == "TACKLE_MOVE")
                {
                    return CreatureCmd.Stun(Owner, serpent.StunnedMove, "POWER_UP_MOVE");
                }
                if (serpent.NextMove.Id == "POWER_UP_MOVE")
                {
                    return CreatureCmd.Stun(Owner, serpent.StunnedMove, "TACKLE_MOVE");
                }
                if (serpent.NextMove.Id == "BITE_MOVE")
                {
                    return CreatureCmd.Stun(Owner, serpent.StunnedMove, "BITE_MOVE");
                }
            }
        }
        return Task.CompletedTask;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        await base.AfterDeath(choiceContext, target, wasRemovalPrevented, deathAnimLength);
        
        if (!wasRemovalPrevented && target != Owner && target.Side == Owner.Side && !Owner.IsDead)
        {
            if (Owner.Monster is VenomousSerpent serpent)
            {
                if (Owner.IsStunned)
                {
                    Owner.Monster.SetMoveImmediate(new MoveState("STUN_MOVE", serpent.StunnedMoveWithoutAnim, new StunIntent())
                    {
                        FollowUpStateId = "BITE_MOVE",
                        MustPerformOnceBeforeTransitioning = true
                    }, true);
                }

                serpent.IsLonely = true;
            }
            
            await PowerCmd.Remove(this);
        }
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        GetInternalData<Data>().damageReceivedThisTurn = default(decimal);
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }
}