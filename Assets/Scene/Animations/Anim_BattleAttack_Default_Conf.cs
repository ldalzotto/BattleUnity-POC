using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "Anim_BattleAttack_Default_Conf", menuName = "Animations/Anim_BattleAttack_Default_Conf", order = 1)]
public class Anim_BattleAttack_Default_Conf : ScriptableObject
{
    public AnimationCurve AnimatedTransform_Speed_V2;
    public float AnimatedTransform_Speed;
    public float DistanceFromTarget;
}