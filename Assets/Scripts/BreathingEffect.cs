using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathingEffect : MonoBehaviour
{
    private Vector3 initialScale;
    private Vector3 breathInScale;
    private Vector3 breathOutScale;

    // Duration for each phase in seconds
    private float phaseDuration = 1f;

    private Coroutine breathingCoroutine;

    private void Start()
    {
        // Set the initial scale to the object’s current scale
        //initialScale = transform.localScale;
        initialScale = new Vector3(1,1,1);

        // Define scaled-up values relative to the initial scale
        breathInScale = new Vector3(initialScale.x, initialScale.y * 1.05f, initialScale.z);
        breathOutScale = new Vector3(initialScale.x * 1.1f, initialScale.y, initialScale.z * 1.1f);
    }

    private void OnEnable()
    {
        // Start the breathing cycle coroutine whenever the object is enabled
        breathingCoroutine = StartCoroutine(BreathingCycle());
    }

    private void OnDisable()
    {
        // Stop the coroutine when the object is disabled to avoid errors
        if (breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
            transform.localScale = initialScale;
        }
    }

    private IEnumerator BreathingCycle()
    {
        while (true)
        {
            // Transition to breath-in scale
            yield return StartCoroutine(ChangeScaleOverTime(initialScale, breathInScale, phaseDuration));

            // Transition to breath-out scale
            yield return StartCoroutine(ChangeScaleOverTime(breathInScale, breathOutScale, phaseDuration*1.5f));

            // Transition back to initial scale
            yield return StartCoroutine(ChangeScaleOverTime(breathOutScale, initialScale, phaseDuration));
        }
    }

    private IEnumerator ChangeScaleOverTime(Vector3 fromScale, Vector3 toScale, float duration)
    {
        float timeElapsed = 0;

        while (timeElapsed < duration)
        {
            transform.localScale = Vector3.Lerp(fromScale, toScale, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the exact final scale
        transform.localScale = toScale;
    }
}
