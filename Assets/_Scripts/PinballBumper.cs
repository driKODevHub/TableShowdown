using UnityEngine;

// Переконаємось, що на об'єкті є Collider (не тригер!),
// щоб фізика могла розрахувати зіткнення.
[RequireComponent(typeof(Collider))]
public class PinballBumper : MonoBehaviour
{
    [Header("Bumper Settings")]
    [Tooltip("Сила відштовхування. 50-100 = сильний поштовх, 500+ = запуск в повітря.")]
    [SerializeField] private float bounceForce = 150f;

    [Tooltip("Чи додавати силу також 'вгору', щоб трохи підкинути машину.")]
    [SerializeField] private float upwardForceMultiplier = 0.1f;

    [Header("Effects (Optional)")]
    [Tooltip("Ефект партіклів, що створиться в точці удару")]
    [SerializeField] private GameObject impactEffect;
    [Tooltip("Звук, який відтвориться при ударі")]
    [SerializeField] private AudioClip impactSound;

    // Ми будемо кешувати AudioSource, якщо він потрібен
    private AudioSource audioSource;

    private void Awake()
    {
        // Якщо ви додали звук, ми автоматично додамо компонент для його відтворення
        if (impactSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = impactSound;
            audioSource.spatialBlend = 1.0f; // Зробити звук 3D
        }
    }

    /// <summary>
    /// Цей метод викликається АВТОМАТИЧНО, коли інший Rigidbody (наша машинка)
    /// вдаряється в Collider цього об'єкта.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // 1. Перевіряємо, чи є у об'єкта, що вдарився, Rigidbody.
        // У вашому випадку, collision.rigidbody - це і буде той самий `rb`
        // зі скрипта ArcadeVehicleController.
        if (collision.rigidbody == null)
        {
            return; // Це не фізичний об'єкт (наприклад, стіна), ігноруємо.
        }

        // 2. Отримуємо напрямок відштовхування.
        // collision.contacts[0].normal - це вектор, що "дивиться" ВІД поверхні
        // бампера назустріч тому, хто вдарив. Це саме той напрямок, що нам потрібен!
        Vector3 bounceDirection = collision.contacts[0].normal;

        // 3. (Опціонально) Додаємо трохи сили вгору
        // Це дасть ефект "підстрибування"
        if (upwardForceMultiplier > 0)
        {
            bounceDirection = (bounceDirection + Vector3.up * upwardForceMultiplier).normalized;
        }

        // 4. Застосовуємо МИТТЄВУ силу (імпульс) до Rigidbody машинки.
        // ForceMode.Impulse - це саме те, що потрібно для "пінбольного" ефекту.
        collision.rigidbody.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

        // 5. Відтворюємо ефекти в точці зіткнення
        PlayEffects(collision.contacts[0].point, bounceDirection);
    }

    private void PlayEffects(Vector3 impactPoint, Vector3 impactNormal)
    {
        // Ефект партіклів в точці зіткнення
        if (impactEffect != null)
        {
            // Створюємо ефект і повертаємо його так, щоб він "дивився" від бампера
            Instantiate(impactEffect, impactPoint, Quaternion.LookRotation(impactNormal));
        }

        // Звук
        if (audioSource != null && impactSound != null)
        {
            audioSource.Play();
        }
    }
}