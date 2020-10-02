using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Collections.LowLevel.Unsafe;
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

        _battleResolutionStep = BattleResolutionStep.Alloc();
    }
}

public enum BattleEntity_Team
{
    PLAYER = 0,
    FOE = 1
}

public class BattleEntity
{
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
                l_entity.ATB_Value = Math.Min(l_entity.ATB_Value + (l_entity.ATB_Speed * d), 1.0f);

                if (l_entity.ATB_Value >= 1.0f)
                {
                    Battle_Algorithm.BattleEventPicker(this, l_entity);
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

public static class Battle_Algorithm
{
    /// <summary>
    /// Pick the right BattleQueueEvent and push it to the queue.
    /// </summary>
    /// <param name="p_actingEntity"></param>
    public static void BattleEventPicker(Battle p_battle, BattleEntity p_actingEntity)
    {
        using (UnsafeList<int> l_targettableEntities = new UnsafeList<int>(0, Unity.Collections.Allocator.Temp))
        {
            switch (p_actingEntity.Team)
            {
                case BattleEntity_Team.PLAYER:
                    {
                        for (int i = 0; i < p_battle.BattleEntities.Count; i++)
                        {
                            if (p_battle.BattleEntities[i].Team != BattleEntity_Team.PLAYER)
                            {
                                l_targettableEntities.Add(i);
                            }
                        }
                    }
                    break;
                case BattleEntity_Team.FOE:
                    {
                        for (int i = 0; i < p_battle.BattleEntities.Count; i++)
                        {
                            if (p_battle.BattleEntities[i].Team != BattleEntity_Team.FOE)
                            {
                                l_targettableEntities.Add(i);
                            }
                        }
                    }
                    break;
            }

            if (l_targettableEntities.Length > 0)
            {
                BattleEntity l_targettedEntity = p_battle.BattleEntities[l_targettableEntities[Random.Range(0, l_targettableEntities.Length)]];
                BQE_Attack_UserDefined l_attackEvent = new BQE_Attack_UserDefined { Source = p_actingEntity, Target = l_targettedEntity };
                Battle_Singletons._battleResolutionStep.push_attack_event(p_actingEntity, l_attackEvent, BattleQueueEvent_Type.ATTACK);
            }
        }
    }

}

public enum BattleQueueEvent_Type
{
    NOTHING = 0,
    ATTACK = 1,
    ATTACK_USERDEFINED = 2
}

public class BQE_Attack_UserDefined
{
    public BattleEntity Source;
    public BattleEntity Target;

    public int UserObject_Context_Type;
    public object UserObject_Context;
}

public class BQE_Damage_Apply
{
    public BattleEntity Target;
    public int DamageApplied;

    public static BQE_Damage_Apply Alloc() { return new BQE_Damage_Apply(); }
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
public enum Process_ReturnCode
{
    EVENT_FINISHED = 0,
    EVENT_INPROGRESS = 1,
    NO_EVENT_INPROGRESS = 2
}

public class BattleResolutionStep
{
    public Queue<BattleQueueEvent> AttackEvents;
    public List<BQE_Damage_Apply> DamageEvents;
    // public Queue<BattleQueueEvent> DeathEvents;

    public Func<BattleQueueEvent, Initialize_ReturnCode> BattleQueueEventInitialize_UserFunction;
    public Func<BattleQueueEvent, Process_ReturnCode> BattleQueueEventProcess_UserFunction;

    public BattleQueueEvent CurrentExecutingEvent = null;

    public static BattleResolutionStep Alloc()
    {
        BattleResolutionStep l_resolution = new BattleResolutionStep();
        l_resolution.AttackEvents = new Queue<BattleQueueEvent>();
        // l_resolution.DeathEvents = new Queue<BattleQueueEvent>();
        l_resolution.DamageEvents = new List<BQE_Damage_Apply>();
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

            switch (this.BattleQueueEventProcess_UserFunction.Invoke(this.CurrentExecutingEvent))
            {
                case Process_ReturnCode.EVENT_FINISHED:
                    {
                        Battle_Singletons._battle.entity_finishedAction(this.CurrentExecutingEvent.ActiveEntityHandle);
                        this.CurrentExecutingEvent = null;
                        goto step;
                    }
            }
        }
        else
        {
            Battle_Singletons._battle.unlock_ATB();
        }

        if(this.DamageEvents.Count > 0)
        {
            for (int i = 0; i < this.DamageEvents.Count; i++)
            {
                BQE_Damage_Apply l_damageEvent = this.DamageEvents[i];
                if (Battle_Singletons._battle.apply_damage_raw(l_damageEvent.DamageApplied, l_damageEvent.Target))
                {
                    l_damageEvent.Target.IsDead = true;
                }
            }
            this.DamageEvents.Clear();
        }
   


    }


    public void push_attack_event(BattleEntity p_activeEntityHandle, object p_event, BattleQueueEvent_Type l_type)
    {
        BattleQueueEvent l_event = BattleQueueEvent.Alloc();
        l_event.ActiveEntityHandle = p_activeEntityHandle;
        l_event.Type = l_type;
        l_event.Event = p_event;
        this.AttackEvents.Enqueue(l_event);
    }

    /*
    public void push_death_event(BattleEntity_Handle p_activeEntityHandle, object p_event, BattleQueueEvent_Type l_type)
    {
        BattleQueueEvent l_event = BattleQueueEvent.Alloc();
        l_event.ActiveEntityHandle = p_activeEntityHandle;
        l_event.Type = l_type;
        l_event.Event = p_event;
        this.DeathEvents.Enqueue(l_event);
    }
    */
    public void push_damage_event(BattleEntity p_targettedEntity, int p_damageValue)
    {
        BQE_Damage_Apply l_damageEvent = BQE_Damage_Apply.Alloc();
        l_damageEvent.DamageApplied = p_damageValue;
        l_damageEvent.Target = p_targettedEntity;
        this.DamageEvents.Add(l_damageEvent);
    }
}