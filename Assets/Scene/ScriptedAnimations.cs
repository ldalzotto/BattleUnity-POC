
using System;
using UnityEngine;

public enum Anim_BattleAttack_Default_State
{
    End = 0,
    MovingForward = 1,
    MovingBackward = 2
}

[Serializable]
public struct Anim_BattleAttack_Default
{
    /* PARAMETERS */
    public Transform AnimatedTransform;
    public float AnimatedTransform_Speed;
    public Transform TargetTransform;

    /*  */
    Vector3 InitalAnimatedTransform_Position;
    float LastFrameDistace;
    public Anim_BattleAttack_Default_State State;

    public void Initialize()
    {
        this.LastFrameDistace = Vector3.Distance(this.AnimatedTransform.position, this.TargetTransform.position);
        this.State = Anim_BattleAttack_Default_State.MovingForward;
        this.InitalAnimatedTransform_Position = this.AnimatedTransform.position;
    }

    public void Update(float delta)
    {
        switch (this.State)
        {
            case Anim_BattleAttack_Default_State.MovingForward:
                {
                    float l_distance = Vector3.Distance(this.AnimatedTransform.position, this.TargetTransform.position);

                    if (l_distance > this.LastFrameDistace)
                    {
                        // We terminate the movement
                        this.AnimatedTransform.position = this.TargetTransform.position;
                        Debug.Log("BOOM");
                        this.State = Anim_BattleAttack_Default_State.MovingBackward;

                        this.LastFrameDistace = Vector3.Distance(this.InitalAnimatedTransform_Position, this.TargetTransform.position);
                        return;
                    }

                    Vector3 l_direction = Vector3.Normalize(this.TargetTransform.position - this.AnimatedTransform.position);
                    this.AnimatedTransform.position += l_direction * this.AnimatedTransform_Speed * delta;

                    this.LastFrameDistace = l_distance;
                }
                return;
            case Anim_BattleAttack_Default_State.MovingBackward:
                {

                    float l_distance = Vector3.Distance(this.AnimatedTransform.position, this.InitalAnimatedTransform_Position);

                    if (l_distance > this.LastFrameDistace)
                    {
                        // We terminate the movement
                        this.AnimatedTransform.position = this.InitalAnimatedTransform_Position;
                        this.State = Anim_BattleAttack_Default_State.End;
                        return;
                    }

                    Vector3 l_direction = Vector3.Normalize(this.InitalAnimatedTransform_Position - this.TargetTransform.position);
                    this.AnimatedTransform.position += l_direction * this.AnimatedTransform_Speed * delta;

                    this.LastFrameDistace = l_distance;
                }
                return;
        }

        return;
    }
}