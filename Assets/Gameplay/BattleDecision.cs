using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class BattleDecision
{
    public static class Utils
    {
        public static BattleEntity find_battleEntity_ofTeam_random(BattleResolutionStep p_battle, BattleEntity_Team p_team)
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

}
