using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Collections.LowLevel.Unsafe;

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

public enum BattleEntity_Team
{
    PLAYER = 0,
    FOE = 1
}

public struct BattleEntity
{
    public BattleEntity_Team Team;
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
                    BattleEventPicker(i);
                }

                this.BattleEntities[i] = l_entity;
            }
        }
    }

    /// <summary>
    /// Pick the right BattleQueueEvent and push it to the queue.
    /// </summary>
    /// <param name="p_actingEntityHandle"></param>
    private void BattleEventPicker(int p_actingEntityHandle)
    {
        using (UnsafeList<int> l_targettableEntities = new UnsafeList<int>(0, Unity.Collections.Allocator.Temp))
        {
            switch (this.BattleEntities.ValueRef(p_actingEntityHandle).Team)
            {
                case BattleEntity_Team.PLAYER:
                    {
                        for (int i = 0; i < this.BattleEntities.Count; i++)
                        {
                            if (this.BattleEntities[i].Team != BattleEntity_Team.PLAYER)
                            {
                                l_targettableEntities.Add(i);
                            }
                        }
                    }
                    break;
                case BattleEntity_Team.FOE:
                    {
                        for (int i = 0; i < this.BattleEntities.Count; i++)
                        {
                            if (this.BattleEntities[i].Team != BattleEntity_Team.FOE)
                            {
                                l_targettableEntities.Add(i);
                            }
                        }
                    }
                    break;
            }

            if (l_targettableEntities.Length > 0)
            {
                int l_targettedEntity = l_targettableEntities[Random.Range(0, l_targettableEntities.Length)];
                BQE_Attack l_attackEvent = new BQE_Attack { Source = new BattleEntity_Handle { Handle = p_actingEntityHandle }, Target = new BattleEntity_Handle { Handle = l_targettedEntity } };
                Battle_Singletons._battleQueue.push_event(new BattleEntity_Handle { Handle = p_actingEntityHandle }, l_attackEvent, BattleQueueEvent_Type.ATTACK);
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