using System.Collections.Generic;
using UnityEngine;

public class GameLoopComponent : MonoBehaviour
{
    public AnimationConfiguration AnimationConfiguration;
    public ATB_UIComponent ATB_Line_Prefab;

    ATB_UI AtbUI;

    private void Start()
    {
        Battle_Singletons.Alloc();
        Battle_Singletons._battleResolutionStep.BattleQueueEventInitialize_UserFunction = BattleQueueConsumer.BattleQueueEvent_Initialize;
        Battle_Singletons._battleResolutionStep.BattleQueueEventProcess_UserFunction = BattleQueueConsumer.BattleQueueEvent_process;

        BattleEntityComponent[] l_battleEntityComponents = GameObject.FindObjectsOfType<BattleEntityComponent>();
        for(int i=0;i<l_battleEntityComponents.Length;i++)
        {
            l_battleEntityComponents[i].Initialize(this.AnimationConfiguration);
        }

        this.AtbUI = new ATB_UI();
        this.AtbUI.Initialize(this.ATB_Line_Prefab, Battle_Singletons._battle);
    }

    private void Update()
    {
        float l_delta = Time.deltaTime;
        Battle_Singletons._battle.update(l_delta);
        Battle_Singletons._battleResolutionStep.perform_step();
        this.AtbUI.Update(l_delta);
    }

}

public static class BattleQueueConsumer
{
    public static Initialize_ReturnCode BattleQueueEvent_Initialize(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    BQE_Attack_UserDefined l_event = (BQE_Attack_UserDefined)p_event.Event;
                    BattleEntityComponent l_battleEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Source.Handle];
                    if (l_battleEntity != null)
                    {
                        if (l_battleEntity.AnimationComponent != null)
                        {
                            l_event.UserObject_Context = new BQE_Attack_SceneContext() { Source = l_battleEntity, Target = BattleEntityComponent_Container.ComponentsByHandle[l_event.Target.Handle] };
                            l_battleEntity.AnimationComponent.InitializeAnimation(((BQE_Attack_SceneContext)l_event.UserObject_Context).Target.transform);
                            return Initialize_ReturnCode.NEEDS_TO_BE_PROCESSED;
                        }
                    }
                }
                break;
        }
        return Initialize_ReturnCode.NOTHING;
    }



    public static Process_ReturnCode BattleQueueEvent_process(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    BQE_Attack_UserDefined l_event = (BQE_Attack_UserDefined)p_event.Event;
                    BQE_Attack_SceneContext l_context = (BQE_Attack_SceneContext)l_event.UserObject_Context;

                    // Calculate and apply damages
                    while(l_context.Source.AnimationComponent.AnimBattle.DamageStepCount > 0)
                    {
                        l_context.Source.AnimationComponent.AnimBattle.DamageStepCount -= 1;

                        //TODO -> push damage event instead
                        Battle_Singletons._battle.apply_damage_raw(1, l_event.Target.Handle);
                    }

                    // End event if necessary
                    if (l_context.Source.AnimationComponent.AnimBattle.State == Anim_BattleAttack_Default_State.End)
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