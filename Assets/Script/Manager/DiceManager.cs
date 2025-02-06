using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class DiceManager : MonoBehaviour
{
    public Transform diceTransform; // Kéo GameObject xúc sắc vào đây
    public float faceRotationDuration = 0.33f; // Mỗi lần xoay mất 0.33s
    public float jumpHeight = 15f; // Chiều cao tối đa của cú nhảy
    public float jumpDuration = 2f; // Thời gian nhảy lên (1 giây)
    public float rotateDuration = 0.4f;
    public float totalDuration = 2f; // Tổng thời gian hiệu ứng (2 giây)

    private Dictionary<int, Quaternion> faceRotations; // Lưu góc quay của từng mặt
    private readonly int minX = -6, maxX = 6, minZ = -6, maxZ = 6;
    private void Start()
    {
        // trạng thái ban đầu của xúc sắc
        diceTransform.rotation = Quaternion.Euler(0, 0, 0);
        diceTransform.position = new Vector3(0f, 0.7f, 0f);
        // Định nghĩa góc quay cho từng mặt xúc sắc
        faceRotations = new Dictionary<int, Quaternion>
        {
            { 1, Quaternion.Euler(-90, 0, 0) },
            { 2, Quaternion.Euler(0, 0, 0) },
            { 3, Quaternion.Euler(0, 0, -90) },
            { 4, Quaternion.Euler(0, 0, 90) },
            { 5, Quaternion.Euler(180, 0, 0) },
            { 6, Quaternion.Euler(90, 0, 0) }
        };
    }

    public void Action(int face)
    {
        StartCoroutine(JumpAndRotateEffect(face));
    }

    private IEnumerator JumpAndRotateEffect(int face)
    {
        float elapsedTime = 0f;
        Vector3 newPosition = diceTransform.position;  
        newPosition.z = 0.7f;                          
        diceTransform.position = newPosition;
        Vector3 startPosition = diceTransform.position;

        // Random vị trí X và Z khi chạm đất
        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        Vector3 endPosition = new Vector3(randomX, startPosition.y, randomZ);

        Quaternion startRotation = diceTransform.rotation;
        Quaternion targetRotation = faceRotations[face]; // Xoay về mặt xúc sắc cần hiển thị


        while (elapsedTime < jumpDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDuration; // Tỷ lệ 0 -> 1

            // Di chuyển theo quỹ đạo parabol
            float height = 4 * jumpHeight * (t - t * t); // Công thức parabol
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);
            currentPosition.y += height;

            diceTransform.position = currentPosition;

            // Xoay dần về target rotation
            diceTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Đảm bảo vị trí cuối chính xác
        diceTransform.position = endPosition;
        diceTransform.rotation = targetRotation;
    }   
}
