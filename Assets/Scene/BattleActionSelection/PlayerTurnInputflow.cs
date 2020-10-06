
using UnityEngine;

public enum PlayerTurnInputflowState
{
    NOTHING = 0,
    IN_ACTIONSELECTION_MENU = 1,
    TARGETTING_ENTITY = 2
}

public struct PlayerTurnInputflow
{
    public PlayerTurnInputflowState State;

    public static PlayerTurnInputflow build()
    {
        return new PlayerTurnInputflow() { State = 0 };
    }

    public void update(float d)
    {
        if (Battle_Singletons._battleActionSelection.CurrentlySelectedEntity != null)
        {
            if (!SceneGlobalObjects.BattleActionSelectionUI.isEnabled)
            {
                SceneGlobalObjects.BattleActionSelectionUI.enable();
                this.set_state(PlayerTurnInputflowState.IN_ACTIONSELECTION_MENU);
            }

            switch (this.State)
            {
                case PlayerTurnInputflowState.NOTHING:
                    break;
                case PlayerTurnInputflowState.IN_ACTIONSELECTION_MENU:
                    {
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            this.set_state(PlayerTurnInputflowState.TARGETTING_ENTITY);
                        }
                        else if (Input.GetKeyDown(KeyCode.LeftControl))
                        {
                            Battle_Singletons._battleActionSelection.switch_selection();
                        }
                    }
                    break;
                case PlayerTurnInputflowState.TARGETTING_ENTITY:
                    {
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            BattleEntity l_targettedEntity = BattleDecision.Utils.find_battleEntity_ofTeam_random(Battle_Singletons._battleResolutionStep._battle, BattleEntity_Team.FOE);
                            if (l_targettedEntity != null)
                            {
                                BQE_Attack_UserDefined l_attackEvent = new BQE_Attack_UserDefined { Attack = AttackDefinition.build(Attack_Type.DEFAULT, 2), Source = Battle_Singletons._battleActionSelection.CurrentlySelectedEntity, Target = l_targettedEntity };
                                Battle_Singletons._battleActionSelection.pushAction_forCurrentSelectedEntity(l_attackEvent);
                            }

                            this.state_switchToActionSelectionMenu();
                        }
                        else if(Input.GetKeyDown(KeyCode.LeftControl))
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

        }
        else
        {
            if (SceneGlobalObjects.BattleActionSelectionUI.isEnabled)
            {
                this.set_state(PlayerTurnInputflowState.NOTHING);
                SceneGlobalObjects.BattleActionSelectionUI.disable();
            }
        }

    }

    private void state_switchToActionSelectionMenu()
    {
        if (SceneGlobalObjects.BattleActionSelectionUI.isEnabled)
        {
            this.set_state(PlayerTurnInputflowState.IN_ACTIONSELECTION_MENU);
        }
        else
        {
            this.set_state(PlayerTurnInputflowState.NOTHING);
        }
    }

    private void set_state(PlayerTurnInputflowState p_newState)
    {
        if (this.State != p_newState)
        {
            switch (this.State)
            {
                case PlayerTurnInputflowState.NOTHING:
                case PlayerTurnInputflowState.IN_ACTIONSELECTION_MENU:
                    {
                        switch (p_newState)
                        {
                            case PlayerTurnInputflowState.TARGETTING_ENTITY:
                                {
                                    Battle_Singletons._battleTargetSelection.enable();
                                }
                                break;
                        }
                    }
                    break;

                case PlayerTurnInputflowState.TARGETTING_ENTITY:
                    {
                        Battle_Singletons._battleTargetSelection.disable();
                    }
                    break;
            }
        }

        this.State = p_newState;
    }
}
