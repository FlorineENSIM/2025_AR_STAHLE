using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitRotation : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}
