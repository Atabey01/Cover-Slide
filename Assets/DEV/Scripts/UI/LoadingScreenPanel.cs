using System.Collections;
using DEV.Scripts.Config;
using DEV.Scripts.Enums;
using DEV.Scripts.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    public class LoadingScreenPanel : BasePanel
    {
        [Header("Loading Screen References")] [SerializeField]
        private Slider _slider;

        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private TextMeshProUGUI loadingProgressText;

        [SerializeField] private CanvasGroup canvasGroup;

        private Coroutine _loadingCoroutine;
        private Coroutine _loadingTextCoroutine;
        private bool _isActive;

        // DOTween IDs for tracking and killing tweens
        private const string FADE_TWEEN_ID = "LoadingScreen_Fade";
        private const string PROGRESS_TWEEN_ID = "LoadingScreen_Progress";

        public override void Initialize(GameConfig gameConfig)
        {
            base.Initialize(gameConfig);
            Setup();
        }

        private void Setup()
        {
        }

        public override void Show()
        {
            // Check if loading screen is enabled
            bool isEnabled = _gameConfig?.GamePlayConfig?.LoadingScreenIsEnabled ?? true;
            if (!isEnabled)
            {
                // If disabled, skip loading screen and go directly to Start state
                StateManager.SetGameState(GameState.Start);
                return;
            }

            base.Show();

            if (_isActive) return;

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
            }

            _loadingCoroutine = StartCoroutine(ShowCoroutine());
        }

        public override void Close()
        {
            if (!_isActive) return;

            if (canvasGroup != null)
            {
                _isActive = false;
                StateManager.SetGameState(GameState.Start);

                // Kill existing fade tween if any
                DOTween.Kill(FADE_TWEEN_ID);

                canvasGroup.DOFade(0f, 0.3f)
                    .SetId(FADE_TWEEN_ID)
                    .OnComplete(() => { base.Close(); });
            }
            else
            {
                _isActive = false;
                base.Close();
            }
        }

        private IEnumerator ShowCoroutine()
        {
            _isActive = true;

            // Null check before starting
            if (this == null || gameObject == null) yield break;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            if (_slider != null)
            {
                _slider.value = 0f;
            }

            // Initialize text fields
            if (loadingText != null)
            {
                loadingText.text = "Loading.";
            }

            if (loadingProgressText != null)
            {
                loadingProgressText.text = "0/100";
            }

            // Start loading text animation
            if (_loadingTextCoroutine != null)
            {
                StopCoroutine(_loadingTextCoroutine);
            }

            _loadingTextCoroutine = StartCoroutine(AnimateLoadingText());

            float totalDuration = _gameConfig?.GamePlayConfig?.LoadingScreenProgressDuration ?? 1.5f;

            // Divide total duration into 3 random parts
            float[] durations = DivideIntoThreeParts(totalDuration);
            float[] pauseDurations = { 0.1f, 0.15f, 0.1f }; // Short pauses between stages
            float[] targetProgresses = { 0.35f, 0.70f, 1.0f }; // Three stages: 35%, 70%, 100%

            if (_slider != null && this != null && gameObject != null)
            {
                // Kill any existing progress tweens
                DOTween.Kill(PROGRESS_TWEEN_ID);

                // Stage 1: 0% -> 35%
                _slider.DOValue(targetProgresses[0], durations[0])
                    .SetId(PROGRESS_TWEEN_ID)
                    .SetEase(Ease.OutQuad)
                    .OnUpdate(() =>
                    {
                        if (loadingProgressText != null && _slider != null)
                        {
                            int progress = Mathf.RoundToInt(_slider.value * 100f);
                            loadingProgressText.text = $"{progress}/100";
                        }
                    });
                yield return new WaitForSeconds(durations[0] + pauseDurations[0]);

                // Check if still valid
                if (this == null || gameObject == null || _slider == null) yield break;

                // Stage 2: 35% -> 70%
                _slider.DOValue(targetProgresses[1], durations[1])
                    .SetId(PROGRESS_TWEEN_ID)
                    .SetEase(Ease.OutQuad)
                    .OnUpdate(() =>
                    {
                        if (loadingProgressText != null && _slider != null)
                        {
                            int progress = Mathf.RoundToInt(_slider.value * 100f);
                            loadingProgressText.text = $"{progress}/100";
                        }
                    });
                yield return new WaitForSeconds(durations[1] + pauseDurations[1]);

                // Check if still valid
                if (this == null || gameObject == null || _slider == null) yield break;

                // Stage 3: 70% -> 100%
                _slider.DOValue(targetProgresses[2], durations[2])
                    .SetId(PROGRESS_TWEEN_ID)
                    .SetEase(Ease.OutQuad)
                    .OnUpdate(() =>
                    {
                        if (loadingProgressText != null && _slider != null)
                        {
                            int progress = Mathf.RoundToInt(_slider.value * 100f);
                            loadingProgressText.text = $"{progress}/100";
                        }
                    });
                yield return new WaitForSeconds(durations[2]);
            }
            else
            {
                // If no progress bar, just wait for total duration
                yield return new WaitForSeconds(totalDuration);
            }

            // Final null check before closing
            if (this == null || gameObject == null) yield break;

            // Ensure progress bar is at 100%
            if (_slider != null)
            {
                _slider.value = 1f;
            }

            // Ensure progress text is at 100/100
            if (loadingProgressText != null)
            {
                loadingProgressText.text = "100/100";
            }

            yield return new WaitForSeconds(0.2f);

            // Final check before calling Close
            if (this != null && gameObject != null)
            {
                Close();
            }
        }

        /// <summary>
        /// Animates loading text: Loading. => Loading.. => Loading... (looping)
        /// </summary>
        private IEnumerator AnimateLoadingText()
        {
            if (loadingText == null) yield break;

            int dotCount = 1;
            while (_isActive && this != null && gameObject != null)
            {
                if (loadingText != null)
                {
                    loadingText.text = "Loading" + new string('.', dotCount);
                }

                dotCount++;
                if (dotCount > 3)
                {
                    dotCount = 1;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// Divides total duration into 3 random parts that sum up to total duration.
        /// Each part will be between 20% and 40% of total duration.
        /// </summary>
        private float[] DivideIntoThreeParts(float totalDuration)
        {
            // Generate random percentages that sum to 1.0
            float p1 = Random.Range(0.2f, 0.4f);
            float p2 = Random.Range(0.2f, 0.4f);
            float p3 = 1f - p1 - p2;

            // Ensure p3 is also in valid range, if not, adjust
            if (p3 < 0.2f)
            {
                float excess = 0.2f - p3;
                p1 = Mathf.Max(0.2f, p1 - excess / 2f);
                p2 = Mathf.Max(0.2f, p2 - excess / 2f);
                p3 = 1f - p1 - p2;
            }
            else if (p3 > 0.4f)
            {
                float excess = p3 - 0.4f;
                p1 = Mathf.Min(0.4f, p1 + excess / 2f);
                p2 = Mathf.Min(0.4f, p2 + excess / 2f);
                p3 = 1f - p1 - p2;
            }

            return new float[]
            {
                totalDuration * p1,
                totalDuration * p2,
                totalDuration * p3
            };
        }

        public override void Dispose()
        {
            // Kill all tweens by ID (clean and efficient)
            DOTween.Kill(FADE_TWEEN_ID);
            DOTween.Kill(PROGRESS_TWEEN_ID);

            // Fallback: Kill all tweens on components
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }

            if (_slider != null)
            {
                _slider.DOKill();
            }

            // Stop coroutines
            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }

            if (_loadingTextCoroutine != null)
            {
                StopCoroutine(_loadingTextCoroutine);
                _loadingTextCoroutine = null;
            }

            _isActive = false;
        }

        private void OnDestroy()
        {
            // Kill all tweens by ID immediately (prevents callback errors)
            DOTween.Kill(FADE_TWEEN_ID);
            DOTween.Kill(PROGRESS_TWEEN_ID);

            // Fallback: Kill all tweens on components
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
            }

            if (_slider != null)
            {
                _slider.DOKill();
            }
        }
    }
}