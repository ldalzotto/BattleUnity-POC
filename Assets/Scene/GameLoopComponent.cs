using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class SceneGlobalObjects
{
    public static Canvas MainCanvas;
    public static Camera MainCamera;
    public static AnimationConfiguration AnimationConfiguration;

    public static BattleActionSelectionUI BattleActionSelectionUI;
    public static BattleTargetSelectionUI BattleTargetSelectionUI;

    public static BattleSelectionFlow PlayerTurnInputflow;
}

public class GameLoopComponent : MonoBehaviour
{
    public AnimationConfiguration AnimationConfiguration;
    public ATB_UIComponent ATB_Line_Prefab;
    public RectTransform DamageTextPrefab;
    public RectTransform ActionSelectionMenuPrefab;
    public GameObject CurrentBattleActionSelectionEntityCursorPrefab;
    public GameObject BattleTargetSelectionUIGameObjectPrefab;

    ATB_UI AtbUI;

    private void Start()
    {
        SceneGlobalObjects.MainCamera = GameObject.FindGameObjectWithTag(Tags.Main_Camera).GetComponent<Camera>();
        SceneGlobalObjects.MainCanvas = GameObject.FindGameObjectWithTag(Tags.Main_Canvas).GetComponent<Canvas>();
        SceneGlobalObjects.AnimationConfiguration = this.AnimationConfiguration;
        SceneGlobalObjects.BattleActionSelectionUI = BattleActionSelectionUI.alloc(this.ActionSelectionMenuPrefab, this.CurrentBattleActionSelectionEntityCursorPrefab);
        SceneGlobalObjects.BattleTargetSelectionUI = BattleTargetSelectionUI.alloc(this.BattleTargetSelectionUIGameObjectPrefab);
        SceneGlobalObjects.PlayerTurnInputflow = BattleSelectionFlow.build();

        Battle_Singletons.Alloc();
        Battle_Singletons._battleResolutionStep.BattleQueueEventInitialize_UserFunction = BattleAnimation.initialize_attackAnimation;
        Battle_Singletons._battleResolutionStep.BattleActionDecision_UserFunction = BattleDecision_Specific.Interface.decide_nextAction;

        // Battle_Singletons._battleResolutionStep.BattleAction_OnCurrentActionExecuted_UserFunction = (BattleQueueEvent p_event) => { Battle_Singletons._battleActionSelection.on_battleActionCompleted(p_event); };

        BattleEntityComponent[] l_battleEntityComponents = GameObject.FindObjectsOfType<BattleEntityComponent>();
        for (int i = 0; i < l_battleEntityComponents.Length; i++)
        {
            l_battleEntityComponents[i].Initialize();
        }

        this.AtbUI = new ATB_UI();
        this.AtbUI.Initialize(this.ATB_Line_Prefab, Battle_Singletons._battleResolutionStep._battle);
    }

    private void Update()
    {
        float l_delta = Time.deltaTime;

        Battle_Singletons._battleActionSelection.update(l_delta);
        Battle_Singletons._battleTargetSelection.update(l_delta);

        SceneGlobalObjects.PlayerTurnInputflow.update(l_delta);

        Battle_Singletons._battleResolutionStep.update(l_delta);

        this.AtbUI.Update(l_delta);

        if (Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events.Count > 0)
        {
            for (int i = 0; i < Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events.Count; i++)
            {
                BQEOut_Damage_Applied l_event = Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events[i];
                BattleEntityComponent l_targetEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Target];

                RectTransform l_instanciatedDamageTextObject = GameObject.Instantiate(this.DamageTextPrefab, SceneGlobalObjects.MainCanvas.transform);
                Text l_instanciatedDamageText = l_instanciatedDamageTextObject.GetComponentInChildren<Text>();
                l_instanciatedDamageText.text = l_event.FinalDamage.ToString();
                ((RectTransform)l_instanciatedDamageTextObject.transform).position = SceneGlobalObjects.MainCamera.WorldToScreenPoint(l_targetEntity.DamageDisplay_Transform.transform.position);

                BattleAnimation.onDamageReceived(l_targetEntity);
            }
            Battle_Singletons._battleResolutionStep.Out_DamageApplied_Events.Clear();
        }

        // On battle entity death
        if (Battle_Singletons._battleResolutionStep.Out_Death_Events.Count > 0)
        {
            for (int i = 0; i < Battle_Singletons._battleResolutionStep.Out_Death_Events.Count; i++)
            {
                BattleEntity l_deadEntity = Battle_Singletons._battleResolutionStep.Out_Death_Events[i];
                BattleEntityComponent l_destroyedEntityComponent = BattleEntityComponent_Container.ComponentsByHandle[l_deadEntity];

                SceneGlobalObjects.PlayerTurnInputflow.on_battleEntityDeath(l_destroyedEntityComponent);

                //handling selection
                Battle_Singletons._battleActionSelection.on_battleEntityDeath(l_destroyedEntityComponent.BattleEntityHandle);

                //handling target selection
                Battle_Singletons._battleTargetSelection.on_battleEntityDeath(i);

                //We update the BattleTargetSelectionUI is the _battleTargetSelection has changed by taking into account death
                SceneGlobalObjects.BattleTargetSelectionUI.update(Battle_Singletons._battleTargetSelection);

                l_destroyedEntityComponent.Dispose();
            }
            Battle_Singletons._battleResolutionStep.Out_Death_Events.Clear();
        }

        BattleEntityComponent_Container.Update(l_delta);
    }

}
