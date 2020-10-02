using UnityEngine;
using UnityEngine.UI;

public class ATB_UIComponent : MonoBehaviour
{
    public BattleEntity_Handle AssociatedEntityHandle;
    public Text Text;
    public void Initialize(BattleEntity_Handle p_associatedEntityHandle)
    {
        this.AssociatedEntityHandle = p_associatedEntityHandle;
        this.Text = GetComponent<Text>();
    }
}