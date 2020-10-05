﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleActionSelection
{
    public BattleResolutionStep BattleResolution;

    public List<BattleEntity> PlayerControlledEntity_WaitingForInput;
    public List<BattleEntity> PlayerControlledEntity_ExecutingAction;
    public BattleEntity CurrentlySelectedEntity;

    public static BattleActionSelection alloc()
    {
        BattleActionSelection l_battleActionSelection = new BattleActionSelection();
        l_battleActionSelection.BattleResolution = Battle_Singletons._battleResolutionStep;
        l_battleActionSelection.PlayerControlledEntity_WaitingForInput = new List<BattleEntity>();
        l_battleActionSelection.PlayerControlledEntity_ExecutingAction = new List<BattleEntity>();
        return l_battleActionSelection;
    }

    public void update(float d)
    {
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
                this.CurrentlySelectedEntity = this.PlayerControlledEntity_WaitingForInput[l_pickedEntit_index];
            }
        }
        //TODO -> If the currently selected entity is dead, we call switch_selection. The check can be donner in the caller function because we already have
        //an event when an entity is dead (search Out_Death_Events).

    }

    // (this.CurrentlySelectedEntity != null) condition is verified before
    public void pushAction_forCurrentSelectedEntity(BQE_Attack_UserDefined p_attackEvent)
    {
        //Optional, as it must be provided, but just to be safe
        p_attackEvent.Source = this.CurrentlySelectedEntity;

        this.BattleResolution.push_attack_event(this.CurrentlySelectedEntity, p_attackEvent);
        this.PlayerControlledEntity_ExecutingAction.Add(this.CurrentlySelectedEntity);
        this.PlayerControlledEntity_WaitingForInput.Remove(this.CurrentlySelectedEntity);

        this.CurrentlySelectedEntity = null;
    }

    // (this.CurrentlySelectedEntity != null) condition is verified before
    public void switch_selection()
    {
        int l_index = this.PlayerControlledEntity_WaitingForInput.IndexOf(this.CurrentlySelectedEntity) + 1;
        if(l_index == this.PlayerControlledEntity_WaitingForInput.Count)
        {
            l_index = 0;
        }
        this.CurrentlySelectedEntity = this.PlayerControlledEntity_WaitingForInput[l_index];

        Debug.Log(l_index);
    }
}
