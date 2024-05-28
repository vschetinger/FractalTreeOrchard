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
        //Debug.Log("BeeCollectible: Trigger detected with: " + other.gameObject.name);
        if (other.CompareTag("Bee"))
        {
            BeeAgent beeAgent = other.GetComponent<BeeAgent>();
            if (beeAgent != null)
            {
                // Call the method on the BeeAgent to register the collision
                beeAgent.RegisterFruitCollision(transform.position);
                string collectedWord = GetWord();
                if (!string.IsNullOrEmpty(collectedWord))
                {
                    GameManager.AddCollectedWord(collectedWord);
                }

                // Handle interaction with fruit (e.g., increase energy, etc.)
                beeAgent.energy += 5;  // Example: increase energy by 50 when colliding with a fruit
                Destroy(gameObject);  // Example: destroy the fruit
            }
        }
    }
}