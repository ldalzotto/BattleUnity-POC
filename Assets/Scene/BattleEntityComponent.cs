using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleEntityComponent_Container
{
    public static Dictionary<int, BattleEntityComponent> ComponentsByHandle = new Dictionary<int, BattleEntityComponent>();
}

public class BattleEntityComponent : MonoBehaviour
{
    public BattleEntity_Team Team;
    public float ATB_Speed;

    BattleEntity_Handle BattleEntityHandle;
    
    /* Internal components */
    public AnimationComponent AnimationComponent;

    public void Initialize()
    {
        this.AnimationComponent = this.gameObject.AddComponent<AnimationComponent>();
        this.AnimationComponent.AnimBattle.AnimatedTransform_Speed = 15.0f;

        BattleEntity l_entity = new BattleEntity();
        l_entity.Team = this.Team;
        l_entity.ATB_Speed = this.ATB_Speed;
        this.BattleEntityHandle = Battle_Singletons._battle.push_battleEntity(ref l_entity);
        BattleEntityComponent_Container.ComponentsByHandle.Add(this.BattleEntityHandle.Handle, this);
    }
}
