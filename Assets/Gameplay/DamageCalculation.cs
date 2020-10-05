
using System;
using UnityEngine;

public struct BaseDamageStep
{
    public BattleEntity Source;
    public BattleEntity Target;
    public AttackDefinition BaseAttack;

    public static BaseDamageStep build(BattleEntity p_source, BattleEntity p_target, in AttackDefinition p_baseAttack)
    {
        return new BaseDamageStep() { Source = p_source, Target = p_target, BaseAttack = p_baseAttack };
    }
}

[Serializable]
public struct BattleEntity_BaseStats
{
    public int STR;
    public int DEF;
}

public static class DamageCalculation_Algorithm
{
    public static int calculate(BaseDamageStep p_damageStep)
    {
        float l_mitigation = p_damageStep.Source.Stats.STR / (2.0f * p_damageStep.Target.Stats.DEF);
        return Mathf.CeilToInt(p_damageStep.BaseAttack.BaseDamage * l_mitigation);
    }
}