using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleEntityComponent_Container
{
    public static Dictionary<int, BattleEntityComponent> ComponentsByHandle = new Dictionary<int, BattleEntityComponent>();
}

public class BattleEntityComponent : MonoBehaviour
{
    public BattleEntity_Handle BattleEntityHandle;
    public float ATB_Speed;

    void Start()
    {
        BattleEntity l_entity = new BattleEntity();
        l_entity.ATB_Speed = this.ATB_Speed;
        this.BattleEntityHandle = Battle.get_singleton().push_battleEntity(ref l_entity);
        BattleEntityComponent_Container.ComponentsByHandle.Add(this.BattleEntityHandle.Handle, this);
    }
}
