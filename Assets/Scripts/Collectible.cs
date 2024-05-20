using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class Collectible : MonoBehaviour
{
    private TextMeshPro textMeshPro;


    private void Start()
    {
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        
        // Generate a random duration between minDuration and maxDuration
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            string word = textMeshPro.text;
            GameManager.AddCollectedWord(word);
            Debug.Log("Collected: " + word);
            Destroy(gameObject);
        }
    }
}