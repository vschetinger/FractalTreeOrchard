using System;
using UnityEngine;
using TMPro;

public class Collectible : MonoBehaviour
{
    private TextMeshPro textMeshPro;

    private void Start()
    {
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        // Generate a random duration between minDuration and maxDuration if needed
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectWordForPlayer(other.gameObject);
        }
        else if (other.CompareTag("Bee"))
        {
            CollectWordForBee(other.gameObject);
        }
    }

    private void CollectWordForPlayer(GameObject player)
    {
        string word = textMeshPro.text;
        GameManager.AddCollectedWord(word);
        Debug.Log("Collected by player: " + word);
        Destroy(gameObject);
    }

    private void CollectWordForBee(GameObject bee)
    {
        // Increase bee's energy or apply other logic
        BeeAgent beeAgent = bee.GetComponent<BeeAgent>();
        if (beeAgent != null)
        {
            beeAgent.energy += 50; // Example: increase energy by 50
        }
        Debug.Log("Collected by bee");
        Destroy(gameObject);
    }
}