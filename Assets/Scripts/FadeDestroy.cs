using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeDestroy : MonoBehaviour
{
    private Material material;
    private float fadeDuration = 5f; // Duration for the fade-out effect

    private void Start()
    {
        // Get the material of the object
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
            StartCoroutine(WaitBeforeFade());
        }
        else
        {
            Debug.LogWarning("No Renderer found on the object. Destroying immediately.");
            Destroy(gameObject); // Destroy immediately if no renderer is found
        }
    }
    
    private IEnumerator WaitBeforeFade()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(FadeOutAndDestroy());
    }
    private IEnumerator FadeOutAndDestroy()
    {
        float startAlpha = material.color.a; // Initial alpha
        float timeElapsed = 0f;

        // Gradually reduce alpha over fadeDuration
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0, timeElapsed / fadeDuration);

            // Update material color with new alpha
            Color newColor = material.color;
            newColor.a = newAlpha;
            material.color = newColor;

            yield return null; // Wait until the next frame
        }

        // Ensure the object is completely transparent and destroy it
        material.color = new Color(material.color.r, material.color.g, material.color.b, 0);
        Destroy(gameObject);
    }
}
