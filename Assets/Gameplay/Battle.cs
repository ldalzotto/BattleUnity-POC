using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Battle_Singletons
{
    public static Battle _battle;
    public static BattleQueue _battleQueue;

    public static void Alloc()
    {
        _battle = new Battle();
        _battle.BattleEntities = new RefList<BattleEntity>();

        _battleQueue = new BattleQueue();
        _battleQueue.PendingEvents = new Queue<BattleQueueEvent>();
    }
}

public struct BattleEntity
{
    public float ATB_Value;
    public float ATB_Speed;
}

public struct BattleEntity_Handle
{
    public int Handle;
}

public class Battle
{
    public RefList<BattleEntity> BattleEntities;
    private bool ATB_Locked = false;

    public BattleEntity_Handle push_battleEntity(ref BattleEntity p_battleEntity)
    {
        p_battleEntity.ATB_Value = 0.0f;
        this.BattleEntities.AddRef(ref p_battleEntity);
        return new BattleEntity_Handle { Handle = this.BattleEntities.Count - 1 };
    }

    public void update(float d)
    {
        // Update ATB
        if (!this.ATB_Locked)
        {
            for (int i = 0; i < this.BattleEntities.Count; i++)
            {
                BattleEntity l_entity = this.BattleEntities[i];
                l_entity.ATB_Value += l_entity.ATB_Speed * d;

                if (l_entity.ATB_Value >= 1.0f)
                {
                    Debug.Log(i);
                    // BattleEventPicker(l_entity);

                    //TODO, this is temporary for test
                    //TODO we may have a smarter choice by adding a type to the battle entity (PLAYER_CONTROLLED ?, FOE ?) and then picking the correct target
                    { 
                        BQE_Attack l_attackEvent = new BQE_Attack { Source = new BattleEntity_Handle { Handle = i }, Target = new BattleEntity_Handle { Handle = 0 } };
                        Battle_Singletons._battleQueue.push_event(new BattleEntity_Handle { Handle = i }, l_attackEvent, BattleQueueEvent_Type.ATTACK);
                    }

                }

                this.BattleEntities[i] = l_entity;
            }
        }
    }

    /*
    private void BattleEventPicker(BattleEntity p_actingEntity)
    {

    }
    */

    public void lock_ATB()
    {
        this.ATB_Locked = true;
    }
    public void unlock_ATB()
    {
        this.ATB_Locked = false;
    }

    public void entity_finishedAction(BattleEntity_Handle p_actingEntity)
    {
        this.BattleEntities.ValueRef(p_actingEntity.Handle).ATB_Value = 0.0f;
    }
}

public enum BattleQueueEvent_Type
{
    NOTHING = 0,
    ATTACK = 1
}

public class BQE_Attack
{
    public BattleEntity_Handle Source;
    public BattleEntity_Handle Target;
}

public class BattleQueueEvent
{
    public BattleQueueEvent_Type Type;
    public BattleEntity_Handle ActiveEntityHandle;
    public object Event;
}

public class BattleQueue
{
    public Queue<BattleQueueEvent> PendingEvents;

    public bool get_next(out BattleQueueEvent out_event)
    {
        if (this.PendingEvents.Count == 0)
        {
            out_event = null;
            return false;
        }

        out_event = this.PendingEvents.Dequeue();

        return true;
    }

    public void push_event(BattleEntity_Handle p_activeEntityHandle, object p_event, BattleQueueEvent_Type l_type)
    {
        BattleQueueEvent l_event = new BattleQueueEvent();
        l_event.ActiveEntityHandle = p_activeEntityHandle;
        l_event.Type = l_type;
        l_event.Event = p_event;
        PendingEvents.Enqueue(l_event);
    }
}