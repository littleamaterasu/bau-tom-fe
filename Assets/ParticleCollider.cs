using UnityEngine;

public class ParticleCollider : MonoBehaviour
{
    public GameObject target;
    public float scaleMultiplier = 1.1f;
    public float scaleDuration = 0.2f;

    private Vector3 originalScale;
    private int count = 0;

    private void Start()
    {
        originalScale = target.transform.localScale;
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("Collision detected!");
        ++count;
        if ((count & 3) == 1)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleEffect());
        }
    }

    private System.Collections.IEnumerator ScaleEffect()
    {
        Vector3 targetScale = originalScale * scaleMultiplier;
        float elapsedTime = 0;

        // Phóng to
        while (elapsedTime < scaleDuration)
        {
            target.transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        target.transform.localScale = targetScale;

        elapsedTime = 0;
        while (elapsedTime < scaleDuration)
        {
            target.transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        target.transform.localScale = originalScale;
    }
}
