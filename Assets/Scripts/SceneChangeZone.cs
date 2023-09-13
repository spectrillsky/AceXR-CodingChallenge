using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChangeZone : PlayerZone
{
    [SerializeField] SceneReference DesiredScene;
    protected override void OnPlayerEnter(PlayerController player)
    {
        base.OnPlayerEnter(player);

        if(DesiredScene == null)
        {
            Debug.LogError($"No Desired Scene ref set!");
            return;
        }

        GameManager.Instance.Server_ChangeScene(DesiredScene.Name);
    }
}
