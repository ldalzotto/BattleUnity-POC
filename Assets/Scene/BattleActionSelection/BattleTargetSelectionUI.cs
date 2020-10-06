using UnityEngine;

public class BattleTargetSelectionUI
{
    public bool IsEnabled;
    private BattleTargetSelectionUIGameObject BattleTargetSelectionUIGameObject;
    private BattleEntityComponent CurrentSelectedBattleEntityComponent;

    public static BattleTargetSelectionUI alloc(GameObject p_battleTargetSelectionUIGameObjectPrefab)
    {
        BattleTargetSelectionUI l_ui = new BattleTargetSelectionUI();
        l_ui.BattleTargetSelectionUIGameObject = BattleTargetSelectionUIGameObject.build(p_battleTargetSelectionUIGameObjectPrefab);
        l_ui.disable();
        return l_ui;
    }

    public void update(BattleTargetSelection p_battleTargetSelection)
    {
        if (p_battleTargetSelection.CurrentlySelectedEntity_HasChanged)
        {
            if (p_battleTargetSelection.CurrentlySelectedEntity_BattleIndex == BattleTargetSelection.CurrentlySelectedEntity_BattleIndex_None)
            {
                if (this.IsEnabled)
                {
                    this.disable();
                }
                this.CurrentSelectedBattleEntityComponent = null;
            }
            else
            {
                if (!this.IsEnabled)
                {
                    this.enable();
                }

                this.CurrentSelectedBattleEntityComponent = BattleEntityComponent_Container.ComponentsByHandle[
                        Battle_Singletons._battleResolutionStep._battle.BattleEntities[p_battleTargetSelection.CurrentlySelectedEntity_BattleIndex]
                    ];
                ;
            }
        }

        if (this.CurrentSelectedBattleEntityComponent != null)
        {
            this.BattleTargetSelectionUIGameObject.InstanciatedBattleTargetSelectionUIGameObject.transform.position = this.CurrentSelectedBattleEntityComponent.AboveHead_Transform.position;
        }
    }

    public void enable()
    {
        this.IsEnabled = true;
        this.BattleTargetSelectionUIGameObject.enable();
    }

    public void disable()
    {
        this.IsEnabled = false;
        this.BattleTargetSelectionUIGameObject.disable();
    }
}

public struct BattleTargetSelectionUIGameObject
{
    public GameObject InstanciatedBattleTargetSelectionUIGameObject;

    public static BattleTargetSelectionUIGameObject build(GameObject p_battleTargetSelectionUIGameObjectPrefab)
    {
        BattleTargetSelectionUIGameObject l_instance = new BattleTargetSelectionUIGameObject();
        l_instance.InstanciatedBattleTargetSelectionUIGameObject = GameObject.Instantiate(p_battleTargetSelectionUIGameObjectPrefab);
        return l_instance;
    }

    public void enable()
    {
        this.InstanciatedBattleTargetSelectionUIGameObject.SetActive(true);
    }

    public void disable()
    {
        this.InstanciatedBattleTargetSelectionUIGameObject.SetActive(false);
    }
}