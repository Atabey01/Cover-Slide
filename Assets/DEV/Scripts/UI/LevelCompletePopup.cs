using System.Collections.Generic;
using System.Linq;
using AssetKits.ParticleImage;
using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using DEV.Scripts.Managers;
using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    public class LevelCompletePopUp : BasePopUp
    {
        // [SerializeField] private NewFeature newFeature;
        [SerializeField] private Button NextLevelButton;
        [SerializeField] private GameObject NextLevelButtonParent;

        [SerializeField] private TextMeshProUGUI CoinText;
        [SerializeField] private TextMeshProUGUI LevelText;
        [SerializeField] private TextAnimator_TMP LevelCompleteText;
        [SerializeField] private List<ParticleImage> confettiParticles;
        [SerializeField] private GameObject popup;


        private int _currentCoinDisplay;

        private readonly string[] celebrationMessages = new string[]
        {
            "Excellent!",
            "Wonderful!",
            "Amazing!",
            "Fantastic!",
            "Incredible!",
            "Spectacular!",
            "Brilliant!",
            "Outstanding!",
            "Magnificent!",
            "Superb!",
            "Perfect!",
            "Awesome!"
        };

        private readonly string[] hardLevelMessages = new string[]
        {
            "Legendary Performance!",
            "You Conquered the Challenge!",
            "Master of Difficulty!",
            "Epic Victory!",
            "Unbeatable!",
            "Champion!"
        };

        public override void Initialize(GameConfig gameConfig)
        {
            base.Initialize(gameConfig);


            NextLevelButton.onClick.RemoveAllListeners();
            NextLevelButton.onClick.AddListener(Close);

            // newFeature.Initialize(gameConfig);


            // Initialize coin display with current amount
            // _currentCoinDisplay = gameConfig.PlayerConfig.Coin.Value;

            // CoinText.text = _currentCoinDisplay.ToString();
            // WinCoinText.text = Config.GameEconData.levelCompleteCoin.ToString();
        }


        public override void Show(float delay = 0)
        {
            _isActivated = true;
            gameObject.SetActive(true);

            LevelText.text = $"Level {Config.PlayerConfig.LevelId.Value + 1}";
            var randomMessage = GetRandomCelebrationMessage();
            LevelCompleteText.SetText("<wave>" + randomMessage + "<wave>", true);


            popup.transform.localPosition = new Vector3(0, 0, 0);
            LevelCompleteText.transform.localPosition = new Vector3(0, 0, 0);
            NextLevelButtonParent.transform.localScale = Vector3.one;
            NextLevelButtonParent.SetActive(false);


            var textAppearSequence = DOTween.Sequence();
            var confettiSequence = DOTween.Sequence();

            foreach (var particle in confettiParticles)
            {
                particle.gameObject.SetActive(false);
            }

            foreach (var particle in confettiParticles)
            {
                confettiSequence.AppendCallback(() =>
                {
                    particle.gameObject.SetActive(true);
                    particle.Play();
                });
                confettiSequence.AppendInterval(0.5f);
            }
            confettiSequence.SetId("LevelCompleteConfettiSequence" + GetInstanceID());

            int totalChars = LevelCompleteText.CharactersCount;

            textAppearSequence.AppendCallback(() =>
            {
                popup.transform.localPosition = new Vector3(0, -1450f, 0);

                LevelCompleteText.firstVisibleCharacter = 0;
                LevelCompleteText.maxVisibleCharacters = 0;

                for (int i = 0; i < totalChars; i++)
                {
                    LevelCompleteText.SetVisibilityChar(i, true);
                }
            });

            textAppearSequence.Append(DOTween.To(
                () => LevelCompleteText.maxVisibleCharacters,
                x => LevelCompleteText.maxVisibleCharacters = x,
                totalChars,
                1f
            ).SetEase(Ease.OutQuad));

            textAppearSequence.AppendInterval(.75f);
            textAppearSequence.AppendCallback(() =>
            {
                NextLevelButtonParent.transform.localScale = Vector3.zero;
                NextLevelButtonParent.SetActive(true);
            });
            textAppearSequence.Append(LevelCompleteText.transform.DOLocalMoveY(530f, 0.5f).SetEase(Ease.OutQuad));
            textAppearSequence.Join(popup.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutQuad));
            textAppearSequence.AppendInterval(.75f);
            textAppearSequence.Append(NextLevelButtonParent.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack));
            textAppearSequence.SetId("LevelCompleteTextSequence" + GetInstanceID());


            // if (newFeature.HasNewFeature())
            // {
            //     newFeature.Show();
            //     GoldPanel.SetActive(false);
            // }
            // else
            // {
            //     newFeature.gameObject.SetActive(false);
            //     GoldPanel.SetActive(true);
            // }
        }

        private string GetRandomCelebrationMessage()
        {
            var levelData = Config.GamePlayConfig.LevelConfig.GetLevel();
            if (levelData && levelData.LevelDifficultyType == LevelDifficultyType.Hard)
            {
                return hardLevelMessages[Random.Range(0, hardLevelMessages.Length)];
            }

            return celebrationMessages[Random.Range(0, celebrationMessages.Length)];
        }

        public override void Close()
        {
            CloseComplete();
        }


        private void CloseComplete()
        {
            gameObject.SetActive(false);
            StateManager.SetPopupState(PopupState.None);
            StateManager.SetGameState(GameState.Start);
            
            DOTween.Kill("LevelCompleteTextSequence" + GetInstanceID());
            DOTween.Kill("LevelCompleteConfettiSequence" + GetInstanceID());
        }
    }
}