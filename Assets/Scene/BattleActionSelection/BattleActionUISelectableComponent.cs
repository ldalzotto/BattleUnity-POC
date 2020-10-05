using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleActionUISelectableComponent : MonoBehaviour
{
    public Text Text;
    public bool IsSelected;

    private void OnEnable()
    {
        this.Text = GetComponent<Text>();
    }

    public void select()
    {
        this.Text.color = Color.red;
        this.IsSelected = true;
    }

    public void unselect()
    {
        this.Text.color = Color.black;
        this.IsSelected = false;
    }
}
