using System.Collections.Generic;
using UnityEngine;

public class ATB_UI
{
    // private GameObject ATB_Bars_GO;
    private GameObject ATB_Bars_Background_GO;
    private ATB_UIComponent ATB_Line_Prefab;

    private List<ATB_UIComponent> ATB_Lines;

    public void Initialize(ATB_UIComponent p_atb_line_prefab, BattleResolutionStep p_battle)
    {
        this.ATB_Lines = new List<ATB_UIComponent>();
        this.ATB_Bars_Background_GO = GameObject.FindGameObjectWithTag(Tags.ATB_Bars_Background);
        this.ATB_Line_Prefab = p_atb_line_prefab;

        for (int i = 0; i < p_battle.BattleEntities.Count; i++)
        {
            ATB_UIComponent l_atb_line = GameObject.Instantiate(this.ATB_Line_Prefab, this.ATB_Bars_Background_GO.transform);
            RectTransform l_transform = (RectTransform)l_atb_line.transform;
            l_transform.anchoredPosition = new Vector2(l_transform.anchoredPosition.x, -5 - (i * l_transform.sizeDelta.y)) ;
            l_atb_line.Initialize(p_battle.BattleEntities[i]);
            this.ATB_Lines.Add(l_atb_line);
        }
    }

    public void Update(float d)
    {
        for (int i = 0; i < this.ATB_Lines.Count; i++)
        {
            this.ATB_Lines[i].Text.text = string.Format("{0} / 1.000", this.ATB_Lines[i].AssociatedEntity.ATB_Value);
        }
    }
}
