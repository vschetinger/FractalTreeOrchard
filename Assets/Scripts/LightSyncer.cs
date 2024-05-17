using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightSyncer : MonoBehaviour
{
    private Light directionalLight;

    private void Start()
    {
        directionalLight = GetComponent<Light>();
    }

    private void Update()
    {
        if (GameManager.instance != null && directionalLight != null)
        {
            directionalLight.transform.rotation = GameManager.instance.GetDirectionalLightRotation();
        }
    }
}
