using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    [field:SerializeField] public GameObject HandObjectPrefab { get; private set; }
    [field:SerializeField] public bool ColorInteractionsEnabled { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


}
