using CandyCoded;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Data
{
    /// <summary>
    /// GameState Reference ScriptableObject.
    /// Used by StateManager for game state management.
    /// </summary>
    [CreateAssetMenu(fileName = "GameStateReference", menuName = "References/Data/GameStateReference", order = 0)]
    public class GameStateReference : CustomGenericScriptableObject<GameState>
    {
    }
}

