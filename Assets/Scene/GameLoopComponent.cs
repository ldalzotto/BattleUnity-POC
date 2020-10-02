using UnityEngine;

public class GameLoopComponent : MonoBehaviour
{
    BattleQueueConsumer BattleQueueConsumer;

    private void Start()
    {
        Battle_Singletons.Alloc();
        this.BattleQueueConsumer = new BattleQueueConsumer();

        BattleEntityComponent[] l_battleEntityComponents = GameObject.FindObjectsOfType<BattleEntityComponent>();
        for(int i=0;i<l_battleEntityComponents.Length;i++)
        {
            l_battleEntityComponents[i].Initialize();
        }

    }

    private void Update()
    {
        float l_delta = Time.deltaTime;
        this.BattleQueueConsumer.Update(l_delta);
        Battle_Singletons._battle.update(l_delta);
    }

}

public class BattleQueueConsumer
{
    BattleQueueEvent CurrentExecutingEvent = null;
    object CurrentExecutingEvent_SceneContext = null;

    public void Update(float d)
    {
        if (this.CurrentExecutingEvent == null)
        {
            while (Battle_Singletons._battleQueue.get_next(out this.CurrentExecutingEvent))
            {
                if (BattleQueueEvent_Initialize(this.CurrentExecutingEvent) == Initialize_ReturnCode.NEEDS_TO_BE_PROCESSED)
                {
                    Battle_Singletons._battle.lock_ATB();
                    break;
                }
                else
                {
                    Battle_Singletons._battle.unlock_ATB();
                }
            }
        }

        if (this.CurrentExecutingEvent != null)
        {
            if (BattleQueueEvent_process(this.CurrentExecutingEvent) == Process_ReturnCode.EVENT_FINISHED)
            {
                this.CurrentExecutingEvent = null;
                Battle_Singletons._battle.unlock_ATB();
            };
        }
    }

    enum Initialize_ReturnCode
    {
        NOTHING = 0,
        NEEDS_TO_BE_PROCESSED = 1
    }

    private Initialize_ReturnCode BattleQueueEvent_Initialize(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    BQE_Attack l_event = (BQE_Attack)p_event.Event;
                    BattleEntityComponent l_battleEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Source.Handle];
                    if (l_battleEntity != null)
                    {
                        if (l_battleEntity.AnimationComponent != null)
                        {
                            this.CurrentExecutingEvent_SceneContext = new BQE_Attack_SceneContext() { Source = l_battleEntity, Target = BattleEntityComponent_Container.ComponentsByHandle[l_event.Target.Handle] };
                            l_battleEntity.AnimationComponent.InitializeAnimation(((BQE_Attack_SceneContext)this.CurrentExecutingEvent_SceneContext).Target.transform);
                            return Initialize_ReturnCode.NEEDS_TO_BE_PROCESSED;
                        }
                    }
                }
                break;
        }
        return Initialize_ReturnCode.NOTHING;
    }

    enum Process_ReturnCode
    {
        EVENT_FINISHED = 0,
        EVENT_INPROGRESS = 1,
        NO_EVENT_INPROGRESS = 2
    }

    private Process_ReturnCode BattleQueueEvent_process(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    if(((BQE_Attack_SceneContext)this.CurrentExecutingEvent_SceneContext).Source.AnimationComponent.AnimBattle.State == Anim_BattleAttack_Default_State.End)
                    {
                        return Process_ReturnCode.EVENT_FINISHED;
                    }
                    else
                    {
                        return Process_ReturnCode.EVENT_INPROGRESS;
                    }
                }
        }

        return Process_ReturnCode.NO_EVENT_INPROGRESS;
    }
}

public class BQE_Attack_SceneContext
{
    public BattleEntityComponent Source;
    public BattleEntityComponent Target;
}