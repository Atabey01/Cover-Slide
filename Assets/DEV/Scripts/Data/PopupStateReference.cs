using CandyCoded;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.Data
{
    /// <summary>
    /// PopupState Reference ScriptableObject.
    /// Used by StateManager for popup state management.
    /// </summary>
    [CreateAssetMenu(fileName = "PopupStateReference", menuName = "References/Data/PopupStateReference", order = 0)]
    public class PopupStateReference : CustomGenericScriptableObject<PopupState>
    {
    }
}

