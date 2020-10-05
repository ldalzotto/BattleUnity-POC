using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Runtime.CompilerServices;

public class Battle_Singletons
{
    public static BattleResolutionStep _battleResolutionStep;
    public static BattleActionSelection _battleActionSelection;

    public static void Alloc()
    {
        _battleResolutionStep = BattleResolutionStep.Alloc();
        _battleActionSelection = BattleActionSelection.alloc();
    }
}

public enum BattleEntity_Team
{
    PLAYER = 0,
    FOE = 1
}

public enum BattleEntity_Type
{
    DEFAULT = 0,
    SOLIDER_MACHINEGUN_0 = 1
}

public class BattleEntity
{
    public BattleEntity_Type Type;
    public BattleEntity_Team Team;
    public float ATB_Speed;
    public bool IsControlledByPlayer;
    public BattleEntity_BaseStats Stats;
    // public List<AttackLine> AttackSet;

    public float ATB_Value;

    public int Life;
    public bool IsDead;

    public static BattleEntity Alloc() { return new BattleEntity(); }
}

public class Battle
{
    public List<BattleEntity> BattleEntities;
    public List<BattleEntity> DeadBattleEntities;

    public bool ATB_Locked = false;

    public void push_battleEntity(BattleEntity p_battleEntity)
    {
        p_battleEntity.ATB_Value = 0.0f;
        this.BattleEntities.Add(p_battleEntity);
    }

    public void entity_finishedAction(BattleEntity p_actingEntity)
    {
        p_actingEntity.ATB_Value = 0.0f;
    }


    public bool apply_damage_raw(int p_appliedDamage, BattleEntity p_hittedEntity)
    {
        p_hittedEntity.Life -= p_appliedDamage;
        p_hittedEntity.Life = Math.Max(p_hittedEntity.Life, 0);
        return p_hittedEntity.Life == 0;
    }
}



public enum BattleQueueEvent_Type
{
    NOTHING = 0,
    ATTACK = 1
}

public enum Attack_Type
{
    DEFAULT = 0
}

public struct AttackDefinition
{
    public Attack_Type AttackType;
    public int BaseDamage;

    public static AttackDefinition build(Attack_Type p_attackType, int p_baseDamage)
    {
        return new AttackDefinition() { AttackType = p_attackType, BaseDamage = p_baseDamage };
    }
}

public class BQE_Attack_UserDefined
{
    public AttackDefinition Attack;

    public BattleEntity Source;
    public BattleEntity Target;

    public List<BaseDamageStep> DamageSteps;
    public bool HasEnded;

    public object Context_UserObject;
}

public class BQEOut_Damage_Applied
{
    public BattleEntity Target;
    public int FinalDamage;
    public static BQEOut_Damage_Applied Alloc(BattleEntity p_target, int p_finalDamageApplied)
    {
        return new BQEOut_Damage_Applied { Target = p_target, FinalDamage = p_finalDamageApplied };
    }
}

public class BattleQueueEvent
{
    public BattleQueueEvent_Type Type;
    public BattleEntity ActiveEntityHandle;
    public object Event;

    public static BattleQueueEvent Alloc() { return new BattleQueueEvent(); }
}

public enum Initialize_ReturnCode
{
    NOTHING = 0,
    NEEDS_TO_BE_PROCESSED = 1
}

public class BattleResolutionStep
{
    public Battle _battle;

    public Queue<BattleQueueEvent> AttackEvents;
    public List<BaseDamageStep> DamageEvents;

    public List<BQEOut_Damage_Applied> Out_DamageApplied_Events;
    public List<BattleEntity> Out_Death_Events;
    public List<BattleQueueEvent> Out_CompletedBattlequeue_Events;

    public Action<Battle, BattleEntity> BattleActionDecision_UserFunction;
    public Func<BattleQueueEvent, Initialize_ReturnCode> BattleQueueEventInitialize_UserFunction;

    public BattleQueueEvent CurrentExecutingEvent = null;

    public static BattleResolutionStep Alloc()
    {
        BattleResolutionStep l_resolution = new BattleResolutionStep();

        l_resolution._battle = new Battle();
        l_resolution._battle.BattleEntities = new List<BattleEntity>();
        l_resolution._battle.DeadBattleEntities = new List<BattleEntity>();

        l_resolution.AttackEvents = new Queue<BattleQueueEvent>();
        l_resolution.DamageEvents = new List<BaseDamageStep>();
        l_resolution.Out_DamageApplied_Events = new List<BQEOut_Damage_Applied>();
        l_resolution.Out_Death_Events = new List<BattleEntity>();
        l_resolution.Out_CompletedBattlequeue_Events = new List<BattleQueueEvent>();

        return l_resolution;
    }

    public void update(float d)
    {
        this.Out_CompletedBattlequeue_Events.Clear();

        // Update ATB
        if (!this._battle.ATB_Locked)
        {
            for (int i = 0; i < this._battle.BattleEntities.Count; i++)
            {
                BattleEntity l_entity = this._battle.BattleEntities[i];

                if (!l_entity.IsDead)
                {
                    l_entity.ATB_Value = Math.Min(l_entity.ATB_Value + (l_entity.ATB_Speed * d), 1.0f);
                    if (l_entity.ATB_Value >= 1.0f)
                    {
                        if (!l_entity.IsControlledByPlayer)
                        {
                            this.BattleActionDecision_UserFunction.Invoke(this._battle, l_entity);
                        }
                    }
                }

                this._battle.BattleEntities[i] = l_entity;
            }
        }

    step:

        if (this.CurrentExecutingEvent == null)
        {
            if (this.AttackEvents.Count > 0)
            {
                this.CurrentExecutingEvent = this.AttackEvents.Dequeue();
                switch (this.BattleQueueEventInitialize_UserFunction.Invoke(this.CurrentExecutingEvent))
                {
                    case Initialize_ReturnCode.NEEDS_TO_BE_PROCESSED:
                        goto attackevents_init_end;
                    case Initialize_ReturnCode.NOTHING:
                        on_currentActionFinished();
                        goto step;
                }
            }
        }

    attackevents_init_end:;


        if (this.CurrentExecutingEvent != null)
        {
            this._battle.ATB_Locked = true;


            switch (this.CurrentExecutingEvent.Type)
            {
                case BattleQueueEvent_Type.ATTACK:
                    {
                        BQE_Attack_UserDefined l_event = (BQE_Attack_UserDefined)this.CurrentExecutingEvent.Event;
                        if (l_event.DamageSteps != null && l_event.DamageSteps.Count > 0)
                        {
                            for (int i = 0; i < l_event.DamageSteps.Count; i++)
                            {
                                this.DamageEvents.Add(l_event.DamageSteps[i]);
                            }
                            l_event.DamageSteps.Clear();
                        }
                        if (l_event.HasEnded)
                        {
                            on_currentActionFinished();
                           // goto step;
                        }
                    }
                    break;
            }

        }
        else
        {
            this._battle.ATB_Locked = false;
        }

        if (this.DamageEvents.Count > 0)
        {
            for (int i = 0; i < this.DamageEvents.Count; i++)
            {
                BaseDamageStep l_damageEvent = this.DamageEvents[i];
                //TOOD -> Calculate damage mitigation
                int l_appliedDamage = DamageCalculation_Algorithm.calculate(l_damageEvent);
                if (this._battle.apply_damage_raw(l_appliedDamage, l_damageEvent.Target))
                {
                    l_damageEvent.Target.IsDead = true;
                    push_death_event(l_damageEvent.Target);
                    this._battle.BattleEntities.Remove(l_damageEvent.Target);
                    this._battle.DeadBattleEntities.Add(l_damageEvent.Target);
                }

                this.Out_DamageApplied_Events.Add(BQEOut_Damage_Applied.Alloc(l_damageEvent.Target, l_appliedDamage));
            }
            this.DamageEvents.Clear();
        }

    }

    public void push_attack_event(BattleEntity p_activeEntityHandle, object p_event)
    {
        BattleQueueEvent l_event = BattleQueueEvent.Alloc();
        l_event.ActiveEntityHandle = p_activeEntityHandle;
        l_event.Type = BattleQueueEvent_Type.ATTACK;
        l_event.Event = p_event;
        this.AttackEvents.Enqueue(l_event);
    }

    private void on_currentActionFinished()
    {
        this._battle.entity_finishedAction(this.CurrentExecutingEvent.ActiveEntityHandle);
        this.Out_CompletedBattlequeue_Events.Add(this.CurrentExecutingEvent);
        this.CurrentExecutingEvent = null;
    }
    private void push_death_event(BattleEntity p_activeEntityHandle)
    {
        this.Out_Death_Events.Add(p_activeEntityHandle);
    }

}