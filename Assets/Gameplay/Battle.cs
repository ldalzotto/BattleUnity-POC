using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private static Battle _battle;

    //TODO - Must be private
    public bool ATB_Locked;
    public RefList<BattleEntity> BattleEntities;

    public static Battle get_singleton()
    {
        if (_battle == null)
        {
            _battle = new Battle();
            _battle.BattleEntities = new RefList<BattleEntity>();
        }

        return _battle;
    }

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


                    //TODO, this is temporary for test
                    { 
                        BQE_Attack l_attackEvent = new BQE_Attack { Source = new BattleEntity_Handle { Handle = i }, Target = new BattleEntity_Handle { Handle = 0 } };
                        BattleQueue.get_singleton().push_event(l_attackEvent, BattleQueueEvent_Type.ATTACK);
                    }


                    l_entity.ATB_Value = 0.0f;
                }

                this.BattleEntities[i] = l_entity;
            }
        }
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
    public object Event;
}

public class BattleQueue
{
    private static BattleQueue _battleQueue;

    Queue<BattleQueueEvent> PendingEvents;

    public static BattleQueue get_singleton()
    {
        if (_battleQueue == null)
        {
            _battleQueue = new BattleQueue();
            _battleQueue.PendingEvents = new Queue<BattleQueueEvent>();
        }
        return _battleQueue;
    }

    public bool get_next(out BattleQueueEvent out_event)
    {
        if (this.PendingEvents.Count == 0)
        {
            out_event = null;
            // Battle.get_singleton().ATB_Locked = false;
            return false;
        }

        out_event = this.PendingEvents.Dequeue();
        // Battle.get_singleton().ATB_Locked = true;

        return true;
    }

    public void push_event(object p_event, BattleQueueEvent_Type l_type)
    {
        BattleQueueEvent l_event = new BattleQueueEvent();
        l_event.Type = l_type;
        l_event.Event = p_event;
        PendingEvents.Enqueue(l_event);
    }
}