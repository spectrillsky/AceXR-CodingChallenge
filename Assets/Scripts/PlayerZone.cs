using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class that can be used for networked zone interactions
/// </summary>
public class PlayerZone : MonoBehaviour
{
    [SerializeField] List<PlayerController> players = new List<PlayerController>();

    protected virtual void OnPlayerEnter(PlayerController player)
    {
        Debug.Log($"[PlayerZone] Player Entered: {player.photonView.Owner.NickName}");
        players.Add(player);
    }

    protected virtual void OnPlayerExit(PlayerController player)
    {
        players.Remove(player);
    }

    #region Triggers
    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        if (players.Contains(player)) return;
        OnPlayerEnter(player);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        if (!players.Contains(player)) return; //Might be an issue with further complexity. Log Error or warning

        OnPlayerExit(player);
    }
    #endregion


}
