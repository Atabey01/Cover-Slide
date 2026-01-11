using AYellowpaper.SerializedCollections;
using DEV.Scripts.Config;
using DEV.Scripts.Enums;
using DEV.Scripts.Managers;
using UnityEngine;

namespace DEV.Scripts.UI
{
    public class PanelManager : MonoBehaviour
    {
        [Header("Panels")] [SerializeField] private SerializedDictionary<Enums.GameState, BasePanel> Panels;

        public void Initialize(GameConfig gameConfig)
        {
            gameObject.SetActive(true);
            
            foreach (var panel in Panels) panel.Value.Initialize(gameConfig);

            StateManager.OnGameStateChanged += OnGameStateChanged;
        }

        private void OnGameStateChanged(Enums.GameState value)
        {
            if (!Panels.TryGetValue(value, out var panel)) return;
            if (panel == null) return;

            panel.Show();

            foreach (var p in Panels)
            {
                if (p.Value != panel)
                {
                    p.Value.Close();
                }
            }
        }

        public void Dispose()
        {
            foreach (var panel in Panels) panel.Value.Dispose();
            StateManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}