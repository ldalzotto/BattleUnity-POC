using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleEntityComponent_Container
{
    public static Dictionary<BattleEntity, BattleEntityComponent> ComponentsByHandle = new Dictionary<BattleEntity, BattleEntityComponent>();
}

public class BattleEntityComponent : MonoBehaviour
{
    public BattleEntity_Team Team;
    public float ATB_Speed;

    BattleEntity BattleEntityHandle;
    
    /* Internal components */
    public AnimationComponent AnimationComponent;

    public void Initialize(AnimationConfiguration p_animationConfiguration)
    {
        this.AnimationComponent = this.gameObject.AddComponent<AnimationComponent>();
        this.AnimationComponent.AnimBattle.Conf = p_animationConfiguration.Anim_BattleAttack_Default;
        /* .AnimatedTransform_Speed = 15.0f */

        this.BattleEntityHandle = BattleEntity.Alloc();
        this.BattleEntityHandle.Team = this.Team;
        this.BattleEntityHandle.ATB_Speed = this.ATB_Speed;
        Battle_Singletons._battle.push_battleEntity(this.BattleEntityHandle);
        BattleEntityComponent_Container.ComponentsByHandle.Add(this.BattleEntityHandle, this);
    }
}
