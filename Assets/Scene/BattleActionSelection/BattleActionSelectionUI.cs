using System.Runtime.CompilerServices;
using UnityEngine;

public class BattleActionSelectionUI
{
    private BattleActionSelectionUIGameObject BattleActionSelectionUIGameObject;
    private GameObject Cursor;
    public bool isEnabled;
    private BattleEntityComponent currentlySelectedbattleEntityComponent;

    public static BattleActionSelectionUI alloc(RectTransform p_actionSelectionMenuPrefab, GameObject p_currentBattleActionSelectionEntityCursorPrefab)
    {
        BattleActionSelectionUI l_ui = new BattleActionSelectionUI();
        l_ui.BattleActionSelectionUIGameObject = BattleActionSelectionUIGameObject.build(p_actionSelectionMenuPrefab);
        l_ui.Cursor = GameObject.Instantiate(p_currentBattleActionSelectionEntityCursorPrefab);
        l_ui.disable();
        return l_ui;
    }

    public void enable()
    {
        this.BattleActionSelectionUIGameObject.enable();
        this.Cursor.SetActive(true);
        this.isEnabled = true;
    }

    public void update(BattleActionSelection p_battleActionSelection)
    {
        if(p_battleActionSelection.CurrentlySelectedEntity_HasChanged)
        {
            if(p_battleActionSelection.CurrentlySelectedEntity != null)
            {
                this.currentlySelectedbattleEntityComponent = BattleEntityComponent_Container.ComponentsByHandle[p_battleActionSelection.CurrentlySelectedEntity];
            }
            else
            {
                this.currentlySelectedbattleEntityComponent = null;
            }
        }

        //update transform
        if (this.currentlySelectedbattleEntityComponent != null)
        {
            this.Cursor.transform.position = this.currentlySelectedbattleEntityComponent.AboveHead_Transform.transform.position;
        }
    }

    public void disable()
    {
        this.BattleActionSelectionUIGameObject.disable();
        this.Cursor.SetActive(false);
        this.isEnabled = false;
    }
}

public struct BattleActionSelectionUIGameObject
{
    private GameObject InstanciatedUI;
    public BattleActionUISelectableComponent AttackSection;

    public static BattleActionSelectionUIGameObject build(RectTransform p_actionSelectionMenuPrefab)
    {
        BattleActionSelectionUIGameObject l_go = new BattleActionSelectionUIGameObject();
        l_go.InstanciatedUI = GameObject.Instantiate(p_actionSelectionMenuPrefab.gameObject, SceneGlobalObjects.MainCanvas.transform);
        l_go.AttackSection = l_go.InstanciatedUI.GetComponentInChildren<BattleActionUISelectableComponent>();
        l_go.disable();
        return l_go;
    }
    public void enable()
    {
        this.InstanciatedUI.SetActive(true);
        this.AttackSection.select();
    }

    public void disable()
    {
        this.InstanciatedUI.SetActive(false);
        this.AttackSection.unselect();
    }
}