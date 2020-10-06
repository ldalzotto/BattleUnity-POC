
using UnityEngine;

public enum BattleSelectionFlowState
{
    NOTHING = 0,

    /// <summary>
    /// When a battle entity is ready to perform an action, and the player is not selecting anything.
    /// Basically, the menu is unselected
    /// </summary>
    IN_ACTIONSELECTION_MENU = 1,

    /// <summary>
    /// When the player is selecting a battle entity to target 
    /// </summary>
    TARGETTING_ENTITY = 2
}

/// <summary>
/// System responsible fo the execution flow of player input in combat.
/// Updates dynamic UI objects related to battle entity selection.
/// </summary>
public struct BattlePlayerSelectionFlow
{
    public BattleSelectionFlowState State;

    public static BattlePlayerSelectionFlow build()
    {
        return new BattlePlayerSelectionFlow() { State = 0 };
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
                            BQE_Attack l_attackEvent = new BQE_Attack
                            {
                                Attack = BattleAttackConfiguration_Algorithm.find_defaultAttackDefinition(BattleEntityComponent_Container.ComponentsByHandle[Battle_Singletons._battleActionSelection.CurrentlySelectedEntity]),
                                Source = Battle_Singletons._battleActionSelection.CurrentlySelectedEntity,
                                Target = Battle_Singletons._battleResolutionStep.BattleEntities[Battle_Singletons._battleTargetSelection.CurrentlySelectedEntity_BattleIndex]
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
            // We are exiting the current state
            switch (this.State)
            {
                case BattleSelectionFlowState.NOTHING:
                case BattleSelectionFlowState.IN_ACTIONSELECTION_MENU:
                    {
                        switch (p_newState)
                        {
                            case BattleSelectionFlowState.TARGETTING_ENTITY:
                                {
                                    //TODO -> target selection filter won't always be BattleEntity_Team.FOE
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
        switch(this.State)
        {
            case BattleSelectionFlowState.TARGETTING_ENTITY:
                {
                    if (Battle_Singletons._battleActionSelection.CurrentlySelectedEntity == p_deadBattleEntityComponent.BattleEntityHandle)
                    {
                        this.set_state(BattleSelectionFlowState.IN_ACTIONSELECTION_MENU);
                    }
                }
                break;
        }
    }
}
