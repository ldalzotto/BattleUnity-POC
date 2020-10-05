
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public static class AnimatorStateConstants
{
    public static readonly string Attack_Close_Prepare = "CharacterArmature|Attack_Prepare";
    public static readonly string Attack_Close_Slash = "CharacterArmature|Attack_Slash";
    public static readonly string Attack_Close_MoveBackward = "CharacterArmature|Attack_MoveBackward";
    public static readonly string Attack_Close_Idle = "CharacterArmature|Idle";
    public static readonly string Attack_Close_MoveForward = "CharacterArmature|Attack_MoveForward";
    public static readonly string Attack_Distance_MoveForward = "CharacterArmature|Attack_Distance_MoveForward";
    public static readonly string Attack_Distance_Slash = "CharacterArmature|Attack_Distance_Slash";

    public static readonly string Damage_Received = "CharacterArmature|Damage_Received";
}



public enum Anim_BattleAttack_Default_State
{
    End = 0,
    Preparing = 1,
    MovingForward = 2,
    Slashing = 3,
    MovingBackward = 4
}
public class Anim_BattleAttack_Default
{
    /* PARAMETERS */
    public BattleEntityComponent AnimatedTransform;
    public BattleEntityComponent TargetTransform;
    public AttackDefinition Attack;
    public Anim_BattleAttack_Default_Conf Conf;

    /*  */
    Vector3 InitalAnimatedTransform_Position;
    Quaternion InitalAnimatedTransform_Rotation;

    Vector3 TargetPosition_MovingForward;
    Vector3 Movement_ForwardDirection;

    float LastFrameDistace;

    public bool IsSlashAnimationTriggered;
    public bool IsSlashAnimationOver;
    public float StandingStill_AfterSlash_Timer;
    public Anim_BattleAttack_Default_State State;

    public void Initialize(BattleEntityComponent p_animatedTransform, BattleEntityComponent p_targetTransform, Anim_BattleAttack_Default_Conf p_animationConfiguration,
        AttackDefinition p_attack)
    {
        this.AnimatedTransform = p_animatedTransform;
        this.TargetTransform = p_targetTransform;
        this.Attack = p_attack;

        this.AnimatedTransform.AnimatorDispatcher.registerListener(this, Anim_BattleAttack_Default.OnAnimatorEvent);

        this.Conf = p_animationConfiguration;

        this.LastFrameDistace = Vector3.Distance(this.AnimatedTransform.transform.position, this.TargetTransform.transform.position);

        this.State = (Anim_BattleAttack_Default_State)1;
        this.AnimatedTransform.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Attack_Close_Prepare);

        this.InitalAnimatedTransform_Position = this.AnimatedTransform.transform.position;

        this.TargetPosition_MovingForward = this.AnimatedTransform.transform.position + ((this.LastFrameDistace - this.Conf.DistanceFromTarget) * Vector3.Normalize(this.TargetTransform.transform.position - this.AnimatedTransform.transform.position));

        this.Movement_ForwardDirection = Vector3.Normalize(this.TargetPosition_MovingForward - this.AnimatedTransform.transform.position);

        //Orient to transform to the target
        this.InitalAnimatedTransform_Rotation = p_animatedTransform.transform.rotation;
        p_animatedTransform.transform.rotation = Quaternion.LookRotation(this.Movement_ForwardDirection, Vector3.up);
    }

    public static void Update(BQE_Attack_UserDefined p_attackEvent, float delta)
    {
        Anim_BattleAttack_Default l_battleAttack = (Anim_BattleAttack_Default)p_attackEvent.Context_UserObject;
        switch (l_battleAttack.State)
        {
            case Anim_BattleAttack_Default_State.Preparing:
                {

                }
                break;
            case Anim_BattleAttack_Default_State.MovingForward:
                {
                    float l_distance = Vector3.Distance(l_battleAttack.AnimatedTransform.transform.position, l_battleAttack.TargetPosition_MovingForward);

                    if ((l_distance > l_battleAttack.LastFrameDistace) || (l_distance <= 0.001f))
                    {
                        // We terminate the movement
                        l_battleAttack.AnimatedTransform.transform.position = l_battleAttack.TargetPosition_MovingForward;
                        if (p_attackEvent.DamageSteps == null) { p_attackEvent.DamageSteps = new List<BaseDamageStep>(); }
                        p_attackEvent.DamageSteps.Add(BaseDamageStep.build(p_attackEvent.Source, p_attackEvent.Target, p_attackEvent.Attack));

                        l_battleAttack.State = Anim_BattleAttack_Default_State.Slashing;

                        l_battleAttack.LastFrameDistace = Vector3.Distance(l_battleAttack.InitalAnimatedTransform_Position, l_battleAttack.TargetPosition_MovingForward);
                        return;
                    }

                    float l_distanceRatio = 1.0f - (l_distance / Vector3.Distance(l_battleAttack.TargetPosition_MovingForward, l_battleAttack.InitalAnimatedTransform_Position));

                    l_battleAttack.AnimatedTransform.transform.position += l_battleAttack.Movement_ForwardDirection * l_battleAttack.Conf.AnimatedTransform_Speed_V2.Evaluate(l_distanceRatio) * l_battleAttack.Conf.AnimatedTransform_Speed_Forward * delta;
                    l_battleAttack.LastFrameDistace = l_distance;

                    if (!l_battleAttack.IsSlashAnimationTriggered && l_distanceRatio >= l_battleAttack.Conf.DistanceRatio_MoveForward_StartMovingLegs)
                    {
                        l_battleAttack.IsSlashAnimationTriggered = true;
                        l_battleAttack.AnimatedTransform.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Attack_Close_Slash);
                    }
                }
                break;
            case Anim_BattleAttack_Default_State.Slashing:
                {
                    if (l_battleAttack.IsSlashAnimationOver)
                    {
                        l_battleAttack.StandingStill_AfterSlash_Timer += delta;
                        if (l_battleAttack.StandingStill_AfterSlash_Timer >= l_battleAttack.Conf.TimeStandingStill_AfterSlashing)
                        {
                            l_battleAttack.AnimatedTransform.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Attack_Close_MoveBackward);
                            l_battleAttack.State = Anim_BattleAttack_Default_State.MovingBackward;
                            Update(p_attackEvent, delta);
                        }
                    }
                }
                break;
            case Anim_BattleAttack_Default_State.MovingBackward:
                {

                    float l_distance = Vector3.Distance(l_battleAttack.AnimatedTransform.transform.position, l_battleAttack.InitalAnimatedTransform_Position);

                    if ((l_distance > l_battleAttack.LastFrameDistace) || (l_distance <= 0.001f))
                    {
                        // We terminate the movement
                        l_battleAttack.AnimatedTransform.transform.position = l_battleAttack.InitalAnimatedTransform_Position;
                        l_battleAttack.AnimatedTransform.transform.rotation = l_battleAttack.InitalAnimatedTransform_Rotation;
                        p_attackEvent.HasEnded = true;
                        l_battleAttack.State = Anim_BattleAttack_Default_State.End;
                        l_battleAttack.AnimatedTransform.AnimatorDispatcher.Animator.CrossFadeInFixedTime(AnimatorStateConstants.Attack_Close_Idle, 0.1f);
                        return;
                    }

                    l_battleAttack.AnimatedTransform.transform.position += (-l_battleAttack.Movement_ForwardDirection) * l_battleAttack.Conf.AnimatedTransform_Speed_Backward * delta;

                    l_battleAttack.LastFrameDistace = l_distance;
                }
                break;
        }

        return;
    }

    private static void OnAnimatorEvent(object p_battleAttack, AnimationEvent_Type p_event)
    {
        Anim_BattleAttack_Default l_attack = (Anim_BattleAttack_Default)p_battleAttack;
        switch (p_event)
        {
            case AnimationEvent_Type.CharacterArmature_Attack_Begin_END:
                {
                    if (l_attack.State == Anim_BattleAttack_Default_State.Preparing)
                    {
                        l_attack.AnimatedTransform.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Attack_Close_MoveForward);
                        l_attack.State = Anim_BattleAttack_Default_State.MovingForward;
                    }
                }
                break;
            case AnimationEvent_Type.CharacterArmature_Attack_End_END:
                {
                    l_attack.IsSlashAnimationOver = true;
                }
                break;
        }
    }
}

public enum Anim_BattleAttack_Distance_State
{
    End = 0,
    MovingForward = 1,
    Slashing = 2,
}

public class Anim_BattleAttack_Distance
{
    BattleEntityComponent AnimatedTransform;
    BattleEntityComponent TargetTransform;

    Anim_BattleAttack_Distance_State State;
    Quaternion InitalAnimatedTransform_Rotation;

    public void Initialize(BattleEntityComponent p_animatedTransform, BattleEntityComponent p_targetTransform)
    {
        this.AnimatedTransform = p_animatedTransform;
        this.TargetTransform = p_targetTransform;
        p_animatedTransform.AnimatorDispatcher.registerListener(this, OnAnimatorEvent);
        p_animatedTransform.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Attack_Distance_MoveForward);
        this.State = Anim_BattleAttack_Distance_State.MovingForward;
        this.InitalAnimatedTransform_Rotation = p_animatedTransform.transform.rotation;
        p_animatedTransform.transform.rotation = Quaternion.LookRotation((p_targetTransform.transform.position - p_animatedTransform.transform.position).normalized, Vector3.up);
    }

    public static void Update(BQE_Attack_UserDefined p_attackEvent, float d)
    {
        Anim_BattleAttack_Distance l_battleAttack = (Anim_BattleAttack_Distance)p_attackEvent.Context_UserObject;
        switch (l_battleAttack.State)
        {
            case Anim_BattleAttack_Distance_State.End:
                {
                    if (p_attackEvent.DamageSteps == null) { p_attackEvent.DamageSteps = new List<BaseDamageStep>(); }
                    p_attackEvent.DamageSteps.Add(BaseDamageStep.build(p_attackEvent.Source, p_attackEvent.Target, p_attackEvent.Attack));
                    l_battleAttack.AnimatedTransform.transform.rotation = l_battleAttack.InitalAnimatedTransform_Rotation;

                    p_attackEvent.HasEnded = true;
                }
                break;
        }
    }

    private static void OnAnimatorEvent(object p_battleAttack, AnimationEvent_Type p_event)
    {
        Anim_BattleAttack_Distance l_attack = (Anim_BattleAttack_Distance)p_battleAttack;
        switch (p_event)
        {
            case AnimationEvent_Type.CharacterArmature_Attack_Distance_MoveForward_END:
                {
                    l_attack.State = Anim_BattleAttack_Distance_State.Slashing;
                    l_attack.AnimatedTransform.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Attack_Distance_Slash);
                }
                break;
            case AnimationEvent_Type.CharacterArmature_Attack_Distance_Slash_END:
                {
                    l_attack.State = Anim_BattleAttack_Distance_State.End;
                    l_attack.AnimatedTransform.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Attack_Close_Idle);
                }
                break;
        }
    }
}

public static class BattleAnimation
{
    public static Initialize_ReturnCode initialize_attackAnimation(BattleQueueEvent p_event)
    {
        switch (p_event.Type)
        {
            case BattleQueueEvent_Type.ATTACK:
                {
                    BQE_Attack_UserDefined l_event = (BQE_Attack_UserDefined)p_event.Event;
                    if (!l_event.Source.IsDead && !l_event.Target.IsDead)
                    {
                        switch (l_event.Attack.AttackType)
                        {
                            case Attack_Type.DEFAULT:
                                {
                                    switch (l_event.Source.Type)
                                    {
                                        case BattleEntity_Type.DEFAULT:
                                            {
                                                BattleEntityComponent l_battleEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Source];
                                                l_event.Context_UserObject = new Anim_BattleAttack_Default();
                                                ((Anim_BattleAttack_Default)l_event.Context_UserObject).Initialize(BattleEntityComponent_Container.ComponentsByHandle[l_event.Source],
                                                    BattleEntityComponent_Container.ComponentsByHandle[l_event.Target], SceneGlobalObjects.AnimationConfiguration.Anim_BattleAttack_Default, l_event.Attack);
                                                l_battleEntity.BattleAnimations.push_attackAnimation(l_event, Anim_BattleAttack_Default.Update);
                                                return Initialize_ReturnCode.NEEDS_TO_BE_PROCESSED;
                                            }
                                        case BattleEntity_Type.SOLIDER_MACHINEGUN_0:
                                            {
                                                BattleEntityComponent l_battleEntity = BattleEntityComponent_Container.ComponentsByHandle[l_event.Source];
                                                l_event.Context_UserObject = new Anim_BattleAttack_Distance();
                                                ((Anim_BattleAttack_Distance)l_event.Context_UserObject).Initialize(BattleEntityComponent_Container.ComponentsByHandle[l_event.Source],
                                                    BattleEntityComponent_Container.ComponentsByHandle[l_event.Target]);
                                                l_battleEntity.BattleAnimations.push_attackAnimation(l_event, Anim_BattleAttack_Distance.Update);
                                                return Initialize_ReturnCode.NEEDS_TO_BE_PROCESSED;
                                            }
                                    }
                                }
                                break;
                        }
                    }
                }
                break;
        }
        return Initialize_ReturnCode.NOTHING;
    }

    public static void onDamageReceived(BattleEntityComponent p_target)
    {
        switch (p_target.BattleEntityHandle.Type)
        {
            default:
                {
                    p_target.AnimatorDispatcher.Animator.Play(AnimatorStateConstants.Damage_Received);
                }
                break;
        }
    }
}