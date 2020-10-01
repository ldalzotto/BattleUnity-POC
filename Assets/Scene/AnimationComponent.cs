using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationComponent : MonoBehaviour
{
    public Anim_BattleAttack_Default AnimBattle;

    private void Start()
    {
        this.AnimBattle.State = Anim_BattleAttack_Default_State.End;
        // this.AnimBattle.Initialize();
    }

    public void InitializeAnimation(Transform p_target)
    {
        this.AnimBattle.AnimatedTransform = transform;
        this.AnimBattle.TargetTransform = p_target;
        this.AnimBattle.Initialize();
    }

    private void Update()
    {
        this.AnimBattle.Update(Time.deltaTime);
    }
}
