
using UnityEngine;

public enum BattleSelectionFlowState
{
    NOTHING = 0,
    IN_ACTIONSELECTION_MENU = 1,
    TARGETTING_ENTITY = 2
}

public struct BattleSelectionFlow
{
    public BattleSelectionFlowState State;

    public static BattleSelectionFlow build()
    {
        return new BattleSelectionFlow() { State = 0 };
    }

    public void update(float d)
    {
        if (Battle_Singletons._battleActionSelection.CurrentlySelectedEntity != null)
        {
            if (!SceneGlobalObjects.BattleActionSelectionUI.isEnabled)
            {
                SceneGlobalObjects.BattleActionSelectionUI.enable();
                this.set_state(BattleSelectionFlowState.IN_ACTIONSELECTION_MENU);
            }

            switch (this.State)
            {
                case BattleSelectionFlowState.NOTHING:
                    break;
                case BattleSelectionFlowState.IN_ACTIONSELECTION_MENU:
                    {
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            this.set_state(BattleSelectionFlowState.TARGETTING_ENTITY);
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftControl))
                        {
                            Battle_Singletons._battleActionSelection.switch_selection();
                        }
                    }
                    break;
                case BattleSelectionFlowState.TARGETTING_ENTITY:
                    {
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            //BattleDecision.Utils.find_battleEntity_ofTeam_random(Battle_Singletons._battleResolutionStep._battle, BattleEntity_Team.FOE);

                            BQE_Attack_UserDefined l_attackEvent = new BQE_Attack_UserDefined
                            {
                                Attack = AttackDefinition.build(Attack_Type.DEFAULT, 2),
                                Source = Battle_Singletons._battleActionSelection.CurrentlySelectedEntity,
                                Target = Battle_Singletons._battleResolutionStep._battle.BattleEntities[Battle_Singletons._battleTargetSelection.CurrentlySelectedEntity_BattleIndex]
                            };
                            Battle_Singletons._battleActionSelection.pushAction_forCurrentSelectedEntity(l_attackEvent);

                            this.state_switchToActionSelectionMenu();
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftControl))
                        {
                            Battle_Singletons._battleTargetSelection.switch_target();
                        }
                        else if (Input.GetKeyDown(KeyCode.Backspace))
                        {
                            this.state_switchToActionSelectionMenu();
                        }
                    }
                    break;
            }


            SceneGlobalObjects.BattleActionSelectionUI.update(Battle_Singletons._battleActionSelection);
            SceneGlobalObjects.BattleTargetSelectionUI.update(Battle_Singletons._battleTargetSelection);

        }
        else
        {
            if (SceneGlobalObjects.BattleActionSelectionUI.isEnabled)
            {
                this.set_state(BattleSelectionFlowState.NOTHING);
                SceneGlobalObjects.BattleActionSelectionUI.disable();
            }
        }

    }

    private void state_switchToActionSelectionMenu()
    {
        if (SceneGlobalObjects.BattleActionSelectionUI.isEnabled)
        {
            this.set_state(BattleSelectionFlowState.IN_ACTIONSELECTION_MENU);
        }
        else
        {
            this.set_state(BattleSelectionFlowState.NOTHING);
        }
    }

    private void set_state(BattleSelectionFlowState p_newState)
    {
        if (this.State != p_newState)
        {
            switch (this.State)
            {
                case BattleSelectionFlowState.NOTHING:
                case BattleSelectionFlowState.IN_ACTIONSELECTION_MENU:
                    {
                        switch (p_newState)
                        {
                            case BattleSelectionFlowState.TARGETTING_ENTITY:
                                {
                                    Battle_Singletons._battleTargetSelection.enable(BattleTargetSelection_FilterCriteria.build(BattleEntity_Team.FOE));
                                }
                                break;
                        }
                    }
                    break;

                case BattleSelectionFlowState.TARGETTING_ENTITY:
                    {
                        Battle_Singletons._battleTargetSelection.disable();
                    }
                    break;
            }
        }

        this.State = p_newState;
    }

    public void on_battleEntityDeath(BattleEntityComponent p_deadBattleEntityComponent)
    {
        if (Battle_Singletons._battleActionSelection.CurrentlySelectedEntity == p_deadBattleEntityComponent.BattleEntityHandle)
        {
            if (this.State == BattleSelectionFlowState.TARGETTING_ENTITY)
            {
                this.set_state(BattleSelectionFlowState.IN_ACTIONSELECTION_MENU);
            }
        }

    }
}
