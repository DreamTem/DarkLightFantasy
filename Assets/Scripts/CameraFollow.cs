using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Сюди ми покладемо твій Player
    public Vector3 offset = new Vector3(-5f, 2f, 0f); // Зміщення камери (висота і дистанція)
    public float smoothSpeed = 5f; // Швидкість згладжування

    void LateUpdate()
    {
        if (target != null)
        {
            // Вираховуємо точку, де має бути камера
            Vector3 desiredPosition = target.position + offset;
            
            // Плавно переміщуємо камеру в цю точку (Lerp)
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Змушуємо камеру завжди дивитися чітко на куб
            transform.LookAt(target);
        }
    }
}