using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationComponent : MonoBehaviour
{
    // public Anim_BattleAttack_Default AnimBattle;

    public BQE_Attack_UserDefined CurrentAnimationObject;
    public Action<BQE_Attack_UserDefined, float> AnimationUpdateFunction;

    public void push_attackAnimation(BQE_Attack_UserDefined p_attackAnimation, Action<BQE_Attack_UserDefined, float> p_animationUpdateFunction)
    {
        this.CurrentAnimationObject = p_attackAnimation;
        this.AnimationUpdateFunction = p_animationUpdateFunction;
    }


    private void Update()
    {
        if(this.AnimationUpdateFunction != null && this.CurrentAnimationObject != null)
        {
            this.AnimationUpdateFunction.Invoke(this.CurrentAnimationObject, Time.deltaTime);
            if(this.CurrentAnimationObject.HasEnded)
            {
                this.AnimationUpdateFunction = null;
                this.CurrentAnimationObject = null;
            }
        }
    }
}
