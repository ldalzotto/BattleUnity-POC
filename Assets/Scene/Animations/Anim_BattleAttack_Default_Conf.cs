using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
[CreateAssetMenu(fileName = "Anim_BattleAttack_Default_Conf", menuName = "Animations/Anim_BattleAttack_Default_Conf", order = 1)]
public class Anim_BattleAttack_Default_Conf : ScriptableObject
{
    [Tooltip("Speed multipliation factor based on the remaining normalized forward travel distance.")]
    public AnimationCurve AnimatedTransform_Speed_V2;

    [Tooltip("Speed multipliation factor for forward movement.")]
    public float AnimatedTransform_Speed_Forward;

    [Tooltip("Speed multipliation factor for backward movement.")]
    public float AnimatedTransform_Speed_Backward;

    [Tooltip("If 1, the animated Transform will move to the exact position of the target position. " +
        "When < 1, this means that the transform will stop before the exact point.")]
    public float DistanceFromTarget;

    [Tooltip("The normalized distance from with slash animation starts.")]
    public float DistanceRatio_MoveForward_StartMovingLegs;

    [Tooltip("Time in seconds where the animation is frozen after the slashing animation.")]
    public float TimeStandingStill_AfterSlashing;
}