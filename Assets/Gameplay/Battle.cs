using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Runtime.CompilerServices;

public class Battle_Singletons
{
    public static Battle _battle;
    public static BattleResolutionStep _battleResolutionStep;

    public static void Alloc()
    {
        _battle = new Battle();
        _battle.BattleEntities = new List<BattleEntity>();
        _battle.DeadBattleEntities = new List<BattleEntity>();

        _battleResolutionStep = BattleResolutionStep.Alloc();
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
    TEST = 1
}

public class BattleEntity
{
    public BattleEntity_Type Type;
    public BattleEntity_Team Team;
    public float ATB_Value;
    public float ATB_Speed;

    public int Life;
    public bool IsDead;

    public static BattleEntity Alloc() { return new BattleEntity(); }
}

public class Battle
{
    public List<BattleEntity> BattleEntities;
    public List<BattleEntity> DeadBattleEntities;
    
    private bool ATB_Locked = false;

    public void push_battleEntity(BattleEntity p_battleEntity)
    {
        p_battleEntity.ATB_Value = 0.0f;
        this.BattleEntities.Add(p_battleEntity);
    }

    public void update(float d)
    {
        // Update ATB
        if (!this.ATB_Locked)
        {
            for (int i = 0; i < this.BattleEntities.Count; i++)
            {
                BattleEntity l_entity = this.BattleEntities[i];

                if (!l_entity.IsDead)
                {
                    l_entity.ATB_Value = Math.Min(l_entity.ATB_Value + (l_entity.ATB_Speed * d), 1.0f);

                    if (l_entity.ATB_Value >= 1.0f)
                    {
                        BattleDecision.Interface.decide_nextAction(this, l_entity);
                    }
                }

                this.BattleEntities[i] = l_entity;
            }
        }
    }

    public void lock_ATB()
    {
        this.ATB_Locked = true;
    }
    public void unlock_ATB()
    {
        this.ATB_Locked = false;
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

//TODO -> In the future, this will have damage value and type
public class AttackEvent_DamageStep
{
    public BattleEntity Target;
}

public class BQE_Attack_UserDefined
{
    public Attack_Type AttackType;

    public BattleEntity Source;
    public BattleEntity Target;

    public List<AttackEvent_DamageStep> DamageSteps;
    public bool HasEnded;

    public object Context_UserObject;
}

public class BQE_Damage_Apply
{
    public BattleEntity Target;
    //TODO -> This damage is raw. Future damage mitigation will be performed during the BattleResolutionStep
    public int DamageApplied;

    public static BQE_Damage_Apply Alloc() { return new BQE_Damage_Apply(); }
}

public class BQEOut_Damage_Applied
{
    public BattleEntity Target;
    public int FinalDamageApplied;
    public static BQEOut_Damage_Applied Alloc(BattleEntity p_target, int p_finalDamageApplied)
    {
        return new BQEOut_Damage_Applied { Target = p_target, FinalDamageApplied = p_finalDamageApplied };
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
    public Queue<BattleQueueEvent> AttackEvents;
    public List<BQE_Damage_Apply> DamageEvents;
    // public Queue<BattleQueueEvent> DeathEvents;

    public List<BQEOut_Damage_Applied> Out_DamageApplied_Events;
    public List<BattleEntity> Out_Death_Events;

    public Func<BattleQueueEvent, Initialize_ReturnCode> BattleQueueEventInitialize_UserFunction;
    // public Func<BattleQueueEvent, Process_ReturnCode> BattleQueueEventProcess_UserFunction;

    public BattleQueueEvent CurrentExecutingEvent = null;

    public static BattleResolutionStep Alloc()
    {
        BattleResolutionStep l_resolution = new BattleResolutionStep();
        l_resolution.AttackEvents = new Queue<BattleQueueEvent>();
        l_resolution.DamageEvents = new List<BQE_Damage_Apply>();
        l_resolution.Out_DamageApplied_Events = new List<BQEOut_Damage_Applied>();
        l_resolution.Out_Death_Events = new List<BattleEntity>();
        return l_resolution;
    }

    public void perform_step()
    {
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
                        this.CurrentExecutingEvent = null;
                        goto step;
                }
            }
        }

    attackevents_init_end:;


        if (this.CurrentExecutingEvent != null)
        {
            Battle_Singletons._battle.lock_ATB();


            switch (this.CurrentExecutingEvent.Type)
            {
                case BattleQueueEvent_Type.ATTACK:
                    {
                        BQE_Attack_UserDefined l_event = (BQE_Attack_UserDefined)this.CurrentExecutingEvent.Event;
                        if(l_event.DamageSteps != null && l_event.DamageSteps.Count > 0)
                        {
                            for(int i=0;i< l_event.DamageSteps.Count;i++)
                            {
                                this.push_damage_event(l_event.DamageSteps[i].Target, 1);   
                            }
                            l_event.DamageSteps.Clear();
                        }
                        if (l_event.HasEnded)
                        {
                            Battle_Singletons._battle.entity_finishedAction(this.CurrentExecutingEvent.ActiveEntityHandle);
                            this.CurrentExecutingEvent = null;
                            goto step;
                        }
                    }
                    break;
            }

        }
        else
        {
            Battle_Singletons._battle.unlock_ATB();
        }

        if (this.DamageEvents.Count > 0)
        {
            for (int i = 0; i < this.DamageEvents.Count; i++)
            {
                BQE_Damage_Apply l_damageEvent = this.DamageEvents[i];
                //TOOD -> Calculate damage mitigation
                if (Battle_Singletons._battle.apply_damage_raw(l_damageEvent.DamageApplied, l_damageEvent.Target))
                {
                    l_damageEvent.Target.IsDead = true;
                    push_death_event(l_damageEvent.Target);
                    Battle_Singletons._battle.BattleEntities.Remove(l_damageEvent.Target);
                    Battle_Singletons._battle.DeadBattleEntities.Add(l_damageEvent.Target);
                }

                this.Out_DamageApplied_Events.Add(BQEOut_Damage_Applied.Alloc(l_damageEvent.Target, l_damageEvent.DamageApplied));
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


    private void push_death_event(BattleEntity p_activeEntityHandle)
    {
        this.Out_Death_Events.Add(p_activeEntityHandle);
    }

    private void push_damage_event(BattleEntity p_targettedEntity, int p_damageValue)
    {
        BQE_Damage_Apply l_damageEvent = BQE_Damage_Apply.Alloc();
        l_damageEvent.DamageApplied = p_damageValue;
        l_damageEvent.Target = p_targettedEntity;
        this.DamageEvents.Add(l_damageEvent);
    }
}