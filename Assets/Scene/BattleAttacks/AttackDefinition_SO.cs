
using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "AttackDefinition", menuName = "Attack/AttackDefinition", order = 1)]
public class AttackDefinition_SO : ScriptableObject
{
    public AttackDefinition AttackDefinition;
}

public static class BattleAttackConfiguration_Algorithm
{

    public static AttackDefinition find_defaultAttackDefinition(BattleEntityComponent p_entityComponent)
    {
        switch (p_entityComponent.BattleEntityHandle.Type)
        {
            case BattleEntity_Type.PLAYER_1:
                return ((PLAYER_1_conf)p_entityComponent.BattleEntityConfiguration).DefaultAttack.AttackDefinition;
            case BattleEntity_Type.PLAYER_2:
                return ((PLAYER_2_conf)p_entityComponent.BattleEntityConfiguration).DefaultAttack.AttackDefinition;
        }

        return new AttackDefinition();
    }

}