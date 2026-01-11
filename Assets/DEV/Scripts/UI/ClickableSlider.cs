using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DEV.Scripts.UI
{
    /// <summary>
    /// Slider'a tıklanan yere direkt gider (YouTube tarzı)
    /// Slider component'i ile birlikte kullanılır.
    /// Background objesine tıklamayı algılar.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ClickableSlider : MonoBehaviour, IPointerDownHandler
    {
        [Header("References")]
        [Tooltip("Tıklamayı algılayacak background objesi (Image component'i olmalı)")]
        [SerializeField] private Image clickableBackground;
        
        private Slider _slider;
        private RectTransform _sliderRect;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _sliderRect = GetComponent<RectTransform>();
            
            // Background'ın raycast target'ini aktif et
            if (clickableBackground != null)
            {
                clickableBackground.raycastTarget = true;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_slider == null || _sliderRect == null) return;

            // Tıklanan pozisyonu slider'ın local koordinatlarına çevir
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _sliderRect, 
                eventData.position, 
                eventData.pressEventCamera, 
                out localPoint))
            {
                // Slider'ın yönüne göre değeri hesapla
                float normalizedValue;
                
                if (_slider.direction == Slider.Direction.LeftToRight || _slider.direction == Slider.Direction.RightToLeft)
                {
                    // Horizontal slider
                    float sliderWidth = _sliderRect.rect.width;
                    normalizedValue = (localPoint.x + sliderWidth / 2f) / sliderWidth;
                    
                    // RightToLeft ise tersine çevir
                    if (_slider.direction == Slider.Direction.RightToLeft)
                    {
                        normalizedValue = 1f - normalizedValue;
                    }
                }
                else
                {
                    // Vertical slider
                    float sliderHeight = _sliderRect.rect.height;
                    normalizedValue = (localPoint.y + sliderHeight / 2f) / sliderHeight;
                    
                    // BottomToTop ise tersine çevir
                    if (_slider.direction == Slider.Direction.TopToBottom)
                    {
                        normalizedValue = 1f - normalizedValue;
                    }
                }

                // Normalized value'yu clamp et
                normalizedValue = Mathf.Clamp01(normalizedValue);
                
                // Slider'ın min/max değerlerine göre gerçek değeri hesapla
                float targetValue = Mathf.Lerp(_slider.minValue, _slider.maxValue, normalizedValue);
                
                // Slider'a değeri set et
                _slider.value = targetValue;
            }
        }
    }
}

