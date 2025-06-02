using UnityEngine;

public class AnimalBehavior : MonoBehaviour
{
    public Animator animator;
    private string currentAnimation = "";

    void Start()
    {
        PlayRandomAnimation();
    }

    public void PlayRandomAnimation()
    {
        int rand = Random.Range(0, 3);
        switch (rand)
        {
            case 0:
                animator.SetTrigger("DoAttack");
                currentAnimation = "Attack";
                break;
            case 1:
                animator.SetTrigger("DoRoll");
                currentAnimation = "Roll";
                break;
            case 2:
                animator.SetTrigger("DoSit");
                currentAnimation = "Sit";
                break;
        }

        Debug.Log("Animation jouée : " + currentAnimation);
    }

    public void OnFruitSwiped(string fruitName)
    {
        Debug.Log($"Fruit {fruitName} swipé alors que l'animal est en {currentAnimation}");

        if (fruitName == "Banane")
        {
            if (currentAnimation == "Attack")
            {
                animator.SetTrigger("DoEyesHappy");
                Debug.Log("Banane necessaire");
                animator.SetTrigger("DoRoll");
            }
            else
            {
                Debug.Log("T'as pas donné la Banane putain");
                animator.SetTrigger("DoRoll");
                animator.SetTrigger("DoEyesTrauma");
            }
        }
        else if (fruitName == "Pasteque")
        {
            if (currentAnimation == "Roll")
            {
                animator.SetTrigger("DoEyesHappy");
                Debug.Log("Pasteque necessaire");
                animator.SetTrigger("DoSit");
            }
            else
            {
                Debug.Log("T'as pas donné la pasteque  putain");
                animator.SetTrigger("DoSit");
                animator.SetTrigger("DoEyesTrauma");
            }
        }
        else if (fruitName == "Fromage")
        {
            if (currentAnimation == "Sit")
            {
                animator.SetTrigger("DoEyesHappy");
                Debug.Log("Fromage necessaire");
                animator.SetTrigger("DoAttack");
            }
            else
            {
                Debug.Log("T'as pas donné la Fromage putain");
                animator.SetTrigger("DoAttack");
                animator.SetTrigger("DoEyesTrauma");
            }
        }
    }
}
