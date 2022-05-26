using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Card.Model
{
    [Serializable]
    public class CardModel
    {
        [SerializeField] private int _health, _mana, _attack;
        [SerializeField] private string _title, _description;
        [SerializeField] private Sprite _icon;
        private float _inHandPosition;
        private bool _isInHand;

        public event Action<int> OnHealthChanged, OnManaChanged, OnAttackChanged;
        public event Action OnDeath;
        public event Action<float> OnInHandPositionChanged;
        public event Action<bool> OnIsInHandChanged;

        private const int IconWidth = 172, IconHeight = 144;

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public int Health
        {
            get => _health;
            set
            {
                if (_health != value)
                {
                    _health = value >= 0 ? value : 0;
                    OnHealthChanged?.Invoke(_health);
                    if (_health == 0)
                        OnDeath?.Invoke();
                }
            }
        }

        public int Mana
        {
            get => _mana;
            set
            {
                if (_mana != value)
                {
                    _mana = value >= 0 ? value : 0;
                    OnManaChanged?.Invoke(_mana);
                }
            }
        }

        public int Attack
        {
            get => _attack;
            set
            {
                if (_attack != value)
                {
                    _attack = value >= 0 ? value : 0;
                    OnAttackChanged?.Invoke(_attack);
                }
            }
        }

        public float InHandPosition
        {
            get => _inHandPosition;
            set
            {
                _inHandPosition = value;
                if (_isInHand)
                    OnInHandPositionChanged?.Invoke(_inHandPosition);
            }
        }

        public bool IsInHand
        {
            get => _isInHand;
            set
            {
                _isInHand = value;
                OnIsInHandChanged?.Invoke(_isInHand);
            }
        }

        public Sprite Icon => _icon;

        public bool Ready => _icon;

        public CardModel(string title, string description, int health, int mana, int attack, bool isInHand = true)
        {
            _title = title;
            _description = description;
            _health = health;
            _mana = mana;
            _attack = attack;
            _isInHand = isInHand;
            DownloadIcon();            
        }

        private void DownloadIcon()
        {
            var www = UnityWebRequestTexture.GetTexture($"https://picsum.photos/{IconWidth}/{IconHeight}");
            www.SendWebRequest().completed += _ =>
            {
                var texture = DownloadHandlerTexture.GetContent(www);
                _icon = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            };
        }
    }
}
