using UnityEngine;
using Card.View;
using Card.Model;
using Game;

namespace Card.Presenter
{
    public class CardPresenter
    {
        private CardModel _model;
        private CardView _view;

        public CardPresenter(CardModel model, CardView view)
        {
            _model = model;
            _model.OnHealthChanged += OnHealthChanged;
            _model.OnManaChanged += OnManaChanged;
            _model.OnAttackChanged += OnAttackChanged;
            _model.OnDeath += OnDeath;
            _model.OnInHandPositionChanged += OnInHandPositionChanged;

            _view = view;
            _view.SetTitle(_model.Title);
            _view.SetDescription(_model.Description);
            _view.SetHealth(_model.Health, 0f);
            _view.SetMana(_model.Mana, 0f);
            _view.SetAttack(_model.Attack, 0f);
            _view.SetIcon(_model.Icon);
            _view.SetLight(false);
            _view.OnMouseDown += OnMouseDown;
            _view.OnMouseUp += OnMouseUp;
            _view.OnMouseDrag += OnMouseDrag;
        }

        private void OnHealthChanged(int health)
        {
            _view.SetHealth(health);
        }

        private void OnManaChanged(int mana)
        {
            _view.SetMana(mana);
        }

        private void OnAttackChanged(int attack)
        {
            _view.SetAttack(attack);
        }

        private void OnDeath()
        {
            _view.FlyOut(2.2f, 2f,
                () => GameManager.Instance.UIManager.CardsArc.RemoveCard(_model),
                () => Object.Destroy(_view.gameObject));
        }

        private void OnMouseDown()
        {
            if (_model.IsInHand)
                _view.SetLight(true);
        }

        private void OnMouseUp()
        {
            if (_model.IsInHand)
            {
                _view.SetLight(false);
                if (GameManager.Instance.UIManager.DropPanel.Drop(_view.transform))
                {
                    _model.IsInHand = false;
                    GameManager.Instance.UIManager.CardsArc.RemoveCard(_model);
                }
                else
                    _view.Return();
            }
        }

        private void OnMouseDrag(Vector2 delta)
        {
            if (_model.IsInHand)
            {
                delta /= new Vector2(Screen.width, Screen.height);
                delta *= (_view.transform.parent.parent as RectTransform).rect.size;
                (_view.transform as RectTransform).anchoredPosition += delta;
            }
        }

        private void OnInHandPositionChanged(float position)
        {
            _view.Angle = position;
        }
    }
}
