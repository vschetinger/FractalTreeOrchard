using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class Collectible : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    private RectTransform healthBarTransform;
    public float minDuration = 20f; // Minimum duration in seconds
    public float maxDuration = 40f; // Maximum duration in seconds
    private float duration; // Actual duration of the fruit

    private void Start()
    {
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        healthBarTransform = transform.Find("FruitHealthBarCanvas/HealthBar").GetComponent<RectTransform>();
        
        // Generate a random duration between minDuration and maxDuration
        duration = UnityEngine.Random.Range(minDuration, maxDuration);
        
        StartCoroutine(DepleteHealthBar());
    }

    private IEnumerator DepleteHealthBar()
    {
        float elapsedTime = 0f;
        float startWidth = healthBarTransform.sizeDelta.x;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float newWidth = Mathf.Lerp(startWidth, 0f, t);
            healthBarTransform.sizeDelta = new Vector2(newWidth, healthBarTransform.sizeDelta.y);
            yield return null;
        }

        // Destroy the collectible when the health bar is fully depleted
        Destroy(gameObject);
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