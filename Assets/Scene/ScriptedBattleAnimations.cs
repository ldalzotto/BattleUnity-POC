
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Anim_BattleAttack_Default_State
{
    End = 0,
    MovingForward = 1,
    MovingBackward = 2
}

public class Anim_BattleAttack_Default
{
    /* PARAMETERS */
    public Transform AnimatedTransform;
    public Anim_BattleAttack_Default_Conf Conf;
    public Transform TargetTransform;

    /*  */
    Vector3 InitalAnimatedTransform_Position;
    Vector3 TargetPosition_MovingForward;

    float LastFrameDistace;
    public Anim_BattleAttack_Default_State State;

    public void Initialize(Transform p_animatedTransform, Transform p_targetTransform, AnimationConfiguration p_animationConfiguration)
    {
        this.AnimatedTransform = p_animatedTransform;
        this.TargetTransform = p_targetTransform;

        this.Conf = p_animationConfiguration.Anim_BattleAttack_Default;

        this.LastFrameDistace = Vector3.Distance(this.AnimatedTransform.position, this.TargetTransform.position);
        this.State = Anim_BattleAttack_Default_State.MovingForward;
        this.InitalAnimatedTransform_Position = this.AnimatedTransform.position;
        this.TargetPosition_MovingForward = this.AnimatedTransform.position + ((this.LastFrameDistace - this.Conf.DistanceFromTarget) * Vector3.Normalize(this.TargetTransform.position - this.AnimatedTransform.position));
    }

    public static void Update(BQE_Attack_UserDefined p_attackEvent, float delta)
    {
        Anim_BattleAttack_Default l_battleAttack = (Anim_BattleAttack_Default)p_attackEvent.Context_UserObject;
        switch (l_battleAttack.State)
        {
            case Anim_BattleAttack_Default_State.MovingForward:
                {
                    float l_distance = Vector3.Distance(l_battleAttack.AnimatedTransform.position, l_battleAttack.TargetPosition_MovingForward);

                    if ((l_distance > l_battleAttack.LastFrameDistace) || (l_distance <= 0.001f))
                    {
                        // We terminate the movement
                        l_battleAttack.AnimatedTransform.position = l_battleAttack.TargetPosition_MovingForward;
                        if (p_attackEvent.DamageSteps == null) { p_attackEvent.DamageSteps = new List<AttackEvent_DamageStep>(); }
                        p_attackEvent.DamageSteps.Add(new AttackEvent_DamageStep() { Target = p_attackEvent.Target });
                        l_battleAttack.State = Anim_BattleAttack_Default_State.MovingBackward;

                        l_battleAttack.LastFrameDistace = Vector3.Distance(l_battleAttack.InitalAnimatedTransform_Position, l_battleAttack.TargetPosition_MovingForward);
                        return;
                    }

                    Vector3 l_direction = Vector3.Normalize(l_battleAttack.TargetPosition_MovingForward - l_battleAttack.AnimatedTransform.position);

                    float l_distanceRatio = 1.0f - (l_distance / Vector3.Distance(l_battleAttack.TargetPosition_MovingForward, l_battleAttack.InitalAnimatedTransform_Position));

                    l_battleAttack.AnimatedTransform.position += l_direction * l_battleAttack.Conf.AnimatedTransform_Speed_V2.Evaluate(l_distanceRatio) * l_battleAttack.Conf.AnimatedTransform_Speed * delta;

                    l_battleAttack.LastFrameDistace = l_distance;
                }
                return;
            case Anim_BattleAttack_Default_State.MovingBackward:
                {

                    float l_distance = Vector3.Distance(l_battleAttack.AnimatedTransform.position, l_battleAttack.InitalAnimatedTransform_Position);

                    if ((l_distance > l_battleAttack.LastFrameDistace) || (l_distance <= 0.001f))
                    {
                        // We terminate the movement
                        l_battleAttack.AnimatedTransform.position = l_battleAttack.InitalAnimatedTransform_Position;
                        p_attackEvent.HasEnded = true;
                        l_battleAttack.State = Anim_BattleAttack_Default_State.End;
                        return;
                    }

                    Vector3 l_direction = Vector3.Normalize(l_battleAttack.InitalAnimatedTransform_Position - l_battleAttack.TargetPosition_MovingForward);
                    l_battleAttack.AnimatedTransform.position += l_direction * l_battleAttack.Conf.AnimatedTransform_Speed * delta;

                    l_battleAttack.LastFrameDistace = l_distance;
                }
                return;
        }

        return;
    }
}

public static class BattleAnimation_Initialize
{
    public static Initialize_ReturnCode init(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    BQE_Attack_UserDefined l_event = (BQE_Attack_UserDefined)p_event.Event;
                    if (!l_event.Source.IsDead && !l_event.Target.IsDead)
                    {
                        switch (l_event.AttackType)
                        {
                            case Attack_Type.DEFAULT:
                                {
                                    BattleEntityComponent l_battleEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Source];
                                    l_event.Context_UserObject = new Anim_BattleAttack_Default();
                                    ((Anim_BattleAttack_Default)l_event.Context_UserObject).Initialize(BattleEntityComponent_Container.ComponentsByHandle[l_event.Source].transform,
                                        BattleEntityComponent_Container.ComponentsByHandle[l_event.Target].transform, SceneGlobalObjects.AnimationConfiguration);
                                    l_battleEntity.BattleAnimations.push_attackAnimation(l_event, Anim_BattleAttack_Default.Update);
                                    return Initialize_ReturnCode.NEEDS_TO_BE_PROCESSED;
                                }
                        }
                    }
                }
                break;
        }
        return Initialize_ReturnCode.NOTHING;
    }
}