
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

public struct BattleEntity_BaseStats
{
    public int STR;
    public int DEF;
}

public static class DamageCalculation_Algorithm
{
    public static int calculate(BaseDamageStep p_damageStep)
    {
        return p_damageStep.BaseAttack.BaseDamage;
    }
}