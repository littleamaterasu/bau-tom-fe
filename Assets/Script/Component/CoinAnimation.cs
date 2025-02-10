using UnityEngine;

public class CoinAnimation : MonoBehaviour
{
    public ParticleSystem coinSystem;
    public Vector3 startPos, endPos;

    public void SpawnCoins()
    {
        Vector3 dir = (endPos - startPos).normalized;

        // Tính toán góc quay theo trục X
        float angleX = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Tạo Quaternion mới để xoay
        Quaternion targetRotation = Quaternion.Euler(30, 45, 0) * Quaternion.Euler(-angleX, 90, 0);

        Debug.Log("coin system rotation " + targetRotation);

        coinSystem.transform.rotation = targetRotation;

        // Kích hoạt Particle System
        coinSystem.Play();
    }

    public void SetEndPos(Vector3 pos)
    {
        endPos = pos;
    }

    public Vector3 GetStartPos()
    {
        return startPos;
    }
}
