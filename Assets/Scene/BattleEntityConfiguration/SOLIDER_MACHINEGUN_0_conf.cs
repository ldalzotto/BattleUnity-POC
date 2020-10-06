using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "SOLIDER_MACHINEGUN_0_conf", menuName = "Entity/SOLIDER_MACHINEGUN_0", order = 1)]
public class SOLIDER_MACHINEGUN_0_conf : BattleEntityConfiguration_Abstract
{
    public AttackDefinition_SO DefaultAttack;
}