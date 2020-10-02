using UnityEngine;
using UnityEngine.UI;

public class ATB_UIComponent : MonoBehaviour
{
    public BattleEntity AssociatedEntity;
    public Text Text;
    public void Initialize(BattleEntity p_associatedEntity)
    {
        this.AssociatedEntity = p_associatedEntity;
        this.Text = GetComponent<Text>();
    }
}