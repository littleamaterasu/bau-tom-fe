using TMPro;
using UnityEngine;

public class Effeciency : MonoBehaviour
{
    public TMP_Text fpsText;  // Assign in Inspector
    public TMP_Text glCallsText;  // Assign in Inspector

    private int frameCount = 0;
    private float elapsedTime = 0f;
    private int lastDrawCallCount = 0;

    void Update()
    {
        // Track FPS
        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime >= 1f) // Update every second
        {
            int fps = Mathf.RoundToInt(frameCount / elapsedTime);
            fpsText.text = $"FPS: {fps}";
            frameCount = 0;
            elapsedTime = 0f;
        }

        // Track OpenGL draw calls
        int currentDrawCallCount = GetGLDrawCalls();
        glCallsText.text = $"GL Calls: {currentDrawCallCount}";
    }

    int GetGLDrawCalls()
    {
        // Get Unity's built-in draw call count (Only works in Unity Editor)
#if UNITY_EDITOR
        return UnityEditor.UnityStats.drawCalls;
#else
            return lastDrawCallCount; // Cannot get real GL calls in build
#endif
    }
}
