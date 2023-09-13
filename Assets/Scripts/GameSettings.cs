using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CodingChallenge/GameSettings")]
public class GameSettings : ScriptableObject
{
    [SerializeField] public SceneReference DefaultScene;
}
