using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PLAYER_2_conf", menuName = "Entity/PLAYER_2", order = 1)]
public class PLAYER_2_conf : BattleEntityConfiguration_Abstract
{
    public Anim_BattleAttack_Default_Conf Animation_DefaultAttack;
    public AttackDefinition_SO DefaultAttack;
}