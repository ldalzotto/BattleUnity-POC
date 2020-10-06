using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class Battle_Singletons
{
    public static BattleResolutionStep _battleResolutionStep;
    public static BattleEntityPlayerSelection _battleActionSelection;
    public static BattleEntityTargetSelection _battleTargetSelection;

    public static void Alloc()
    {
        _battleResolutionStep = BattleResolutionStep.Alloc();
        _battleActionSelection = BattleEntityPlayerSelection.alloc(_battleResolutionStep);
        _battleTargetSelection = BattleEntityTargetSelection.alloc(_battleResolutionStep);
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
    SOLIDER_MACHINEGUN_0 = 1,
    PLAYER_1 = 2,
    PLAYER_2 = 3
}

public class BattleEntity
{
    public BattleEntity_Type Type;
    public BattleEntity_Team Team;
    public float ATB_Speed;
    // This bool is different from the BattleEntity_Team enum. In the future we may be able to control foes for example.
    public bool IsControlledByPlayer;
    public BattleEntity_BaseStats Stats;
    // public List<AttackLine> AttackSet;

    public float ATB_Value;

    public int Life;
    public bool IsDead;

    public static BattleEntity Alloc() { return new BattleEntity(); }
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

[Serializable]
public struct AttackDefinition
{
    public Attack_Type AttackType;
    public int BaseDamage;
}

/// <summary>
/// Event that indicates that an "Attack" action will be performed by the Source to the Target
/// </summary>
public class BQE_Attack
{
    public AttackDefinition Attack;

    public BattleEntity Source;
    public BattleEntity Target;

    /// <summary>
    /// While the BQE_Attack is being processed, this list indicates that BaseDamageStep calculation must be performed
    /// between the Source and the Target.
    /// This list is checked every frame while the event is being processed.
    /// </summary>
    public List<BaseDamageStep> Out_DamageSteps;
    public bool HasEnded;

    public object Context_UserObject;
}

/// <summary>
/// The results of <see cref="BQE_Attack.Out_DamageSteps"/> processing. Packed as an event to be consumed by external dependencies.
/// </summary>
public class BQEOut_FinalDamageApplied
{
    public BattleEntity Target;
    public int FinalDamage;
    public static BQEOut_FinalDamageApplied Alloc(BattleEntity p_target, int p_finalDamageApplied)
    {
        return new BQEOut_FinalDamageApplied { Target = p_target, FinalDamage = p_finalDamageApplied };
    }
}

/// <summary>
/// An event that is queued in the <see cref="BattleResolutionStep.BattleQueueEvents"/> and executed one after the other.
/// </summary>
public class BattleQueueEvent
{
    public BattleQueueEvent_Type Type;

    // The BattleEntity that have triggered this event.
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
    public List<BattleEntity> BattleEntities;
    public List<BattleEntity> DeadBattleEntities;
    
    //If true, ATB values are never updated
    private bool ATB_Locked;

    private Queue<BattleQueueEvent> BattleQueueEvents;
    private List<BaseDamageStep> DamageEvents;

    public List<BQEOut_FinalDamageApplied> Out_DamageApplied_Events;
    public List<BattleEntity> Out_Death_Events;
    public List<BattleQueueEvent> Out_CompletedBattlequeue_Events;

    public Action<BattleResolutionStep, BattleEntity> BattleActionDecision_UserFunction;
    public Func<BattleQueueEvent, Initialize_ReturnCode> BattleQueueEventInitialize_UserFunction;

    public BattleQueueEvent CurrentExecutingEvent = null;

    public static BattleResolutionStep Alloc()
    {
        BattleResolutionStep l_resolution = new BattleResolutionStep();

        l_resolution.BattleEntities = new List<BattleEntity>();
        l_resolution.DeadBattleEntities = new List<BattleEntity>();
        l_resolution.ATB_Locked = false;

        l_resolution.BattleQueueEvents = new Queue<BattleQueueEvent>();
        l_resolution.DamageEvents = new List<BaseDamageStep>();
        l_resolution.Out_DamageApplied_Events = new List<BQEOut_FinalDamageApplied>();
        l_resolution.Out_Death_Events = new List<BattleEntity>();
        l_resolution.Out_CompletedBattlequeue_Events = new List<BattleQueueEvent>();

        return l_resolution;
    }

    public void update(float d)
    {
        this.Out_DamageApplied_Events.Clear();
        this.Out_Death_Events.Clear();
        this.Out_CompletedBattlequeue_Events.Clear();

        // Update ATB_Values
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
                        if (!l_entity.IsControlledByPlayer)
                        {
                            this.BattleActionDecision_UserFunction.Invoke(this, l_entity);
                        }
                    }
                }

                this.BattleEntities[i] = l_entity;
            }
        }

    // A step takes the first entry of the BattleQueueEvents and initialize it
    step:

        if (this.CurrentExecutingEvent == null)
        {
            if (this.BattleQueueEvents.Count > 0)
            {
                this.CurrentExecutingEvent = this.BattleQueueEvents.Dequeue();
                switch (this.BattleQueueEventInitialize_UserFunction.Invoke(this.CurrentExecutingEvent))
                {
                    case Initialize_ReturnCode.NOTHING:
                        // When initialization doesn't require further processing, then we perform another step.
                        this.on_currentActionFinished();
                        goto step;
                }
            }
        }


        if (this.CurrentExecutingEvent != null)
        {
            this.ATB_Locked = true;

            // We perform specific operation every frame for the current executing event
            switch (this.CurrentExecutingEvent.Type)
            {
                case BattleQueueEvent_Type.ATTACK:
                    {
                        // We consume damage events of the BQE_Attack event and push it to the global queue.
                        BQE_Attack l_event = (BQE_Attack)this.CurrentExecutingEvent.Event;
                        if (l_event.Out_DamageSteps != null && l_event.Out_DamageSteps.Count > 0)
                        {
                            for (int i = 0; i < l_event.Out_DamageSteps.Count; i++)
                            {
                                this.DamageEvents.Add(l_event.Out_DamageSteps[i]);
                            }
                            l_event.Out_DamageSteps.Clear();
                        }
                        if (l_event.HasEnded)
                        {
                            on_currentActionFinished();
                        }
                    }
                    break;
            }

        }
        else
        {
            this.ATB_Locked = false;
        }

        // Effectively apply damage events
        if (this.DamageEvents.Count > 0)
        {
            for (int i = 0; i < this.DamageEvents.Count; i++)
            {
                BaseDamageStep l_damageEvent = this.DamageEvents[i];
                
                int l_appliedDamage = DamageCalculation_Algorithm.calculate(l_damageEvent);
                if (DamageCalculation_Algorithm.apply_damage_raw(l_appliedDamage, l_damageEvent.Target))
                {
                    l_damageEvent.Target.IsDead = true;
                    push_death_event(l_damageEvent.Target);
                    this.BattleEntities.Remove(l_damageEvent.Target);
                    this.DeadBattleEntities.Add(l_damageEvent.Target);
                }

                this.Out_DamageApplied_Events.Add(BQEOut_FinalDamageApplied.Alloc(l_damageEvent.Target, l_appliedDamage));
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
        this.BattleQueueEvents.Enqueue(l_event);
    }

    private void on_currentActionFinished()
    {
        this.CurrentExecutingEvent.ActiveEntityHandle.ATB_Value = 0.0f;
        this.Out_CompletedBattlequeue_Events.Add(this.CurrentExecutingEvent);
        this.CurrentExecutingEvent = null;
    }
    private void push_death_event(BattleEntity p_activeEntityHandle)
    {
        this.Out_Death_Events.Add(p_activeEntityHandle);
    }

    public void push_battleEntity(BattleEntity p_battleEntity)
    {
        p_battleEntity.ATB_Value = 0.0f;
        this.BattleEntities.Add(p_battleEntity);
    }
}