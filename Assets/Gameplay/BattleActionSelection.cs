using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleActionSelection
{
    public BattleResolutionStep BattleResolution;

    public List<BattleEntity> PlayerControlledEntity_WaitingForInput;
    public List<BattleEntity> PlayerControlledEntity_ExecutingAction;
    public BattleEntity CurrentlySelectedEntity;
    public bool CurrentlySelectedEntity_HasChanged;

    public static BattleActionSelection alloc(BattleResolutionStep p_battleResolution)
    {
        BattleActionSelection l_battleActionSelection = new BattleActionSelection();
        l_battleActionSelection.BattleResolution = p_battleResolution;
        l_battleActionSelection.PlayerControlledEntity_WaitingForInput = new List<BattleEntity>();
        l_battleActionSelection.PlayerControlledEntity_ExecutingAction = new List<BattleEntity>();
        return l_battleActionSelection;
    }

    public void update(float d)
    {
        this.CurrentlySelectedEntity_HasChanged = false;

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

                        if (l_event.ActiveEntityHandle == l_controllableEntity_currentlyExecutingAction)
                        {
                            this.PlayerControlledEntity_ExecutingAction.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < this.BattleResolution._battle.BattleEntities.Count; i++)
        {
            BattleEntity l_entity = this.BattleResolution._battle.BattleEntities[i];
            if (l_entity.IsControlledByPlayer && l_entity.ATB_Value >= 1.0f)
            {
                if (!this.PlayerControlledEntity_WaitingForInput.Contains(l_entity) && !this.PlayerControlledEntity_ExecutingAction.Contains(l_entity))
                {
                    this.PlayerControlledEntity_WaitingForInput.Add(l_entity);
                }
            }
        }

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
    public void pushAction_forCurrentSelectedEntity(BQE_Attack_UserDefined p_attackEvent)
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

    public void on_battleEntityDeath(BattleEntity p_deadbattleEntity)
    {
        if (this.CurrentlySelectedEntity == p_deadbattleEntity)
        {
            this.PlayerControlledEntity_WaitingForInput.Remove(p_deadbattleEntity);
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

public class BattleTargetSelection
{
    public BattleResolutionStep BattleResolutionStep;
    private bool isEnabled;
    private bool CurrentlySelectedEntity_HasChanged;
    public int CurrentlySelectedEntity_BattleIndex;

    private static readonly int CurrentlySelectedEntity_BattleIndex_None = -1;

    public static BattleTargetSelection alloc(BattleResolutionStep p_battleResolutionStep)
    {
        BattleTargetSelection l_targetSelection = new BattleTargetSelection() { BattleResolutionStep = p_battleResolutionStep };
        l_targetSelection.CurrentlySelectedEntity_BattleIndex = CurrentlySelectedEntity_BattleIndex_None;
        return l_targetSelection;
    }

    public void enable()
    {
        this.CurrentlySelectedEntity_HasChanged = false;
        this.isEnabled = true;
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
            if (l_newIndex == this.BattleResolutionStep._battle.BattleEntities.Count)
            {
                l_newIndex = 0;
            }
            set_CurrentlySelectedEntity(l_newIndex);
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
        if(this.BattleResolutionStep._battle.BattleEntities.Count == 0)
        {
            set_CurrentlySelectedEntity(CurrentlySelectedEntity_BattleIndex_None);
        }
        else
        {
            for (int i = 0; i < this.BattleResolutionStep._battle.BattleEntities.Count; i++)
            {
                BattleEntity l_battleEntity = this.BattleResolutionStep._battle.BattleEntities[i];
                set_CurrentlySelectedEntity(i);
            }
        }
    }

    private void set_CurrentlySelectedEntity(int l_index)
    {
        if (this.CurrentlySelectedEntity_BattleIndex != l_index)
        { this.CurrentlySelectedEntity_HasChanged = true; }
        this.CurrentlySelectedEntity_BattleIndex = l_index;
    }
}