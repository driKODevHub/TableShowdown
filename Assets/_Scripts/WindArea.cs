using UnityEngine;
using System.Collections.Generic;

// Переконуємось, що на об'єкті є Collider, і він буде тригером
[RequireComponent(typeof(Collider))]
public class WindArea : MonoBehaviour
{
    [Header("Wind Settings")]
    [Tooltip("Сила вітру. 50-100 = помітний поштовх.")]
    [SerializeField] private float windForce = 50f;

    [Tooltip("Напрямок вітру В МЕЖАХ ЦЬОГО ОБ'ЄКТА. (0, 0, 1) - це 'вперед' по синій осі (Z).")]
    [SerializeField] private Vector3 localWindDirection = Vector3.forward;

    [Tooltip("Режим сили: Acceleration ігнорує масу машини (рекомендовано), Force - враховує.")]
    [SerializeField] private ForceMode forceMode = ForceMode.Acceleration;

    // Список усіх Rigidbody, які зараз в зоні
    private List<Rigidbody> rigidbodiesInZone = new List<Rigidbody>();
    private Vector3 globalWindDirection;

    private void Awake()
    {
        // Переконуємось, що наш collider - це тригер
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("Collider на 'WindArea' (" + name + ") не є тригером! Встановлюю isTrigger = true.", this);
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        // Розраховуємо глобальний напрямок вітру ОДИН РАЗ
        // Це дозволяє вам обертати об'єкт зони, щоб легко міняти напрямок вітру
        globalWindDirection = transform.TransformDirection(localWindDirection.normalized);
    }

    // Коли машина в'їжджає
    private void OnTriggerEnter(Collider other)
    {
        // Ми шукаємо той самий 'rb', до якого ArcadeVehicleController застосовує сили.
        // Ми можемо отримати його через 'attachedRigidbody'
        Rigidbody targetRb = other.attachedRigidbody;

        // Перевіряємо, чи це машина (має Rigidbody) і чи ми ще не додали її
        if (targetRb != null && !rigidbodiesInZone.Contains(targetRb))
        {
            // Важливо! Ми перевіряємо, чи це 'rb' з нашого контролера,
            // а не 'carBody', хоча в вашому випадку вони обидва мають Rigidbody.
            // Найкраще просто перевірити тег "Player".
            if (other.CompareTag("Player"))
            {
                rigidbodiesInZone.Add(targetRb);
            }
        }
    }

    // Коли машина виїжджає
    private void OnTriggerExit(Collider other)
    {
        Rigidbody targetRb = other.attachedRigidbody;

        if (targetRb != null && rigidbodiesInZone.Contains(targetRb))
        {
            rigidbodiesInZone.Remove(targetRb);
        }
    }

    // Застосовуємо силу в кожному кадрі фізики
    private void FixedUpdate()
    {
        // Проходимо по кожному Rigidbody в зоні
        foreach (Rigidbody rb in rigidbodiesInZone)
        {
            if (rb != null)
            {
                // Додаємо постійну силу вітру
                rb.AddForce(globalWindDirection * windForce, forceMode);
            }
        }
    }

    // Візуалізація напрямку вітру в редакторі для зручності
    private void OnDrawGizmos()
    {
        Vector3 globalDir = transform.TransformDirection(localWindDirection.normalized);
        Gizmos.color = new Color(0.5f, 0.8f, 1.0f, 0.5f); // Блакитний колір

        // Малюємо стрілку, що показує напрямок
        Vector3 center = transform.position;
        Vector3 arrowEnd = center + globalDir * 5f; // Довжина стрілки 5 метрів

        Gizmos.DrawLine(center, arrowEnd);
        Gizmos.DrawSphere(arrowEnd, 0.5f);
    }
}