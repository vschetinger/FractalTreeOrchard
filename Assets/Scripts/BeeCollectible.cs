using System;
using UnityEngine;
using TMPro;

public class BeeCollectible : MonoBehaviour
{
    private TextMeshPro textMeshPro;

    private void Start()
    {
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        // Generate a random duration between minDuration and maxDuration if needed

        // Debugging: Check if Collider is present
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("BeeCollectible: Collider component is missing!");
        }
    }

    public string GetWord()
    {
        if (textMeshPro != null)
        {
            //Debug.Log("GetWord returned: " + textMeshPro.text);
            return textMeshPro.text;
        }
        return string.Empty;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bee"))
        {
            BeeAgent beeAgent = other.GetComponent<BeeAgent>();
            if (beeAgent != null)
            {
                string collectedWord = GetWord();
                if (!string.IsNullOrEmpty(collectedWord))
                {
                    beeAgent.RegisterFruitCollision(transform.position, collectedWord);
                    Destroy(gameObject);  // Example: destroy the fruit
                }
            }
        }
    }
}