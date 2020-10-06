using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleEntityComponent_Container
{
    public static Dictionary<BattleEntity, BattleEntityComponent> ComponentsByHandle = new Dictionary<BattleEntity, BattleEntityComponent>();

    public static void Update(float d)
    {
        foreach (var l_entry in ComponentsByHandle)
        {
            l_entry.Value.Tick(d);
        }
    }
}

public class BattleEntityComponent : MonoBehaviour
{
    public BattleEntity_Type Type;
    public BattleEntity_Team Team;
    public BattleEntity_BaseStats Stats;
    public float ATB_Speed;
    public bool IsControlledByPlayer;
    public int InitialHealth;

    public BattleEntityConfiguration_Abstract BattleEntityConfiguration;

    public Transform DamageDisplay_Transform;
    public Transform AboveHead_Transform;
    public BattleEntity BattleEntityHandle;

    /* Internal components */
    public BattleEntityComponent_Animation BattleAnimations;
    public AnimatorEventDispatcherComponent AnimatorDispatcher;


    public void Initialize()
    {
        this.BattleEntityHandle = BattleEntity.Alloc();
        this.BattleEntityHandle.Type = this.Type;
        this.BattleEntityHandle.Team = this.Team;
        this.BattleEntityHandle.Stats = this.Stats;
        this.BattleEntityHandle.IsControlledByPlayer = this.IsControlledByPlayer;
        this.BattleEntityHandle.ATB_Speed = this.ATB_Speed;
        this.BattleEntityHandle.Life = this.InitialHealth;
        this.AnimatorDispatcher = this.GetComponentInChildren<AnimatorEventDispatcherComponent>();

        Battle_Singletons._battleResolutionStep.push_battleEntity(this.BattleEntityHandle);
        BattleEntityComponent_Container.ComponentsByHandle.Add(this.BattleEntityHandle, this);
    }

    public void Tick(float d)
    {
        this.BattleAnimations.Update(d);
    }

    public void Dispose()
    {
        BattleEntityComponent_Container.ComponentsByHandle.Remove(this.BattleEntityHandle);
        GameObject.Destroy(this.gameObject);
    }
}


public struct BattleEntityComponent_Animation
{
    public BQE_Attack CurrentAnimationObject;
    public Action<BQE_Attack, float> AnimationUpdateFunction;

    public void push_attackAnimation(BQE_Attack p_attackAnimation, Action<BQE_Attack, float> p_animationUpdateFunction)
    {
        this.CurrentAnimationObject = p_attackAnimation;
        this.AnimationUpdateFunction = p_animationUpdateFunction;
    }


    public void Update(float d)
    {
        if (this.AnimationUpdateFunction != null && this.CurrentAnimationObject != null)
        {
            this.AnimationUpdateFunction.Invoke(this.CurrentAnimationObject, d);
            if (this.CurrentAnimationObject.HasEnded)
            {
                this.AnimationUpdateFunction = null;
                this.CurrentAnimationObject = null;
            }
        }
    }
}
