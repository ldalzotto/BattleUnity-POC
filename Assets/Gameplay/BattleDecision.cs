using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

static class BattleDecision
{
    static class Utils
    {
        public static BattleEntity find_battleEntity_ofTeam_random(Battle p_battle, BattleEntity_Team p_team)
        {
            using (UnsafeList<int> l_targettableEntities = new UnsafeList<int>(0, Unity.Collections.Allocator.Temp))
            {
                for (int i = 0; i < p_battle.BattleEntities.Count; i++)
                {
                    if (p_battle.BattleEntities[i].Team == p_team)
                    {
                        l_targettableEntities.Add(i);
                    }
                }

                if (l_targettableEntities.Length > 0)
                {
                    return p_battle.BattleEntities[l_targettableEntities[Random.Range(0, l_targettableEntities.Length)]];
                }
            }

            return null;
        }
    }

    static class Algorithm
    {
        /// <summary>
        /// Pick the right BattleQueueEvent and push it to the queue.
        /// </summary>
        /// <param name="p_actingEntity"></param>
        public static void decide_nextAction_default(Battle p_battle, BattleEntity p_actingEntity)
        {
            BattleEntity l_targettedEntity = null;
            switch (p_actingEntity.Team)
            {
                case BattleEntity_Team.PLAYER:
                    {
                        l_targettedEntity = Utils.find_battleEntity_ofTeam_random(p_battle, BattleEntity_Team.FOE);
                    }
                    break;
                case BattleEntity_Team.FOE:
                    {
                        l_targettedEntity = Utils.find_battleEntity_ofTeam_random(p_battle, BattleEntity_Team.PLAYER);
                    }
                    break;
            }

            if (l_targettedEntity != null)
            {
                BQE_Attack_UserDefined l_attackEvent = new BQE_Attack_UserDefined { AttackType = Attack_Type.DEFAULT, Source = p_actingEntity, Target = l_targettedEntity };
                Battle_Singletons._battleResolutionStep.push_attack_event(p_actingEntity, l_attackEvent);
            }
        }
    }

    public static class Interface
    {
        public static void decide_nextAction(Battle p_battle, BattleEntity p_actingEntity)
        {
            switch (p_actingEntity.Type)
            {
                case BattleEntity_Type.DEFAULT:
                case BattleEntity_Type.SOLIDER_MACHINEGUN_0:
                    Algorithm.decide_nextAction_default(p_battle, p_actingEntity);
                    break;
                default:
                    //TODO -> Introducing an attack of type NONE that does nothing but reset the ATB value of the Entity.
                    //TODO -> This is just for testing purpose
                    break;
            }
        }
    }
}
