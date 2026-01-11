namespace DEV.Scripts.Enums
{
    public enum GameState
    {
        Menu = 0,
        Loading = 1,
        Start = 2,
        Play = 3,
        Pause = 4,
        End = 5,
        Restart = 6,
        KeepPlaying = 7,
    }

    public enum PopupState
    {
        None = 0,
        SettingPopUp = 1,
        LevelSuccessPopUp = 2,
        LevelFailPopUp = 3,
        HardLevelPopUp = 4,
        NewFeaturePopUp = 5,
    }

    public enum LevelDifficultyType
    {
        Normal = 0,
        Medium = 1,
        Hard = 2,
    }
    
    public enum ColorType
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Yellow = 3,
        Purple = 4,
        Orange = 5,
        Cyan = 6,
        Pink = 7,
    }

    /// <summary>
    /// SFX (Ses Efektleri) türleri
    /// </summary>
    public enum SFXType
    {
        None = 0,
        ButtonClick = 1,
        LevelComplete = 2,
        LevelFail = 3,
        PopupOpen = 4,
        PopupClose = 5,
        SettingsSliderChange = 6,
        // Buraya ihtiyacınıza göre daha fazla SFX türü ekleyebilirsiniz
    }

    /// <summary>
    /// Music (Müzik) türleri
    /// </summary>
    public enum MusicType
    {
        None = 0,
        MenuMusic = 1,
        GameplayMusic = 2,
        // Buraya ihtiyacınıza göre daha fazla Music türü ekleyebilirsiniz
    }
    
    /// <summary>
    /// Level başarısızlık nedenleri
    /// </summary>
    public enum LevelFailReason
    {
        None = 0,
        TimeOut = 1,
        OutOfMoves = 2,
        // Buraya ihtiyacınıza göre daha fazla fail reason ekleyebilirsiniz
    }
}