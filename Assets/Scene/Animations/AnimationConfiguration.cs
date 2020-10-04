using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "AnimationConfiguration", menuName = "Animations/AnimationConfiguration", order = 1)]
public class AnimationConfiguration : ScriptableObject
{
    public Anim_BattleAttack_Default_Conf Anim_BattleAttack_Default;
    public Anim_BattleAttack_Default_Conf Anim_BattleAttack_MachineGunSoldier;
}
