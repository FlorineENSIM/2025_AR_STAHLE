using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeableFruit : MonoBehaviour, IDragHandler, IEndDragHandler
{
    private Vector2 startDragPos;
    private bool swiped = false;
    public Animator animalAnimator; 
    public AnimalBehavior animal;
    public string fruitName = "Banane";

    void OnMouseDown()
    {
        animal.OnFruitSwiped(fruitName);
        Destroy(gameObject);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!swiped)
        {
            Vector2 delta = eventData.position - startDragPos;

            if (Mathf.Abs(delta.x) > 100f)
            {
                swiped = true;
                Destroy(gameObject);
                if (animalAnimator != null)
                    animalAnimator.SetTrigger("DoRoll");
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        startDragPos = Vector2.zero;
    }

    void Start()
    {
        startDragPos = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"{gameObject.name} : Drag détecté !!!");
        Debug.Log($"{gameObject.name} : Drag détecté !!!");
        startDragPos = eventData.position;
    }
}
