
using UnityEngine;

public class GameLoopComponent : MonoBehaviour
{
    BattleQueueConsumer BattleQueueConsumer;

    private void Start()
    {
        this.BattleQueueConsumer = new BattleQueueConsumer();
    }

    private void Update()
    {
        float l_delta = Time.deltaTime;
        this.BattleQueueConsumer.Update(l_delta);
        Battle.get_singleton().update(l_delta);
        // this.consume_battleQueue(BattleQueue.get_singleton());
    }

}

public class BattleQueueConsumer
{
    BattleQueueEvent CurrentExecutingEvent = null;

    public void Update(float d)
    {
        if (this.CurrentExecutingEvent == null)
        {
            while (BattleQueue.get_singleton().get_next(out this.CurrentExecutingEvent))
            {
                if (BattleQueueEvent_Initialize(this.CurrentExecutingEvent))
                {
                    //TODO - Caching the Battle singleton
                    //TODO - State of Battle must be handled by him
                    Battle.get_singleton().ATB_Locked = true;
                    break;
                }
                else
                {
                    //TODO - Caching the Battle singleton
                    //TODO - State of Battle must be handled by him
                    Battle.get_singleton().ATB_Locked = false;
                }
            }
        }

        if (this.CurrentExecutingEvent != null)
        {
            if (BattleQueueEvent_process(this.CurrentExecutingEvent))
            {
                this.CurrentExecutingEvent = null;
                //TODO - Caching the Battle singleton
                //TODO - State of Battle must be handled by him
                Battle.get_singleton().ATB_Locked = false;
            };
        }
    }

    //TODO - Cleaner interface ? what does the bool mean ?
    private static bool BattleQueueEvent_Initialize(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    BQE_Attack l_event = (BQE_Attack)p_event.Event;
                    //TODO - Having a cleaner way to handle how the event is processed
                    BattleEntityComponent l_battleEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Source.Handle];
                    if (l_battleEntity != null)
                    {
                        AnimationComponent l_anim = l_battleEntity.GetComponent<AnimationComponent>();
                        if (l_anim != null)
                        {
                            l_anim.InitializeAnimation(BattleEntityComponent_Container.ComponentsByHandle[l_event.Target.Handle].transform);
                            return true;
                        }
                    }
                }
                break;
        }
        return false;
    }


    //TODO - Cleaner interface ? what does the bool mean ?
    private static bool BattleQueueEvent_process(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    //TODO - Having a cleaner way to handle how the event is processed
                    BQE_Attack l_event = (BQE_Attack)p_event.Event;
                    return BattleEntityComponent_Container.ComponentsByHandle[l_event.Source.Handle].GetComponent<AnimationComponent>().AnimBattle.State == Anim_BattleAttack_Default_State.End;
                }
        }

        return true;
    }
}