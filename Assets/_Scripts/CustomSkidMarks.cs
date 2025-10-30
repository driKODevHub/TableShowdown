using UnityEngine;
using ArcadeVP; // Нам потрібен цей namespace, щоб читати дані з контролера

// ВАЖЛИВО: Це наш кастомний скрипт, НЕ з асету.
[RequireComponent(typeof(TrailRenderer), typeof(ParticleSystem))]
public class CustomSkidMarks : MonoBehaviour
{
    // Посилання на компоненти, які ми отримаємо автоматично
    private TrailRenderer skidMark;
    private ParticleSystem smoke;

    // Посилання на контролер машини (потрібно перетягнути в інспекторі)
    [Header("Core References")]
    [Tooltip("Перетягни сюди головний об'єкт машини з ArcadeVehicleController")]
    [SerializeField] private ArcadeVehicleController carController;

    // Змінні для інпутів
    private float verticalInput; // W/S
    private float brakeInput; // Space (Jump)
    private bool isBoosting; // Left Shift

    [Header("Skid Settings")]
    [Tooltip("Мінімальна бокова швидкість (дріфт) для появи слідів.")]
    [SerializeField] private float minDriftSpeed = 10f;
    [Tooltip("Мінімальна загальна швидкість (вперед/назад) для появи слідів при натисканні 'Space'.")]
    [SerializeField] private float minBrakingSpeed = 20f;
    [Tooltip("Мінімальна швидкість (Вперед) для появи слідів при гальмуванні заднім ходом (S).")]
    [SerializeField] private float minReverseBrakingSpeed = 15f;

    // --- НОВІ НАЛАШТУВАННЯ ---
    [Header("Burnout Settings")]
    [Tooltip("Максимальна швидкість, при якій 'burnout' (старт з нітро) залишає сліди.")]
    [SerializeField] private float maxBurnoutSpeed = 30f;


    // Змінна для логіки затухання
    private float fadeOutSpeed;

    private void Awake()
    {
        // Отримуємо компоненти, які висять на цьому ж об'єкті
        smoke = GetComponent<ParticleSystem>();
        skidMark = GetComponent<TrailRenderer>();

        // Критична перевірка: без контролера скрипт не має сенсу
        if (carController == null)
        {
            Debug.LogError("CustomSkidMarks: 'Car Controller' не призначено в інспекторі! Скрипт вимкнено.", this);
            enabled = false;
            return;
        }

        // Ми можемо зчитати skidWidth, бо це public поле
        skidMark.startWidth = carController.skidWidth;
        skidMark.emitting = false;
    }

    private void OnEnable()
    {
        if (skidMark != null)
            skidMark.enabled = true;
    }
    private void OnDisable()
    {
        if (skidMark != null)
            skidMark.enabled = false;
    }

    // Зчитуємо інпути в Update()
    private void Update()
    {
        if (!enabled) return; // Не працюємо, якщо скрипт вимкнено (напр., в Awake)

        verticalInput = Input.GetAxis("Vertical");
        brakeInput = Input.GetAxis("Jump");
        isBoosting = Input.GetKey(KeyCode.LeftShift); // Зчитуємо нітро
    }


    // Оновлюємо логіку в FixedUpdate()
    void FixedUpdate()
    {
        if (!enabled) return; // Не працюємо, якщо скрипт вимкнено

        if (carController.grounded())
        {
            // Отримуємо поточні швидкості з контролера
            float lateralSpeed = Mathf.Abs(carController.carVelocity.x); // Бокова швидкість
            float forwardSpeed = carController.carVelocity.z; // Пряма швидкість (може бути від'ємною)
            float overallSpeed = carController.rb.linearVelocity.magnitude; // Загальна швидкість


            // --- ОНОВЛЕНА ЛОГІКА СЛІДІВ ---

            // 1. Дріфт (без змін)
            bool isDrifting = lateralSpeed > minDriftSpeed;

            // 2. Гальмування (Space) на швидкості (без змін)
            bool isBraking = brakeInput > 0.1f && overallSpeed > minBrakingSpeed;

            // 3. Гальмування заднім ходом (S) - ОНОВЛЕНО
            // Тепер спрацьовує ТІЛЬКИ якщо ми їхали швидко вперед і натиснули S
            bool isReverseBraking = verticalInput < -0.1f && forwardSpeed > minReverseBrakingSpeed;

            // 4. Пробуксовка "Burnout" (НОВА УМОВА)
            // Спрацьовує якщо тиснемо W + Shift, але швидкість ще низька
            bool isBurnout = verticalInput > 0.1f && isBoosting && overallSpeed < maxBurnoutSpeed;


            // Вмикаємо сліди, ЯКЩО виконується ХОЧ ОДНА з умов
            if (isDrifting || isBraking || isReverseBraking || isBurnout)
            {
                fadeOutSpeed = 0f;
                // Додаємо перевірку, що матеріали існують
                if (skidMark.materials.Length > 0)
                    skidMark.materials[0].color = Color.black;

                skidMark.emitting = true;
            }
            else
            {
                skidMark.emitting = false;
            }
        }
        else
        {
            skidMark.emitting = false;
        }

        // Логіка затухання слідів
        if (!skidMark.emitting)
        {
            fadeOutSpeed += Time.deltaTime / 2;
            if (skidMark.materials.Length > 0)
            {
                Color m_color = Color.Lerp(Color.black, new Color(0f, 0f, 0f, 0f), fadeOutSpeed);
                skidMark.materials[0].color = m_color;
            }
            if (fadeOutSpeed > 1)
            {
                skidMark.Clear();
            }
        }

        // Логіка диму (з оптимізацією, щоб не викликати Play/Stop щокадру)
        if (skidMark.emitting == true)
        {
            if (!smoke.isPlaying)
                smoke.Play();
        }
        else
        {
            if (smoke.isPlaying)
                smoke.Stop();
        }
    }
}

