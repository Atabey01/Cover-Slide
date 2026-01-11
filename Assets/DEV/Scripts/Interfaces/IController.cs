using DEV.Scripts.Data;

namespace DEV.Scripts.Interfaces
{
    public interface IController
    {
        void StartNewLevel(LevelData levelData);
        void LevelDestroy();
        void Dispose();
    }
}