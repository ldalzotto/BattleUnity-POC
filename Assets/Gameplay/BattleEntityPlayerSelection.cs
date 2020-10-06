using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that holds the currently played BattleEntity by the player.
/// </summary>
public class BattleEntityPlayerSelection
{
    public BattleResolutionStep BattleResolution;

    // Do not set this variable, instead, call set_CurrentlySelectedEntity
    public BattleEntity CurrentlySelectedEntity;
    public bool CurrentlySelectedEntity_HasChanged;

    // List of BattleEntity that are available for player control.
    private List<BattleEntity> PlayerControlledEntity_WaitingForInput;
    // List of BattleEntity for which the player have made an input action, but the action is currently exectued by the BattleResolutionStep.
    private List<BattleEntity> PlayerControlledEntity_ExecutingAction;

    public static BattleEntityPlayerSelection alloc(BattleResolutionStep p_battleResolution)
    {
        BattleEntityPlayerSelection l_battleActionSelection = new BattleEntityPlayerSelection();
        l_battleActionSelection.BattleResolution = p_battleResolution;
        l_battleActionSelection.PlayerControlledEntity_WaitingForInput = new List<BattleEntity>();
        l_battleActionSelection.PlayerControlledEntity_ExecutingAction = new List<BattleEntity>();
        return l_battleActionSelection;
    }

    public void update(float d)
    {
        this.CurrentlySelectedEntity_HasChanged = false;

        // We check is last frame completed battle events matches any of entity in PlayerControlledEntity_ExecutingAction.
        if (this.PlayerControlledEntity_ExecutingAction.Count > 0)
        {
            if (this.BattleResolution.Out_CompletedBattlequeue_Events.Count > 0)
            {
                for (int i = 0; i < this.BattleResolution.Out_CompletedBattlequeue_Events.Count; i++)
                {
                    BattleQueueEvent l_event = this.BattleResolution.Out_CompletedBattlequeue_Events[i];

                    for (int j = 0; j < this.PlayerControlledEntity_ExecutingAction.Count; j++)
                    {
                        BattleEntity l_controllableEntity_currentlyExecutingAction = this.PlayerControlledEntity_ExecutingAction[j];

                        // We remove the entity from PlayerControlledEntity_ExecutingAction is that's the case
                        if (l_event.ActiveEntityHandle == l_controllableEntity_currentlyExecutingAction)
                        {
                            this.PlayerControlledEntity_ExecutingAction.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }

        // We check if any battle entiy matches the condition for being able to be controlled by the player.
        for (int i = 0; i < this.BattleResolution.BattleEntities.Count; i++)
        {
            BattleEntity l_entity = this.BattleResolution.BattleEntities[i];
            if (l_entity.IsControlledByPlayer && l_entity.ATB_Value >= 1.0f)
            {
                if (!this.PlayerControlledEntity_WaitingForInput.Contains(l_entity) && !this.PlayerControlledEntity_ExecutingAction.Contains(l_entity))
                {
                    this.PlayerControlledEntity_WaitingForInput.Add(l_entity);
                }
            }
        }

        // We perform a default selection if there is no currently selected entity
        if (CurrentlySelectedEntity == null)
        {
            if (this.PlayerControlledEntity_WaitingForInput.Count > 0)
            {
                int l_pickedEntit_index = 0;
                this.set_CurrentlySelectedEntity(this.PlayerControlledEntity_WaitingForInput[l_pickedEntit_index]);
            }
        }
    }

    // (this.CurrentlySelectedEntity != null) condition is verified before
    public void pushAction_forCurrentSelectedEntity(BQE_Attack p_attackEvent)
    {
        //Optional, as it must be provided, but just to be safe
        p_attackEvent.Source = this.CurrentlySelectedEntity;

        this.BattleResolution.push_attack_event(this.CurrentlySelectedEntity, p_attackEvent);
        this.PlayerControlledEntity_ExecutingAction.Add(this.CurrentlySelectedEntity);
        this.PlayerControlledEntity_WaitingForInput.Remove(this.CurrentlySelectedEntity);

        this.set_CurrentlySelectedEntity(null);
    }

    // (this.CurrentlySelectedEntity != null) condition is verified before
    public void switch_selection()
    {
        int l_index = this.PlayerControlledEntity_WaitingForInput.IndexOf(this.CurrentlySelectedEntity) + 1;
        if (l_index == this.PlayerControlledEntity_WaitingForInput.Count)
        {
            l_index = 0;
        }
        this.set_CurrentlySelectedEntity(this.PlayerControlledEntity_WaitingForInput[l_index]);

        Debug.Log(l_index);
    }

    /// <summary>
    /// If the dead battle entity is the currently selected, we clear selection and remove it from PlayerControlledEntity_WaitingForInput. 
    /// /!\ This method must be called only when death events are processed.
    /// </summary>
    /// <param name="p_deadbattleEntity"></param>
    public void on_battleEntityDeath(BattleEntity p_deadbattleEntity)
    {
        if (this.CurrentlySelectedEntity == p_deadbattleEntity)
        {
            this.PlayerControlledEntity_WaitingForInput.Remove(p_deadbattleEntity);
            this.PlayerControlledEntity_ExecutingAction.Remove(p_deadbattleEntity);
            this.set_CurrentlySelectedEntity(null);
        }
    }

    private void set_CurrentlySelectedEntity(BattleEntity p_currentlySelectedEntity)
    {
        if (this.CurrentlySelectedEntity != p_currentlySelectedEntity)
        { this.CurrentlySelectedEntity_HasChanged = true; }
        this.CurrentlySelectedEntity = p_currentlySelectedEntity;
    }
}

/// <summary>
/// System that holds the currently target BattleEntity by the player.
/// </summary>
public class BattleEntityTargetSelection
{
    public BattleResolutionStep BattleResolutionStep;
    private bool isEnabled;
    public bool CurrentlySelectedEntity_HasChanged;
    public int CurrentlySelectedEntity_BattleIndex;

    // Filter used against all battle entity to check if they are elligible for targetting
    private BattleTargetSelection_FilterCriteria FilterCriteria;

    public static readonly int CurrentlySelectedEntity_BattleIndex_None = -1;

    public static BattleEntityTargetSelection alloc(BattleResolutionStep p_battleResolutionStep)
    {
        BattleEntityTargetSelection l_targetSelection = new BattleEntityTargetSelection() { BattleResolutionStep = p_battleResolutionStep };
        l_targetSelection.CurrentlySelectedEntity_BattleIndex = CurrentlySelectedEntity_BattleIndex_None;
        return l_targetSelection;
    }

    public void enable()
    {
        this.CurrentlySelectedEntity_HasChanged = false;
        this.isEnabled = true;
        this.select_firstAvailableEntity();
    }

    public void enable(BattleTargetSelection_FilterCriteria p_filterCriteria)
    {
        this.FilterCriteria = p_filterCriteria;
        this.enable();
    }

    public void disable()
    {
        this.set_CurrentlySelectedEntity(CurrentlySelectedEntity_BattleIndex_None);
        this.isEnabled = false;
    }

    public void update(float d)
    {
        this.CurrentlySelectedEntity_HasChanged = false;
        if (this.isEnabled)
        {
            // If enabled and there is no selected entity, we pick the first available one
            if (this.CurrentlySelectedEntity_BattleIndex == CurrentlySelectedEntity_BattleIndex_None)
            {
                this.select_firstAvailableEntity();
            }
        }
    }

    public void switch_target()
    {
        if (this.CurrentlySelectedEntity_BattleIndex != CurrentlySelectedEntity_BattleIndex_None)
        {
            int l_newIndex = this.CurrentlySelectedEntity_BattleIndex + 1;
            if (l_newIndex == this.BattleResolutionStep.BattleEntities.Count)
            {
                l_newIndex = 0;
            }

            while (l_newIndex != this.CurrentlySelectedEntity_BattleIndex)
            {
                if (this.FilterCriteria.isBattleEntity_compliant(this.BattleResolutionStep.BattleEntities[l_newIndex]))
                {
                    set_CurrentlySelectedEntity(l_newIndex);
                    return;
                }


                l_newIndex += 1;
                if (l_newIndex == this.BattleResolutionStep.BattleEntities.Count)
                {
                    l_newIndex = 0;
                }
            }
        }
    }

    public void on_battleEntityDeath(int p_deadbattleEntity_index)
    {
        if (this.CurrentlySelectedEntity_BattleIndex == p_deadbattleEntity_index)
        {
            this.set_CurrentlySelectedEntity(CurrentlySelectedEntity_BattleIndex_None);
            this.select_firstAvailableEntity();
        }
    }

    private void select_firstAvailableEntity()
    {
        for (int i = 0; i < this.BattleResolutionStep.BattleEntities.Count; i++)
        {
            if (this.FilterCriteria.isBattleEntity_compliant(this.BattleResolutionStep.BattleEntities[i]))
            {
                set_CurrentlySelectedEntity(i);
                return;
            }
        }

        set_CurrentlySelectedEntity(CurrentlySelectedEntity_BattleIndex_None);
    }


    private void set_CurrentlySelectedEntity(int l_index)
    {
        if (this.CurrentlySelectedEntity_BattleIndex != l_index)
        { this.CurrentlySelectedEntity_HasChanged = true; }
        this.CurrentlySelectedEntity_BattleIndex = l_index;
    }
}

public struct BattleTargetSelection_FilterCriteria
{
    public bool TeamFilterEnabled;
    public BattleEntity_Team Team;

    public static BattleTargetSelection_FilterCriteria build(BattleEntity_Team p_filteredTeam)
    {
        return new BattleTargetSelection_FilterCriteria()
        {
            TeamFilterEnabled = true,
            Team = p_filteredTeam
        };
    }

    public bool isBattleEntity_compliant(BattleEntity p_battleEntity)
    {
        if (this.TeamFilterEnabled && p_battleEntity.Team != this.Team)
        {
            return false;
        }
        return true;
    }
}