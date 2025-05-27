using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections;
using Unity.Collections;
using UnityEngine.UI; // Pour les UI
using TMPro;


public class PlaneFilter : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public GameObject objectToPlace;
    public Canvas canvasUI;
    public TextMeshProUGUI messageText;
    public GameObject fruitButton;

    [Tooltip("Tolérance en mètres autour de la hauteur du premier plan détecté.")]
    public float heightTolerance = 0.05f;

    [Tooltip("Surface minimale (en m²) requise pour valider le plan.")]
    public float targetArea = 0.36f;

    private bool referenceHeightSet = false;
    private float referenceHeight;
    private ARPlane selectedPlane;
    private bool trackingCompleted = false;
    private bool readyToSpawn = false;

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
            messageText.text = ""; // On efface le message
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
        yield return new WaitForSeconds(1f); // Stabilisation

        planeManager.enabled = false;
        messageText.text = $"Zone détectée : {area:F2} m²\nTouchez l’écran pour faire apparaître l’animal.";
        readyToSpawn = true;
    }

    private void SpawnObjectOnPlane()
    {
        if (objectToPlace != null && selectedPlane != null)
        {
            Vector3 spawnPosition = selectedPlane.center;
            Quaternion rotation = Quaternion.Euler(0, selectedPlane.transform.eulerAngles.y + 180f, 0); // rotation 180°
            GameObject obj = Instantiate(objectToPlace, spawnPosition, rotation);
            obj.transform.localScale *= 0.5f; // réduction à 50% de la taille d’origine
        }
        if (fruitButton != null)
            fruitButton.SetActive(true);
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