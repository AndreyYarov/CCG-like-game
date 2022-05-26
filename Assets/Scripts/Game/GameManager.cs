using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using Game.Events;
using Card.View;
using Card.Model;
using Card.Presenter;
using Game.UI;
using Random = UnityEngine.Random;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private CardView m_CardPrefab;
        [SerializeField] private UIManager m_UIManager;

        public UIManager UIManager => m_UIManager;

        private void PublishMouseEvent(Vector2 position, MouseEvent.EventType eventType, object data = null)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = position;
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);
            if (raycastResults.Count > 0)
                MessageBroker.Default.Publish(new MouseEvent(this, eventType, raycastResults[0].gameObject.transform, data));
            else
                MessageBroker.Default.Publish(new MouseEvent(this, eventType, null, data));
        }

        private void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;
        }

        private IEnumerator Start()
        {
            UIManager.FadeView.SetState(true, 0f, "Loading...");

            lastMousePos = Input.mousePosition;

            CardModel[] cards = new CardModel[Random.Range(4, 6)];
            for (int i = 0; i < cards.Length; i++)
                cards[i] = new CardModel("CARD TITLE", "Card description", Random.Range(1, 10), Random.Range(1, 10), Random.Range(1, 10));


            for (int i = 0; i < cards.Length; i++)
                while (!cards[i].Ready)
                    yield return null;

            for (int i = 0; i < cards.Length; i++)
            {
                CardView view = Instantiate(m_CardPrefab, UIManager.CardsArc.transform);
                CardPresenter presenter = new CardPresenter(cards[i], view);
            }
            UIManager.CardsArc.AddCards(cards);

            UIManager.FadeView.SetState(false, 1f, "Loading...");

            UIManager.TestButton.onClick.AddListener(() =>
            {
                StartCoroutine(TestProcess(cards));
                DestroyImmediate(UIManager.TestButton.gameObject);
            });
        }

        #region test
        private IEnumerator TestProcess(CardModel[] cards)
        {
            List<CardModel> inHandCards = cards.Where(card => card.IsInHand).ToList();
            for (int i = 0; i < inHandCards.Count; i++)
            {
                var card = inHandCards[i];
                card.OnIsInHandChanged += isInHand =>
                {
                    if (!isInHand && inHandCards.Contains(card))
                        inHandCards.Remove(card);
                };
                card.OnDeath += () =>
                {
                    if (inHandCards.Contains(card))
                        inHandCards.Remove(card);
                };
            }

            while (inHandCards.Count != 0)
            {
                for (int i = 0; i < inHandCards.Count; )
                {
                    var card = inHandCards[i];

                    switch (Random.Range(0, 3))
                    {
                        case 0:
                            card.Health = Random.Range(-2, 10);
                            break;
                        case 1:
                            card.Mana = Random.Range(-2, 10);
                            break;
                        case 2:
                            card.Attack = Random.Range(-2, 10);
                            break;
                    }
                    yield return new WaitForSeconds(2.2f);

                    if (i < inHandCards.Count && card == inHandCards[i])
                        i++;
                }
            }
        }
#endregion

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        private int touchId = -1;

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = touchId < 0 ? Input.GetTouch(0) : Array.Find(Input.touches, x => x.fingerId == touchId);

                if (touch.phase == TouchPhase.Began)
                {
                    touchId = touch.fingerId;
                    PublishMouseEvent(touch.position, MouseEvent.EventType.MouseDown);
                }
                else if (touch.fingerId == touchId && touch.phase == TouchPhase.Ended)
                {
                    touchId = -1;
                    PublishMouseEvent(touch.position, MouseEvent.EventType.MouseUp);
                }

                if (touch.deltaPosition != Vector2.zero)
                    PublishMouseEvent(touch.position, MouseEvent.EventType.MouseMove, touch.deltaPosition);
            }
        }
#else

        Vector2 lastMousePos;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                PublishMouseEvent(Input.mousePosition, MouseEvent.EventType.MouseDown);
            else if (Input.GetMouseButtonUp(0))
                PublishMouseEvent(Input.mousePosition, MouseEvent.EventType.MouseUp);
            Vector2 mouseDelta = (Vector2)Input.mousePosition - lastMousePos;
            if (mouseDelta != Vector2.zero)
                PublishMouseEvent(Input.mousePosition, MouseEvent.EventType.MouseMove, mouseDelta);
            lastMousePos = Input.mousePosition;
        }
#endif
    }
}
