using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;

public class PlaneFilter : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public GameObject objectToPlace;
    public GameObject fruitButton;
    public GameObject prefabBanana;
    public GameObject prefabCheese;
    public GameObject prefabWatermelon;
    public Animator animalAnimator;

    public Canvas worldSpaceCanvas; // Canvas en mode World Space, collé à la caméra
    public Transform fruitSpawnAnchor; // Un GameObject positionné en bas-centre de la caméra (voir instructions Unity)

    public TextMeshProUGUI messageText;

    [Tooltip("Tolérance en mètres autour de la hauteur du premier plan détecté.")]
    public float heightTolerance = 0.05f;

    [Tooltip("Surface minimale (en m²) requise pour valider le plan.")]
    public float targetArea = 0.36f;

    private bool referenceHeightSet = false;
    private float referenceHeight;
    private ARPlane selectedPlane;
    private bool trackingCompleted = false;
    private bool readyToSpawn = false;
    private GameObject spawnedAnimal;

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    void Update()
    {
        if (readyToSpawn && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            SpawnObjectOnPlane();
            readyToSpawn = false;
            messageText.text = "";
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var plane in args.added)
            TryProcessPlane(plane);

        foreach (var plane in args.updated)
            TryProcessPlane(plane);
    }

    void TryProcessPlane(ARPlane plane)
    {
        float planeHeight = plane.transform.position.y;

        if (!referenceHeightSet)
        {
            referenceHeight = planeHeight;
            referenceHeightSet = true;
            selectedPlane = plane;
            KeepOnlyThisPlane(plane);
            return;
        }

        if (Mathf.Abs(planeHeight - referenceHeight) > heightTolerance)
        {
            plane.gameObject.SetActive(false);
            return;
        }

        if (plane != selectedPlane)
        {
            plane.gameObject.SetActive(false);
            return;
        }

        float area = CalculatePolygonArea(plane.boundary);

        if (!trackingCompleted && area >= targetArea)
        {
            trackingCompleted = true;
            StartCoroutine(FinalizeTrackingAfterDelay(plane, area));
        }
    }

    void KeepOnlyThisPlane(ARPlane mainPlane)
    {
        foreach (var p in planeManager.trackables)
        {
            if (p != mainPlane)
                p.gameObject.SetActive(false);
        }
    }

    private IEnumerator FinalizeTrackingAfterDelay(ARPlane plane, float area)
    {
        yield return new WaitForSeconds(1f);

        planeManager.enabled = false;

        var meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
        if (meshVisualizer != null) meshVisualizer.enabled = false;

        var renderer = plane.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.enabled = true;

        messageText.text = $"Zone détectée : {area:F2} m²\nTouchez l'écran pour faire apparaître l'animal.";
        readyToSpawn = true;
    }

    private void SpawnObjectOnPlane()
{
    if (objectToPlace != null && selectedPlane != null)
    {
        Vector3 spawnPosition = selectedPlane.transform.position + new Vector3(0f, 0.05f, 0f);
        Quaternion rotation = Quaternion.Euler(0, selectedPlane.transform.eulerAngles.y + 180f, 0);

        spawnedAnimal = Instantiate(objectToPlace, spawnPosition, rotation);
        spawnedAnimal.transform.localScale *= 0.5f;

        // S'assurer que l'Animator est bien assigné dans AnimalBehavior
        var behavior = spawnedAnimal.GetComponent<AnimalBehavior>();
        if (behavior != null && behavior.animator == null)
        {
            behavior.animator = spawnedAnimal.GetComponent<Animator>();
        }

        if (fruitButton != null)
            fruitButton.SetActive(true);
    }
}

    public void OnClickFruitButton()
{
    if (fruitSpawnAnchor == null || spawnedAnimal == null) return;

    GameObject banana = Instantiate(prefabBanana, fruitSpawnAnchor.position + new Vector3(-0.3f, 0, 0), Quaternion.identity, worldSpaceCanvas.transform);
    SetupFruit(banana, "Banane");

    GameObject cheese = Instantiate(prefabCheese, fruitSpawnAnchor.position, Quaternion.identity, worldSpaceCanvas.transform);
    SetupFruit(cheese, "Cheese");

    GameObject watermelon = Instantiate(prefabWatermelon, fruitSpawnAnchor.position + new Vector3(0.3f, 0, 0), Quaternion.identity, worldSpaceCanvas.transform);
    SetupFruit(watermelon, "Pasteque");
}




    private void SetupFruit(GameObject fruit, string fruitName)
{
    SwipeableFruit swipeScript = fruit.GetComponent<SwipeableFruit>();
    if (swipeScript != null)
    {
        swipeScript.animal = spawnedAnimal.GetComponent<AnimalBehavior>();
        swipeScript.fruitName = fruitName;
    }
}


    float CalculatePolygonArea(NativeArray<Vector2> points)
    {
        float area = 0f;
        int j = points.Length - 1;

        for (int i = 0; i < points.Length; i++)
        {
            area += (points[j].x + points[i].x) * (points[j].y - points[i].y);
            j = i;
        }

        return Mathf.Abs(area * 0.5f);
    }
}