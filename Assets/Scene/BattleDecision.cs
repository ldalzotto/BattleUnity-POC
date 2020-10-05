
public static class BattleDecision_Specific
{
    private static void decide_nextAction_default(Battle p_battle, BattleEntity p_actingEntity, AttackDefinition p_attack)
    {
        BattleEntity l_targettedEntity = null;
        switch (p_actingEntity.Team)
        {
            case BattleEntity_Team.PLAYER:
                {
                    l_targettedEntity = BattleDecision.Utils.find_battleEntity_ofTeam_random(p_battle, BattleEntity_Team.FOE);
                }
                break;
            case BattleEntity_Team.FOE:
                {
                    l_targettedEntity = BattleDecision.Utils.find_battleEntity_ofTeam_random(p_battle, BattleEntity_Team.PLAYER);
                }
                break;
        }

        if (l_targettedEntity != null)
        {
            BQE_Attack_UserDefined l_attackEvent = new BQE_Attack_UserDefined { Attack = p_attack, Source = p_actingEntity, Target = l_targettedEntity };
            Battle_Singletons._battleResolutionStep.push_attack_event(p_actingEntity, l_attackEvent);
        }
    }

    public static class Interface
    {
        public static void decide_nextAction(Battle p_battle, BattleEntity p_actingEntity)
        {
            switch (p_actingEntity.Type)
            {
                case BattleEntity_Type.DEFAULT:
                    decide_nextAction_default(p_battle, p_actingEntity, AttackDefinition.build(Attack_Type.DEFAULT, 2));
                    break;
                case BattleEntity_Type.SOLIDER_MACHINEGUN_0:
                    decide_nextAction_default(p_battle, p_actingEntity, AttackDefinition.build(Attack_Type.DEFAULT, 1));
                    break;
                default:
                    //TODO -> Introducing an attack of type NONE that does nothing but reset the ATB value of the Entity.
                    //TODO -> This is just for testing purpose
                    break;
            }
        }

    }
}