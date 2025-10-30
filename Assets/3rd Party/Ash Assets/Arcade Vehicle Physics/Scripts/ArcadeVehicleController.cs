using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadeVP
{
    public class ArcadeVehicleController : MonoBehaviour
    {
        public enum groundCheck { rayCast, sphereCaste };
        public enum MovementMode { Velocity, AngularVelocity };
        public MovementMode movementMode;
        public groundCheck GroundCheck;
        public LayerMask drivableSurface;

        public float MaxSpeed, accelaration, turn, gravity = 7f, downforce = 5f;
        [Tooltip("if true : can turn vehicle in air")]
        public bool AirControl = false;
        [Tooltip("if true : vehicle will drift instead of brake while holding space")]
        public bool kartLike = false;
        [Tooltip("turn more while drifting (while holding space) only if kart Like is true")]
        public float driftMultiplier = 1.5f;

        public Rigidbody rb, carBody;

        [HideInInspector]
        public RaycastHit hit;
        public AnimationCurve frictionCurve;
        public AnimationCurve turnCurve;
        public PhysicsMaterial frictionMaterial;
        [Header("Visuals")]
        public Transform BodyMesh;
        public Transform[] FrontWheels = new Transform[2];
        public Transform[] RearWheels = new Transform[2];
        [HideInInspector]
        public Vector3 carVelocity;

        [Range(0, 10)]
        public float BodyTilt; // Нахил вліво/вправо

        // --- ОНОВЛЕНІ НАЛАШТУВАННЯ ТІЛЬТУ ---
        [Header("Advanced Tilt Settings")]
        [Tooltip("Сила нахилу (назад) при прискоренні.")]
        public float accelerationTiltStrength = 1.3f; // Було 2.5
        [Tooltip("Сила нахилу (вперед) при АКТИВНОМУ гальмуванні (S або Space).")]
        public float brakingTiltStrength = 1.5f; // Було 3.0
        [Tooltip("НОВЕ: Сила нахилу (вперед) при пасивному сповільненні (коли відпустили газ).")]
        public float passiveDecelerationTiltStrength = 0.3f; // Нове поле
        [Tooltip("Додатковий множник сили при прискоренні з нітро.")]
        public float boostTiltMultiplier = 1.4f; // Було 1.75
        [Tooltip("Максимальний кут нахилу назад (прискорення) в градусах.")]
        public float maxAccelerationPitch = 4f; // Було 8
        [Tooltip("Максимальний кут нахилу вперед (гальмування) в градусах.")]
        public float maxBrakingPitch = 5f; // Було 10
        [Tooltip("Швидкість, з якою кузов нахиляється до цільової позиції.")]
        public float tiltSmoothSpeed = 7f;
        [Tooltip("Швидкість, з якою кузов повертається у 0 (стабілізується).")]
        public float stabilizationSmoothSpeed = 5f;
        // --- КІНЕЦЬ ОНОВЛЕНИХ НАЛАШТУВАНЬ ---

        [Header("Audio settings")]
        public AudioSource engineSound;
        [Range(0, 1)]
        public float minPitch;
        [Range(1, 3)]
        public float MaxPitch;
        public AudioSource SkidSound;

        [HideInInspector]
        public float skidWidth;


        private float radius, steeringInput, accelerationInput, brakeInput;
        private Vector3 origin;

        private PlayerResourceController playerResources;
        private float previousForwardSpeed = 0f;
        private float targetPitch = 0f;
        private float targetRoll = 0f;
        private float currentPitch = 0f;
        private float currentRoll = 0f;
        private Quaternion initialBodyMeshRotation;

        private void Start()
        {
            radius = rb.GetComponent<SphereCollider>().radius;
            if (movementMode == MovementMode.AngularVelocity)
            {
                Physics.defaultMaxAngularSpeed = 100;
            }

            playerResources = GetComponent<PlayerResourceController>();
            if (playerResources == null)
            {
                Debug.LogWarning("ArcadeVehicleController: PlayerResourceController не знайдено. Нахил від нітро не працюватиме.");
            }
            if (BodyMesh != null)
            {
                initialBodyMeshRotation = BodyMesh.localRotation;
            }
        }

        private void Update()
        {
            Visuals();
            AudioManager();
        }

        public void ProvideInputs(float _steeringInput, float _accelarationInput, float _brakeInput)
        {
            steeringInput = _steeringInput;
            accelerationInput = _accelarationInput;
            brakeInput = _brakeInput;
        }

        public void AudioManager()
        {
            engineSound.pitch = Mathf.Lerp(minPitch, MaxPitch, Mathf.Abs(carVelocity.z) / MaxSpeed);
            if (Mathf.Abs(carVelocity.x) > 10 && grounded())
            {
                SkidSound.mute = false;
            }
            else
            {
                SkidSound.mute = true;
            }
        }


        void FixedUpdate()
        {
            carVelocity = carBody.transform.InverseTransformDirection(carBody.linearVelocity);

            if (Mathf.Abs(carVelocity.x) > 0)
            {
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
            }


            if (grounded())
            {
                //turnlogic
                float sign = Mathf.Sign(carVelocity.z);
                float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);
                if (kartLike && brakeInput > 0.1f) { TurnMultiplyer *= driftMultiplier; }


                if (accelerationInput > 0.1f || carVelocity.z > 1)
                {
                    carBody.AddTorque(Vector3.up * steeringInput * sign * turn * 100 * TurnMultiplyer);
                }
                else if (accelerationInput < -0.1f || carVelocity.z < -1)
                {
                    carBody.AddTorque(Vector3.up * steeringInput * sign * turn * 100 * TurnMultiplyer);
                }

                // mormal brakelogic
                if (!kartLike)
                {
                    if (brakeInput > 0.1f)
                    {
                        rb.constraints = RigidbodyConstraints.FreezeRotationX;
                    }
                    else
                    {
                        rb.constraints = RigidbodyConstraints.None;
                    }
                }

                //accelaration logic
                if (movementMode == MovementMode.AngularVelocity)
                {
                    if (Mathf.Abs(accelerationInput) > 0.1f && brakeInput < 0.1f && !kartLike)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * accelerationInput * MaxSpeed / radius, accelaration * Time.deltaTime);
                    }
                    else if (Mathf.Abs(accelerationInput) > 0.1f && kartLike)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * accelerationInput * MaxSpeed / radius, accelaration * Time.deltaTime);
                    }
                }
                else if (movementMode == MovementMode.Velocity)
                {
                    if (Mathf.Abs(accelerationInput) > 0.1f && brakeInput < 0.1f && !kartLike)
                    {
                        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carBody.transform.forward * accelerationInput * MaxSpeed, accelaration / 10 * Time.deltaTime);
                    }
                    else if (Mathf.Abs(accelerationInput) > 0.1f && kartLike)
                    {
                        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carBody.transform.forward * accelerationInput * MaxSpeed, accelaration / 10 * Time.deltaTime);
                    }
                }

                rb.AddForce(-transform.up * downforce * rb.mass);
                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, hit.normal) * carBody.transform.rotation, 0.12f));

                CalculateBodyPitch();
            }
            else
            {
                if (AirControl)
                {
                    float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);
                    carBody.AddTorque(Vector3.up * steeringInput * turn * 100 * TurnMultiplyer);
                }

                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, Vector3.up) * carBody.transform.rotation, 0.02f));
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity + Vector3.down * gravity, Time.deltaTime * gravity);

                targetPitch = 0f;
            }

            previousForwardSpeed = carVelocity.z;
        }

        // --- ОНОВЛЕНИЙ МЕТОД РОЗРАХУНКУ НАХИЛУ ---
        private void CalculateBodyPitch()
        {
            float currentForwardSpeed = carVelocity.z;
            float acceleration = (currentForwardSpeed - previousForwardSpeed) / Time.fixedDeltaTime;

            bool isBoosting = (playerResources != null && playerResources.IsBoosting);
            bool isActivelyBraking = (brakeInput > 0.1f || accelerationInput < -0.1f);

            if (acceleration > 0.1f) // Прискорення
            {
                float multiplier = isBoosting ? boostTiltMultiplier : 1.0f;
                float tilt = -acceleration * accelerationTiltStrength * multiplier;
                float maxPitch = maxAccelerationPitch * (isBoosting ? boostTiltMultiplier : 1.0f);
                targetPitch = Mathf.Clamp(tilt, -maxPitch, 0f);
            }
            else if (acceleration < -0.1f) // Сповільнення (будь-яке)
            {
                float tiltStrength = isActivelyBraking ? brakingTiltStrength : passiveDecelerationTiltStrength;
                float tilt = -acceleration * tiltStrength;
                targetPitch = Mathf.Clamp(tilt, 0f, maxBrakingPitch);
            }
            else
            {
                // Стабілізація
                targetPitch = 0f;
            }
        }
        // --- КІНЕЦЬ ОНОВЛЕНОГО МЕТОДУ ---

        public void Visuals()
        {
            //tires
            foreach (Transform FW in FrontWheels)
            {
                FW.localRotation = Quaternion.Slerp(FW.localRotation, Quaternion.Euler(FW.localRotation.eulerAngles.x,
                                   30 * steeringInput, FW.localRotation.eulerAngles.z), 0.7f * Time.deltaTime / Time.fixedDeltaTime);
                FW.GetChild(0).localRotation = rb.transform.localRotation;
            }
            RearWheels[0].localRotation = rb.transform.localRotation;
            RearWheels[1].localRotation = rb.transform.localRotation;

            // Розрахунок нахилу вліво/вправо
            if (carVelocity.z > 1)
            {
                targetRoll = BodyTilt * steeringInput;
            }
            else
            {
                targetRoll = 0f;
            }

            if (kartLike)
            {
                if (brakeInput > 0.1f)
                {
                    BodyMesh.parent.localRotation = Quaternion.Slerp(BodyMesh.parent.localRotation,
                    Quaternion.Euler(0, 45 * steeringInput * Mathf.Sign(carVelocity.z), 0),
                    0.1f * Time.deltaTime / Time.fixedDeltaTime);
                }
                else
                {
                    BodyMesh.parent.localRotation = Quaternion.Slerp(BodyMesh.parent.localRotation,
                    Quaternion.Euler(0, 0, 0),
                    0.1f * Time.deltaTime / Time.fixedDeltaTime);
                }
            }
        }

        private void LateUpdate()
        {
            if (BodyMesh == null) return;

            // Плавно рухаємо поточний нахил (Pitch) до цільового
            float pitchSpeed = (Mathf.Abs(targetPitch) > 0.01f) ? tiltSmoothSpeed : stabilizationSmoothSpeed;
            currentPitch = Mathf.Lerp(currentPitch, targetPitch, pitchSpeed * Time.deltaTime);

            // Плавно рухаємо поточний нахил (Roll) до цільового
            float rollSpeed = (Mathf.Abs(targetRoll) > 0.01f) ? tiltSmoothSpeed : stabilizationSmoothSpeed;
            currentRoll = Mathf.Lerp(currentRoll, targetRoll, rollSpeed * Time.deltaTime);

            BodyMesh.localRotation = initialBodyMeshRotation * Quaternion.Euler(currentPitch, 0, currentRoll);
        }

        public bool grounded()
        {
            origin = rb.position + rb.GetComponent<SphereCollider>().radius * Vector3.up;
            var direction = -transform.up;
            var maxdistance = rb.GetComponent<SphereCollider>().radius + 0.2f;

            if (GroundCheck == groundCheck.rayCast)
            {
                if (Physics.Raycast(rb.position, Vector3.down, out hit, maxdistance, drivableSurface))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (GroundCheck == groundCheck.sphereCaste)
            {
                if (Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, drivableSurface))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else { return false; }
        }

        private void OnDrawGizmos()
        {
            radius = rb.GetComponent<SphereCollider>().radius;
            float width = 0.02f;
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(rb.transform.position + ((radius + width) * Vector3.down), new Vector3(2 * radius, 2 * width, 4 * radius));
                if (GetComponent<BoxCollider>())
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider>().size);
                }
            }
        }
    }
}