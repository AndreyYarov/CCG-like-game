using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Game.Events;

namespace Card.View
{ 
    public class CardView : MonoBehaviour
    {
        [Serializable]
        private class IntField
        {
            [SerializeField] private TextMeshProUGUI m_TextBox;
            private int _value;

            public Color Color
            {
                get => m_TextBox.color;
                set => m_TextBox.color = value;
            }

            public int Value
            {
                get
                {
                    return _value;
                }
                set
                {
                    _value = value;
                    m_TextBox.text = value.ToString();
                }
            }
        }

        [SerializeField] private TextMeshProUGUI m_Title, m_Description;
        [SerializeField] private IntField m_Health, m_Mana, m_Attack;
        [SerializeField] private Image m_CardLight;
        [SerializeField] private Image m_CardIcon;
        [SerializeField] private Color m_NormalTextColor = Color.white, m_GrowingTextColor = Color.green, m_FallingTextColor = Color.red;

        private Dictionary<IntField, Coroutine> _coroutines;
        private Coroutine _angleUpdateCoroutine, _returnCoroutine;
        private bool _drag = false, _return = false;
        private RectTransform _rectTransform;
        private float _angle;

        public RectTransform RectTransform => _rectTransform;
        public float Angle
        {
            get => _angle;
            set
            {
                if (_drag)
                    _angle = value;
                else
                    _angleUpdateCoroutine = StartCoroutine(PositionAnim(value, 1f));
            }
        }

        public event Action OnMouseDown, OnMouseUp;
        public event Action<Vector2> OnMouseDrag;

        private void Awake()
        {
            _coroutines = new Dictionary<IntField, Coroutine>
            {
                { m_Health, null },
                { m_Mana,   null },
                { m_Attack, null },
            };
            _rectTransform = transform as RectTransform;
        }

        private IDisposable mouseEventListener;

        private void Start()
        {
            mouseEventListener = MessageBroker.Default.Receive<MouseEvent>().Subscribe(OnMouseEvent);
        }

        private void OnDestroy()
        {
            mouseEventListener?.Dispose();
        }

        private void OnMouseEvent(MouseEvent e) 
        {
            switch (e.Type)
            {
                case MouseEvent.EventType.MouseDown:
                    if (e.Transform && (e.Transform == transform || e.Transform.IsChildOf(transform)))
                    {
                        _return = false;
                        if (_returnCoroutine != null)
                            StopCoroutine(_returnCoroutine);
                        _drag = true;
                        OnMouseDown?.Invoke();
                    }
                    break;
                case MouseEvent.EventType.MouseUp:
                    if (_drag)
                    {
                        _drag = false;
                        OnMouseUp?.Invoke();
                    }
                    break;
                case MouseEvent.EventType.MouseMove:
                    if (_drag)
                        OnMouseDrag?.Invoke((Vector2)e.Data);
                    break;
            }
        }

        public void SetTitle(string title)
        {
            m_Title.text = title;
        }

        public void SetDescription(string description)
        {
            m_Description.text = description;
        }

        public void SetHealth(int value, float delay = 2f)
        {
            _coroutines[m_Health] = StartCoroutine(TextAnim(m_Health, value, delay));
        }

        public void SetMana(int value, float delay = 2f)
        {
            _coroutines[m_Mana] = StartCoroutine(TextAnim(m_Mana, value, delay));
        }

        public void SetAttack(int value, float delay = 2f)
        {
            _coroutines[m_Attack] = StartCoroutine(TextAnim(m_Attack, value, delay));
        }

        public void SetIcon(Sprite icon)
        {
            m_CardIcon.sprite = icon;
        }

        public void SetLight(bool light)
        {
            m_CardLight.enabled = light;
            if (light)
                transform.SetAsLastSibling();
        }

        public void Return()
        {
            _return = true;
            _returnCoroutine = StartCoroutine(ReturnAnim(1f));
        }

        private IEnumerator ReturnAnim(float delay)
        {
            if (_returnCoroutine != null)
                StopCoroutine(_returnCoroutine);

            (Vector2, Quaternion) start = (RectTransform.anchoredPosition, transform.rotation);
            for (float t = 0f; t < delay; t += Time.deltaTime)
            {
                float k = t / delay;
                Game.GameManager.Instance.UIManager.CardsArc.GetPositionAndRotation(_angle, out var pos, out var rot);
                RectTransform.anchoredPosition = Vector2.Lerp(start.Item1, pos, k);
                RectTransform.rotation = Quaternion.Lerp(start.Item2, rot, k);
                yield return null;
            }

            _return = false;
            UpdateInHandPosition();
        }

        private IEnumerator TextAnim(IntField field, int end, float delay)
        {
            if (_coroutines.TryGetValue(field, out Coroutine coroutine) && coroutine != null)
                StopCoroutine(coroutine);
            int start = field.Value;
            field.Color = end > start ? m_GrowingTextColor : m_FallingTextColor;
            for (float t = delay; t > 0f; t -= Time.deltaTime)
            {
                float k = t / delay;
                k = 1f - k * k;
                field.Value = Mathf.FloorToInt(Mathf.Lerp(start, end, k));
                yield return null;
            }
            field.Value = end;
            yield return new WaitForSeconds(0.2f);
            field.Color = m_NormalTextColor;
        }

        private IEnumerator PositionAnim(float end, float delay)
        {
            if (_angleUpdateCoroutine != null)
                StopCoroutine(_angleUpdateCoroutine);
            float start = _angle;
            for (float t = 0f; t < delay; t += Time.deltaTime)
            {
                _angle = Mathf.Lerp(start, end, t / delay);
                UpdateInHandPosition();
                yield return null;
            }
            _angle = end;
            UpdateInHandPosition();
        }

        public void FlyOut(float time, float delay, Action OnStart, Action OnEnd)
        {
            StartCoroutine(FlyOutAnim(time, delay, OnStart, OnEnd));
        }

        private IEnumerator FlyOutAnim(float time, float delay, Action OnStart, Action OnEnd)
        {
            mouseEventListener?.Dispose();
            yield return new WaitForSeconds(time);

            if (_angleUpdateCoroutine != null)
                StopCoroutine(_angleUpdateCoroutine);
            if (_returnCoroutine != null)
                StopCoroutine(_returnCoroutine);
            OnStart?.Invoke();

            Vector2 start = RectTransform.anchoredPosition;
            Vector2 end = new Vector2(0f, -RectTransform.rect.height);
            for (float t = 0f; t < delay; t += Time.deltaTime)
            {
                RectTransform.anchoredPosition = Vector2.Lerp(start, end, t / delay);
                yield return null;
            }
            RectTransform.anchoredPosition = end;
            OnEnd?.Invoke();
        }

        private void UpdateInHandPosition()
        {
            if (!_return)
            {
                Game.GameManager.Instance.UIManager.CardsArc.GetPositionAndRotation(_angle, out var pos, out var rot);
                RectTransform.anchoredPosition = pos;
                RectTransform.rotation = rot;
            }
        }
    }
}
