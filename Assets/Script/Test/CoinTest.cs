using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class CoinTest : MonoBehaviour
{
    public Button target;
    public ParticleSystem coinSystem;
    public Vector3 startPos;
    public Collider targetCollider;

    private List<Particle> particles;

    private void Start()
    {
        particles = new List<Particle>();
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => SpawnCoins());
    }

    public void SpawnCoins()
    {
        Transform end = target.transform;
        Vector3 dir = (end.position - gameObject.transform.position).normalized;

        // Tính toán góc quay theo trục X
        float angleX = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Tạo Quaternion mới để xoay
        Quaternion targetRotation = Quaternion.Euler(-angleX, 90, 0);

        // Cập nhật vị trí và góc của Particle System
        coinSystem.transform.position = startPos;
        coinSystem.transform.rotation = targetRotation;

        // Kích hoạt Particle System
        coinSystem.Play();
    }
}
