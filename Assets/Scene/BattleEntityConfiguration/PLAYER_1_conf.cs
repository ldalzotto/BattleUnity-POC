using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PLAYER_1_conf", menuName = "Entity/PLAYER_1", order = 1)]
public class PLAYER_1_conf : BattleEntityConfiguration_Abstract
{
    public Anim_BattleAttack_Default_Conf Animation_DefaultAttack;
    public AttackDefinition_SO DefaultAttack;
}