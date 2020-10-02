using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class SceneGlobalObjects
{
    public static Canvas MainCanvas;
    public static Camera MainCamera;
}

public class GameLoopComponent : MonoBehaviour
{
    public AnimationConfiguration AnimationConfiguration;
    public ATB_UIComponent ATB_Line_Prefab;
    public RectTransform DamageTextPrefab;

    ATB_UI AtbUI;

    private void Start()
    {
        SceneGlobalObjects.MainCamera = GameObject.FindGameObjectWithTag(Tags.Main_Camera).GetComponent<Camera>();
        SceneGlobalObjects.MainCanvas = GameObject.FindGameObjectWithTag(Tags.Main_Canvas).GetComponent<Canvas>();

        Battle_Singletons.Alloc();
        Battle_Singletons._battleResolutionStep.BattleQueueEventInitialize_UserFunction = BattleQueueConsumer.BattleQueueEvent_Initialize;
        Battle_Singletons._battleResolutionStep.BattleQueueEventProcess_UserFunction = BattleQueueConsumer.BattleQueueEvent_process;

        BattleEntityComponent[] l_battleEntityComponents = GameObject.FindObjectsOfType<BattleEntityComponent>();
        for (int i = 0; i < l_battleEntityComponents.Length; i++)
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

        if (Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events.Count > 0)
        {
            for (int i = 0; i < Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events.Count; i++)
            {
                BQEOut_Damage_Applied l_event = Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events[i];
                RectTransform l_instanciatedDamageTextObject = GameObject.Instantiate(this.DamageTextPrefab, SceneGlobalObjects.MainCanvas.transform);
                Text l_instanciatedDamageText = l_instanciatedDamageTextObject.GetComponentInChildren<Text>();
                l_instanciatedDamageText.text = l_event.FinalDamageApplied.ToString();
                ((RectTransform)l_instanciatedDamageTextObject.transform).position = SceneGlobalObjects.MainCamera.WorldToScreenPoint(BattleEntityComponent_Container.ComponentsByHandle[l_event.Target].transform.position);
            }
            Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events.Clear();
        }

        if (Battle_Singletons._battleResolutionStep.Out_Death_Events.Count > 0)
        {
            for (int i = 0; i < Battle_Singletons._battleResolutionStep.Out_Death_Events.Count; i++)
            {
                BattleEntity l_deadEntity = Battle_Singletons._battleResolutionStep.Out_Death_Events[i];
                BattleEntityComponent_Container.ComponentsByHandle[l_deadEntity].Dispose();
            }
            Battle_Singletons._battleResolutionStep.Out_Death_Events.Clear();
        }
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
                    if (!l_event.Source.IsDead && !l_event.Target.IsDead)
                    {
                        BattleEntityComponent l_battleEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Source];
                        if (l_battleEntity.AnimationComponent != null)
                        {
                            l_event.UserObject_Context = new BQE_Attack_SceneContext() { Source = l_battleEntity, Target = BattleEntityComponent_Container.ComponentsByHandle[l_event.Target] };
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
                    while (l_context.Source.AnimationComponent.AnimBattle.DamageStepCount > 0)
                    {
                        l_context.Source.AnimationComponent.AnimBattle.DamageStepCount -= 1;

                        Battle_Singletons._battleResolutionStep.push_damage_event(l_event.Target, 1);
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