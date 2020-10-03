
using System;
using UnityEngine;

public enum AnimationEvent_Type
{
    CharacterArmature_Attack_Begin_END = 0,
    CharacterArmature_Attack_End_END = 1
}

public struct AnimatorEventDispatcher_Callback
{
    public object Closure;
    public Action<object, AnimationEvent_Type> Callback;
}

[RequireComponent(typeof(Animator))]
public class AnimatorEventDispatcherComponent : MonoBehaviour
{
    public Animator Animator;
    public AnimatorEventDispatcher_Callback Listener;

    private void Start()
    {
        this.Animator = GetComponent<Animator>();
    }

    public void registerListener(object p_closure, Action<object, AnimationEvent_Type> p_callback)
    {
        this.Listener.Closure = p_closure;
        this.Listener.Callback = p_callback;
    }

    public void CharacterArmature_Attack_Begin_END()
    {
        if (this.Listener.Callback != null)
        {
            this.Listener.Callback.Invoke(this.Listener.Closure, AnimationEvent_Type.CharacterArmature_Attack_Begin_END);
        }
    }

    public void CharacterArmature_Attack_Slash_END()
    {
        if (this.Listener.Callback != null)
        {
            this.Listener.Callback.Invoke(this.Listener.Closure, AnimationEvent_Type.CharacterArmature_Attack_End_END);
        }
    }

}

