# StateManager Kullanım Kılavuzu

## 🎯 Basit ve Temiz State Yönetimi

StateManager, oyun state'lerini merkezi olarak yönetmek için basit bir static class'tır.

---

## 📋 Kurulum

### 1. ScriptableObject Referansları Oluştur

Unity Editor'da:
1. `Assets/DEV/Data/` klasörüne git (yoksa oluştur)
2. Sağ tık → `Create → DEV → Data → GameStateReference`
3. Sağ tık → `Create → DEV → Data → PopupStateReference`
4. Bu asset'leri GameManager'a assign et

### 2. GameManager'da Initialize Et

```csharp
using DEV.Scripts.Managers;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("State Management")]
    [SerializeField] private GameStateReference gameStateRef;
    [SerializeField] private PopupStateReference popupStateRef;

    private void Initialize()
    {
        // StateManager'ı initialize et
        StateManager.Initialize(gameStateRef, popupStateRef);
        
        // Diğer initialization'lar...
    }

    private void Dispose()
    {
        // StateManager'ı dispose et
        StateManager.Dispose();
        
        // Diğer cleanup'lar...
    }
}
```

---

## 🚀 Kullanım

### State Değiştirme

```csharp
// Herhangi bir yerden
StateManager.SetGameState(GameState.Play);
StateManager.SetPopupState(PopupState.SettingPopUp);
```

### State Okuma

```csharp
// Mevcut state'i kontrol et
if (StateManager.GameState == GameState.Play)
{
    // Oyun oynanıyor
}
```

### Event Subscription (Dinleme)

```csharp
public class UIManager : MonoBehaviour
{
    private void Start()
    {
        // State değişikliklerini dinle
        StateManager.OnGameStateChanged += OnGameStateChanged;
        StateManager.OnPopupStateChanged += OnPopupStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        Debug.Log($"GameState changed to: {newState}");
        // UI'ı güncelle
        UpdateUI(newState);
    }

    private void OnPopupStateChanged(PopupState newState)
    {
        Debug.Log($"PopupState changed to: {newState}");
        // Popup'ları yönet
    }

    private void OnDestroy()
    {
        // IMPORTANT: Her zaman unsubscribe et
        StateManager.OnGameStateChanged -= OnGameStateChanged;
        StateManager.OnPopupStateChanged -= OnPopupStateChanged;
    }
}
```

---

## 📝 Örnek Senaryolar

### Senaryo 1: Button Click Handler

```csharp
public class MenuPanel : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        StateManager.SetGameState(GameState.Start);
    }

    public void OnSettingsButtonClicked()
    {
        StateManager.SetPopupState(PopupState.SettingPopUp);
    }
}
```

### Senaryo 2: LevelManager State Yönetimi

```csharp
public class LevelManager
{
    public void Initialize()
    {
        StateManager.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Start:
                StartNewLevel();
                break;
            case GameState.Play:
                // Oyun başladı
                break;
            case GameState.End:
                // Oyun bitti
                break;
        }
    }

    private void StartNewLevel()
    {
        StateManager.SetGameState(GameState.Play);
    }

    public void Dispose()
    {
        StateManager.OnGameStateChanged -= OnGameStateChanged;
    }
}
```

### Senaryo 3: State Reset

```csharp
public void ResetGame()
{
    StateManager.ResetAll();
    // veya
    StateManager.ResetGameState();
    StateManager.ResetPopupState();
}
```

---

## ⚠️ Önemli Notlar

1. **Initialize**: Mutlaka GameManager'da `Initialize()` içinde çağır
2. **Dispose**: Mutlaka GameManager'da `Dispose()` içinde çağır
3. **Event Unsubscription**: Her zaman `OnDestroy()` veya `Dispose()` içinde unsubscribe et
4. **Null Check**: ScriptableObject referansları null olmamalı

---

## 🔄 Çalışma Mantığı

```
StateManager.SetGameState(GameState.Play)
    ↓
_gameStateRef.Value = Play (ScriptableObject'e yaz)
    ↓
ScriptableObject.UpdateEvent tetiklenir
    ↓
OnGameStateRefChanged() çağrılır
    ↓
OnGameStateChanged?.Invoke(Play) (Event tetiklenir)
    ↓
Tüm subscribe olan listener'lar çağrılır
```

---

## ✅ Avantajlar

- ✅ **Basit**: Tek dosya, direkt kullanım
- ✅ **Kontrollü**: Initialize/Dispose ile lifecycle yönetimi
- ✅ **Event-Based**: Decoupled, manager'lar birbirini bilmiyor
- ✅ **Unity Native**: ScriptableObject kullanımı (Editor'da görülebilir)
- ✅ **Kolay Kullanım**: Her yerden `StateManager.SetGameState()` ile erişim

