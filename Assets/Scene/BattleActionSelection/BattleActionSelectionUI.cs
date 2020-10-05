using System.Runtime.CompilerServices;
using UnityEngine;

public class BattleActionSelectionUI
{
    private BattleActionSelectionUIGameObject BattleActionSelectionUIGameObject;
    public bool isEnabled;

    public static BattleActionSelectionUI alloc(RectTransform p_actionSelectionMenuPrefab)
    {
        return new BattleActionSelectionUI() { BattleActionSelectionUIGameObject = BattleActionSelectionUIGameObject.build(p_actionSelectionMenuPrefab) };
    }

    public void enable()
    {
        this.BattleActionSelectionUIGameObject.enable();
        this.isEnabled = true;
    }

    public void disable()
    {
        this.BattleActionSelectionUIGameObject.disable();
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