using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    enum WeaponMode
    {
        None,
        Pistol,
        ShotGun,
        Rifle,
        SMG
    }


    

    private WeaponMode currentWeaponMode = WeaponMode.None;

    public float recoilStrength = 2f; //�ݵ��� ����
    public float maxRecoliAngle = 10.0f; //�ݵ��� �ִ� ����
    private float currenRecoil = 0f; //���� �ݵ� ���� �����ϴ� ����

    public LayerMask hitLayer;
    public float rayDistance = 100f;

    private int shotGunRayCount = 5; //���� �Ѿ��� ������ ��
    private float shotGunSpreadAngle = 10f;

    float moveWalkSpeed = 3.0f; //�Ϲ� �ӵ�
    float moveRunSpeed = 5.0f; //�޸��� �ӵ�
    float currentSpeed = 1.0f; //����ӵ�
    bool isRunning = false;
    float gravity = -9.81f; //�߷°�
    Vector3 velocity; //���� �ӵ� ����
    CharacterController characterController;

    float mouseSensitivity = 100f; //���콺 ����
    public Transform cameraTransfrom; // ī�޶� Transfrom
    public Transform playerHead; //�÷��̾� �Ӹ� ��ġ(1��Ī ����϶� ���)
    public float thirdPersonDistance = 3.0f; // 3��Ī ��忡�� �÷��̾�� ī�޶� �þ� �Ÿ�
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0); // 3��Ī ��忡�� ī�޶� ������
    public Transform playerLookObj; //�÷��̾��� �þ� ��ġ

    public float zoomedDistance = 1.0f; //ī�޶� Ȯ��� ���� �Ÿ�(3��Ī ����϶����)
    public float zoomSpeed = 5f; //Ȯ�� ��Ұ� �Ǵ� �ӵ�
    public float defaultFov = 60f; //�⺻ ī�޶� �þ߰�
    public float zoomedFov = 40f; // Ȯ�� �� ī�޶� �þ߰�
   private float defaultDistance = 10.0f;

    float currenDistance; //���� ī�޶���� �Ÿ�
    float targetDistance; //��ǥ ī�޶� �Ÿ�
    float targetFov; //��ǥ FOV
    bool isZoomed = false; //Ȯ�� ����
    private Coroutine zoomCoroutine; //�ڷ�ƾ�� ����Ͽ� Ȯ��/���
    Camera mainCamera; //ī�޶� ������Ʈ

    float pitch = 0f; //���Ʒ� ȸ����
    float yaw = 0f; //�¿� ȸ����
    bool isFirstPerson = false; //1��Ī ��忩��
    bool rotaterAroundPlayer = true; //ī�޶� �÷��̾� ������ ȸ���ϴ� ����
    public float jumpHeight = 2f; //���� ����
    bool isGround; //���� �浹 ����
    public LayerMask groundLayer;

    Animator animator;
    float horizontal;
    float vertical;

    public Transform leftFoot;
    public Transform rightFoot;

    public float minMoveDistance = 0.01f;

    bool IsLeftFootGround = false;
    public bool IsRightFootGround = false;

    Vector3 previousPosition;

    public float fallThreshold = -10f;
    bool IsGameOver = false;

    bool isJumping = false;  // ĳ���Ͱ� ���� ������ ����

    public float hp = 100;

    bool isAiming = false;
    bool isFiring = false;

    public Transform upperBody; //��ü ���� �Ҵ�(Spine, UpperChest)
    public float upperBodyRotationAngle = -30f; //��ü Aim ��忡���� ȸ���� �Ѵ�.
    private Quaternion originalUpperBodyRotation; //���� ��ü ȸ�� ��

    public Transform aimTarget;

    public float slowMotionScale = 0.5f;
    private float defaultTimeScale = 1.0f;
    private bool isSlowMotion = false;

    //������ ����
    public Vector3 boxSize = new Vector3(1, 1, 1);
    public float castDistance = 5f; //BoxCast �ָ� ���� �Ÿ� 
    public LayerMask itemLayer; //�����۷��̾� 
    public Transform itemGetPos; // BoxCast ��ġ 
    public float debugDuration = 2.0f; // ����� ���� ���� 

    bool isGetItemAction = false;


    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;

    private Vector3 originalPos;

    public GameObject NoneCrossHair;
    public GameObject CrossHair;

    private bool isFire = true; // �߻翩��  -> false�϶��� false
    public float fireDelay = 0.1f; // ���������� ���� ��ġ�� ��ȭ�ؼ� ���� ���� ��ȭ�� �ش�.

    //public float pistolFireDelay = 0.7f;  // ���� �߻� ������
    //public float shotGunDelay = 1.0f;  // ���� ������
    //public float rifleDelay = 1.2f; //������ ������ 
    //public float SMGFireDelay = 0.1f; //������� ������

    public Light flashLight;
    public bool isFlashLightOn = false;

    private Rigidbody[] ragdollbodies;
    private Collider[] ragdollcolliders;

    public Transform effectPos;

    public GameObject PistolUI;
    public GameObject ShotGunUI;
    public GameObject HPUI;
    //public GameObject LightUI;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
         
        ShotGunUI.SetActive(false);
        PistolUI.SetActive(false);
        isFirstPerson = true;
        Cursor.lockState = CursorLockMode.Locked;
        currenDistance = thirdPersonDistance; //�ʱ� ī�޶� �Ÿ��� ����
        targetDistance = thirdPersonDistance; //��ǥ ī�޶� �Ÿ� ����
        targetFov = defaultFov; //�ʱ� FOV ����
        mainCamera = cameraTransfrom.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov; //�⺻ fov ����
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        previousPosition = transform.position; //���۽� ���� ��ġ�� ���� ��ġ�� �ʱ�ȭ

        animator.SetLayerWeight(1, 0);

        if (upperBody != null)
        {
            originalUpperBodyRotation = upperBody.localRotation;
        }
        
        ragdollbodies = GetComponentsInChildren<Rigidbody>();
        ragdollcolliders = GetComponentsInChildren<Collider>();

       
    }

    void Update()
    {
        if (transform.position.y < fallThreshold && !IsGameOver)
        {
            GameOver();
        }
        //�����̴� �ڵ�
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //if(isGetItemAction)  // �˾Ƽ� �����
        //{
        //    return;
        //}

        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);


        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -45f, 45f);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        currentSpeed = isRunning ? moveRunSpeed : moveWalkSpeed;

        animator.SetBool("isRunning", isRunning);

        //if (Input.GetKeyDown(KeyCode.V))
        //{
        //    isFirstPerson = !isFirstPerson;
        //    Debug.Log(isFirstPerson ? "1��Ī���" : "3��Ī���");
        //}

        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isFirstPerson)
        {
            rotaterAroundPlayer = !rotaterAroundPlayer;
            Debug.Log(rotaterAroundPlayer ? "ī�޶� �÷��̾� ������ ȸ��" : "�÷��̾ ���� ȸ��");
        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
        }
        //else
        //{
        //    ThirdPersonMovement();
        //}

        //if (Input.GetMouseButtonDown(1) && !isAiming)
        //{
        //    isAiming = true;

        //    NoneCrossHair.SetActive(false);
        //    CrossHair.SetActive(true);

        //    AimWeapon();

        //    if (zoomCoroutine != null)
        //    {
        //        StopCoroutine(zoomCoroutine);
        //    }

        //    if (isFirstPerson)
        //    {
        //        SetTargetFOV(zoomedFov);
        //        zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
        //    }
        //    else
        //    {
        //        SetTargetDistance(zoomedDistance);
        //        zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
        //    }
        //}


        //if (Input.GetMouseButtonUp(1) && isAiming)
        //{
        //    if (itemGetPos == null)  // ���Ⱑ ���� ������ ��
        //    {
        //        ShotGunUI.SetActive(false);
        //        PistolUI.SetActive(false);
        //        isFiring = false;
        //        isAiming = false;
        //        Debug.Log("No weapon equipped.");  // ����� �޽��� �߰�
        //    }


        //    isAiming = false;
        //    animator.SetLayerWeight(1, 0);

        //    NoneCrossHair.SetActive(true);
        //    CrossHair.SetActive(false);
        //    if (zoomCoroutine != null)
        //    {
        //        StopCoroutine(zoomCoroutine);
        //    }

        //    if (isFirstPerson)
        //    {
        //        SetTargetFOV(zoomedFov);
        //        zoomCoroutine = StartCoroutine(ZoomFieldOfView(targetFov));
        //    }
        //    else
        //    {
        //        SetTargetDistance(zoomedDistance);
        //        zoomCoroutine = StartCoroutine(ZoomCamera(targetDistance));
        //    }
        //}
        if (Input.GetMouseButtonDown(1) && !isAiming)
        {
            if (itemGetPos == null)  // ���Ⱑ ���� ������ ��
            {
                ShotGunUI.SetActive(false);
                PistolUI.SetActive(false);
                isFiring = false;
                Debug.Log("No weapon equipped.");  // ����� �޽��� �߰�
                return;
            }

            isAiming = true;
            NoneCrossHair.SetActive(false);
            CrossHair.SetActive(true);
            AimWeapon();

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            if (isFirstPerson)
            {
                SetTargetFOV(zoomedFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(zoomedFov));
            }
            else
            {
                SetTargetDistance(zoomedDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(zoomedDistance));
            }
        }

        if (Input.GetMouseButtonUp(1) && isAiming)
        {
            isAiming = false;
            animator.SetLayerWeight(1, 0);
            NoneCrossHair.SetActive(true);
            CrossHair.SetActive(false);

            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }

            if (isFirstPerson)
            {
                SetTargetFOV(defaultFov);
                zoomCoroutine = StartCoroutine(ZoomFieldOfView(defaultFov));
            }
            else
            {
                SetTargetDistance(defaultDistance);
                zoomCoroutine = StartCoroutine(ZoomCamera(defaultDistance));
            }
        }


        if (Input.GetMouseButtonDown(0) && isAiming && isFire)
        {
            isFiring = true;
            FireWeapon();
        }
        else
        {
            isFiring = false;
        }

        ChanageWeapon();

        if (currenRecoil > 0)
        {
            currenRecoil -= recoilStrength * Time.deltaTime;

            currenRecoil = Mathf.Clamp(currenRecoil, 0, maxRecoliAngle);
        }

        //���� ��ġ�� ���� ��ġ�� ���̸� ����ϴ� �Լ�
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);

        bool isMoving = distanceMoved > minMoveDistance;//�̵� �߿� ���� ����


        if (isMoving)
        {
            bool leftHit = CheckGround(leftFoot);
            bool rightHit = CheckGround(rightFoot);

            if ((leftHit && !IsLeftFootGround))
            {
                //if (SoundManager.instance.sfxSource.isPlaying)
                    SoundManager.instance.PlaySFX("LeftFoot");
            }

            if ((rightHit && !IsLeftFootGround))
            {
                //if (SoundManager.instance.sfxSource.isPlaying)
                    SoundManager.instance.PlaySFX("RightFoot");
            }

            //���� ���¸� ���� �����Ӱ� ���ϱ� ���� ����
            IsLeftFootGround = leftHit;
            IsRightFootGround = rightHit;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isSlowMotion = !isSlowMotion;
        }

        //���� ��ġ�� ���� ��ġ�� ����(���� �����ӿ��� ���ϱ�����)
        previousPosition = transform.position;


        if (Input.GetKeyDown(KeyCode.E))
        {
            GetItem();

        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            
            ToggleFlashLight();
        }

        if (hp <= 0) // ���� �� 
        {
            ActivateRagdoll();
            Debug.Log("Player Die");
            GameOver();
            animator.SetTrigger("IsDie");
            SoundManager.instance.PlaySFX("PlayerDie");

        }
    }

    void ToggleFlashLight()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlaySFX("FlashLightOn");
        }

        // �÷��� ���¸� ���
        isFlashLightOn = !isFlashLightOn;
        if (flashLight != null)
        {
            flashLight.enabled = isFlashLightOn;
        }
        else
        {
            Debug.LogError("FlashLight�� �Ҵ���� �ʾҽ��ϴ�.");
        }
    }


    //void ToggleFlashLight()
    //{
    //    SoundManager.instance.PlaySFX("FlashLightOn");
    //    isFlashLightOn = !isFlashLightOn;
    //    flashLight.enabled = isFlashLightOn;

    //}
    private void LateUpdate()
    {
        if (isAiming)
        {
            if (upperBody != null)
            {
                upperBody.localRotation = Quaternion.Euler(upperBodyRotationAngle, 0, 0);
            }

        }
        else
        {
            if (upperBody != null)
            {
                upperBody.localRotation = originalUpperBodyRotation;
            }
        }
    }
    public void SetTargetDistance(float distance)
    {
        targetDistance = distance;
    }

    public void SetTargetFOV(float fov)
    {
        targetFov = fov;
    }

    void FirstPersonMovement() //1��Ī
    {

        Vector3 moveDirection = cameraTransfrom.forward * vertical + cameraTransfrom.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        cameraTransfrom.position = playerHead.transform.position;
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        transform.rotation = Quaternion.Euler(0f, cameraTransfrom.eulerAngles.y, 0);

        return;
    }

    //void ThirdPersonMovement() //3��Ī
    //{

    //    Vector3 move = transform.right * horizontal + transform.forward * vertical;
    //    characterController.Move(move * currentSpeed * Time.deltaTime);

    //    UpdateCameraPostion();
    //}

    void UpdateCameraPostion()
    {
        if (rotaterAroundPlayer)
        {
            Vector3 direction = new Vector3(0, 0, -currenDistance); //ī�޶� �Ÿ� ����
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransfrom.position = transform.position + thirdPersonOffset + rotation * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currenDistance);
            //cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);
            //ī�޶� �÷��̾��� �����ʿ��� ������ ��ġ�� �̵�
            cameraTransfrom.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;

            //ī�޶� �÷��̾��� ��ġ�� ���󰡵��� ����
            cameraTransfrom.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));

            UpdateAimTarget();
        }
    }

    void JumpAction()
    {
        
        isGround = CheckIfGrounded();

        // "JumpUp" �ִϸ��̼��� �Ϸ�Ǿ����� Ȯ�� (normalizedTime >= 1)
        bool jumpUpCompleted = animator.GetCurrentAnimatorStateInfo(0).IsName("JumpingUp") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;

        if (isGround && isJumping && jumpUpCompleted)
        {

            animator.SetTrigger("JumpDown");
            isJumping = false;

        }

        if (Input.GetButtonDown("Jump") && isGround)
        {
            SoundManager.instance.PlaySFX("Jump");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("JumpUp");
            isJumping = true;
        }

        velocity.y += gravity * Time.deltaTime;

        characterController.Move(velocity * Time.deltaTime);
    }


    bool CheckIfGrounded()
    {
        // ĳ���� �� �Ʒ��� �߽� ��ġ ���� (�ణ ���� �÷��� ����)
        Vector3 boxCenter = transform.position + Vector3.up * 0.1f;

        // BoxCast�� ũ�� ���� (ĳ������ �� ũ��� �°� ����)
        Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);

        // BoxCast �߻�
        RaycastHit hit;
        bool isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, 0.2f, groundLayer);

        // ������ BoxCast �ð�ȭ
        Color castColor = isGrounded ? Color.green : Color.red;
        Debug.DrawRay(boxCenter, Vector3.down * 0.2f, castColor);

        return isGrounded;
    }

    IEnumerator ZoomCamera(float targetDistance)
    {
        while (Mathf.Abs(currenDistance - targetDistance) > 0.01f)
        {
            currenDistance = Mathf.Lerp(currenDistance, targetDistance, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        currenDistance = targetDistance; //��ǥ �Ÿ��� ������ �� ���� ����
    }

    IEnumerator ZoomFieldOfView(float targetFov)
    {
        while (Mathf.Abs(mainCamera.fieldOfView - targetFov) > 0.01f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
            yield return null;
        }
        mainCamera.fieldOfView = targetFov;
    }

    bool CheckGround(Transform foot)
    {
        Vector3 rayStart = foot.position + Vector3.up * 0.05f;

        bool hit = Physics.Raycast(rayStart, Vector3.down, 0.1f);

        Debug.DrawRay(rayStart, Vector3.down * 0.1f, hit ? Color.green : Color.red);

        return hit;
    }

    void GameOver()
    {
        IsGameOver = true;

        Invoke("RestartGame", 2.0f);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        hp = Mathf.Max(hp, 0);
        HPUI.SetActive(true);
        
        Debug.Log("���� HP :"+ hp );
        
        
    }

    void FixedUpdate()
    {
        JumpAction(); // �߷� ó���� �̵��� ���ÿ� ����
    }

    void UpdateAimTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        aimTarget.position = ray.GetPoint(10);
    }

    void ToggleSlowMotion()
    {
        if (!isSlowMotion)
        {
            Time.timeScale = defaultTimeScale;
            Debug.Log("���ο� ��� ����");
        }
        else
        {
            Time.timeScale = slowMotionScale;
            //currentSpeed *= 2;
            Debug.Log("���ο� ��� Ȱ��ȭ");
        }
    }

    void AimWeapon()
    {
        animator.SetLayerWeight(1, 1);

        if (currentWeaponMode == WeaponMode.Pistol)
        {
            AimPistol();
            Debug.Log("AimePistol");
        }
        else if (currentWeaponMode == WeaponMode.ShotGun)
        {
            Debug.Log("AimShotGun");

            AimShotGun();
        }
        //else if (currentWeaponMode == WeaponMode.Rifle)
        //{
        //    AimRifle();
        //}
        //else if (currentWeaponMode == WeaponMode.SMG)
        //{
        //    AimSMG();
        //}
    }

    void FireWeapon()
    {
        
        StartCoroutine(FireWithDelay(fireDelay));

        //�Ʒ� ������� �ڵ带 �ڷ�ƾ�Լ��� �־��־� �ڷ�ƾ�Լ����� ����ǵ��� ����
        //if (currentWeaponMode == WeaponMode.Pistol)
        //{
        //    fireDelay = 0.5f;
        //    FirePistol();
        //}
        //else if (currentWeaponMode == WeaponMode.ShotGun)
        //{
        //    fireDelay = 1.0f;
        //    FireShotGun();
        //}
        //else if (currentWeaponMode == WeaponMode.Rifle)
        //{
        //    fireDelay = 1.2f;
        //    FireRifle();
        //}
        //else if (currentWeaponMode == WeaponMode.SMG)
        //{
        //    fireDelay = 0.1f;
        //    FireSMG();
        //}
        //ApplyRecoil();

    }

    void ApplyRecoil()
    {
        //���� ī�޶��� ���� ȸ���� ������
        Quaternion currenRotation = Camera.main.transform.rotation;

        // �ݵ��� ����Ͽ� X��(����) ȸ���� �߰�(���� �ö󰡴� �ݵ�)
        Quaternion recoilRotation = Quaternion.Euler(-currenRecoil, 0, 0);

        // ���� ȸ�� ���� �ݵ��� ���Ͽ� ���ο� ȸ������ ����
        Camera.main.transform.rotation = currenRotation * recoilRotation;

        //�ݵ� ���� ������Ŵ
        currenRecoil += recoilStrength;

        //�ݵ� ���� Max�� ���缭 ����
        currenRecoil = Mathf.Clamp(currenRecoil, 0, maxRecoliAngle);

        /*StartCoroutine(CameraShake(0.1f, 0.1f));*/ //��忡 ���� ����
    }


    void FirePistol()
    {
        PistolUI.SetActive(true);
        ShotGunUI.SetActive(false);
        animator.SetTrigger("FirePistol");
        SoundManager.instance.PlaySFX("FirePistol");


        Weapon currentWeapon = WeaponManager.instance.GetCurrentWeaponComponent();
        //if (ParticleManager.instance != null)
        //{
        //    ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.PistolEffect, currentWeapon.effectPos.position);
        //}
        //else
        //{
        //    Debug.LogError("ParticleManager �ν��Ͻ��� �����ϴ�.");
        //    return;
        //}

        Debug.Log("Fire Pistol �ִϸ��̼� ���� ��");

        RaycastHit hit;

        Vector3 origin = Camera.main.transform.position;  //  ī�޶󿡼� ��
        Vector3 direction = Camera.main.transform.forward;

        rayDistance = 150f; //�� �����Ÿ�

        Debug.DrawRay(origin, direction * rayDistance, Color.red, 1.5f);

        if (Physics.Raycast(origin, direction * rayDistance, out hit, hitLayer))
        {
            if (hit.collider != null)
            {
                if (hit.collider.tag == "Ground")
                {
                    ParticleManager.instance.PlayParticle(ParticleManager.ParticleType.BrickImpact, hit.point);
                    return;
                }
            }

            if (hit.collider.tag == "Zombie") 
            {
                ZombieAI zombieAI = hit.collider.GetComponent<ZombieAI>();
                if (zombieAI != null)
                {

                    zombieAI.TakeDamage(5, hit.collider.tag, hit.point); // hit.point �߰�

                }
            }
            else if(hit.collider.tag == "Head")
            {
                ZombieAI zombieAI = hit.collider.GetComponentInParent<ZombieAI>();
                if(zombieAI != null)
                {
                    zombieAI.TakeDamage(10, hit.collider.tag, hit.point); // hit.point �߰�

                }
            }
            Debug.Log("Hit :" + hit.collider.name);
        }
    }

    void FireShotGun()
    {
        //ShotGunUI.SetActive(true);
        //PistolUI.SetActive(false);
        //animator.SetTrigger("FireShotGun");
        //SoundManager.instance.PlaySFX("FireShotGun");

        //rayDistance = 100f;
        //for (int i = 1; i < shotGunRayCount; i++)
        //{
        //    RaycastHit hit;

        //    Vector3 origin = Camera.main.transform.position;  //  ī�޶󿡼� ��

        //    float spreadX = Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);
        //    float spreadY = Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);

        //    Vector3 spreadDirection = Quaternion.Euler(spreadX, spreadY, 0) * Camera.main.transform.forward;

        //    Debug.DrawRay(origin, spreadDirection * rayDistance, Color.red, 1.0f);

        //    if (Physics.Raycast(origin, spreadDirection * rayDistance, out hit, hitLayer))
        //    {
        //        Debug.Log("Hit :" + hit.collider.name);
        //    }

        //}
        ShotGunUI.SetActive(true);
        PistolUI.SetActive(false);
        animator.SetTrigger("FireShotGun");
        SoundManager.instance.PlaySFX("FireShotGun");

        rayDistance = 100f;
        for (int i = 1; i < shotGunRayCount; i++)
        {
            RaycastHit hit;

            Vector3 origin = Camera.main.transform.position;  //  ī�޶󿡼� ��

            float spreadX = Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);
            float spreadY = Random.Range(-shotGunSpreadAngle, shotGunSpreadAngle);

            Vector3 spreadDirection = Quaternion.Euler(spreadX, spreadY, 0) * Camera.main.transform.forward;

            Debug.DrawRay(origin, spreadDirection * rayDistance, Color.red, 1.0f);

            if (Physics.Raycast(origin, spreadDirection, out hit, rayDistance, hitLayer))
            {
                if (hit.collider.tag == "Zombie")
                {
                    ZombieAI zombieAI = hit.collider.GetComponent<ZombieAI>();
                    if (zombieAI != null)
                    {
                        zombieAI.TakeDamage(8, hit.collider.tag, hit.point); // ���� ������
                    }
                }
                else if (hit.collider.tag == "Head")
                {
                    ZombieAI zombieAI = hit.collider.GetComponentInParent<ZombieAI>();
                    if (zombieAI != null)
                    {
                        zombieAI.TakeDamage(15, hit.collider.tag, hit.point); // ��弦 ������
                    }
                }

                Debug.Log("Hit :" + hit.collider.name);
            }
        }
    }

    //void FireRifle()
    //{
    //    //GunUI.SetActive(true);
    //    animator.SetTrigger("FireRifle");
    //    SoundManager.instance.PlaySFX("FireRifle");
    //    Debug.Log("FireRifle �ִϸ��̼� ���� ��");

    //    RaycastHit hit;

    //    Vector3 origin = Camera.main.transform.position;  //  ī�޶󿡼� ��
    //    Vector2 direction = Camera.main.transform.forward;

    //    rayDistance = 1000f; //�� �����Ÿ�

    //    Debug.DrawRay(origin, direction * rayDistance, Color.red, 1.0f);

    //    if (Physics.Raycast(origin, direction * rayDistance, out hit, hitLayer))
    //    {
    //        Debug.Log("Hit :" + hit.collider.name);
    //    }
    //}

    //void FireSMG()
    //{
    //    //GunUI.SetActive(true);
    //    animator.SetTrigger("FireSMG");
    //    SoundManager.instance.PlaySFX("FireSMG");
    //    Debug.Log("FireSMG �ִϸ��̼� ���� ��");

    //    RaycastHit hit;

    //    Vector3 origin = Camera.main.transform.position;  //  ī�޶󿡼� ��
    //    Vector2 direction = Camera.main.transform.forward;

    //    rayDistance = 100f; //�� �����Ÿ�

    //    Debug.DrawRay(origin, direction * rayDistance, Color.red, 1.0f);

    //    if (Physics.Raycast(origin, direction * rayDistance, out hit, hitLayer))
    //    {
    //        Debug.Log("Hit :" + hit.collider.name);
    //    }
    //}

    void AimPistol()
    {
        ShotGunUI.SetActive(false);
        PistolUI.SetActive(true);
        //GunUI.SetActive(true);
        animator.Play("PistolAim");
        Debug.Log("PistolAim");
    }

    void AimShotGun()
    {
        ShotGunUI.SetActive(true);
        PistolUI.SetActive(false);
        //GunUI.SetActive(true);
        animator.Play("ShotGunAim");
        Debug.Log("ShotGunAim");
    }

    //void AimRifle()
    //{
    //    //GunUI.SetActive(true);
    //    animator.Play("RifleAim");
    //    Debug.Log("RifleAim");
    //}

    //void AimSMG()
    //{
    //    //GunUI.SetActive(true);
    //    animator.Play("SMGAim");
    //    Debug.Log("SMGAim");
    //}

    void ChanageWeapon()
    {
        
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShotGunUI.SetActive(false);
            PistolUI.SetActive(true);
            WeaponManager.instance.EquipWeapon(Weapon.WeaponType.Pistol);
            currentWeaponMode = WeaponMode.Pistol;
            Debug.Log("Pistol Chanage");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShotGunUI.SetActive(true);
            PistolUI.SetActive(false);
            WeaponManager.instance.EquipWeapon(Weapon.WeaponType.ShotGun);
            currentWeaponMode = WeaponMode.ShotGun;
            Debug.Log("ShotGun Chanage");
        }
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    WeaponManager.instance.EquipWeapon(Weapon.WeaponType.Rifle);
        //    currentWeaponMode = WeaponMode.Rifle;
        //    Debug.Log("Rifle Chanage");
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    WeaponManager.instance.EquipWeapon(Weapon.WeaponType.SMG);
        //    currentWeaponMode = WeaponMode.SMG;
        //    //Debug.Log("SMG Chanage");
        //}
    }

    void GetItem()
    {

        ShotGunUI.SetActive(false);
        PistolUI.SetActive(false);
        isGetItemAction = true;
        bool isPickUp = animator.GetCurrentAnimatorStateInfo(0).IsName("PickUp") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f;
        Debug.Log("GetItem");
        animator.SetTrigger("PickUp");
        Vector3 direction = itemGetPos.position;
        Vector3 origin = itemGetPos.forward;
        

        RaycastHit[] hits;

        hits = Physics.BoxCastAll(origin, boxSize / 2, direction, Quaternion.identity, castDistance, itemLayer);

        foreach (RaycastHit hit in hits)
        {
            GameObject item = hit.collider.gameObject;
            Debug.Log(hit.collider.name);

            if (item.CompareTag("Weapon"))
            {
                WeaponManager.instance.AddWeapon(item);
                item.SetActive(false);
            }
            else if (item.CompareTag("Item"))
            {

                Debug.Log($"�����۰���: {item.name}");
                item.SetActive(false);
            }
            else
            {
                return;
            }
            animator.SetTrigger("PickUp");
            SoundManager.instance.PlaySFX("Get");
            Debug.Log("�������� �������ϴ�.");
        }

        if (!isPickUp)
        {
            isGetItemAction = false;
            
        }

    }

    void DebugBoxCast(Vector3 origin, Vector3 direction)
    {
        Vector3 enPoint = origin + direction * castDistance;

        Vector3[] corners = new Vector3[8];
        corners[0] = origin + new Vector3(-boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[1] = origin + new Vector3(boxSize.x, -boxSize.y, -boxSize.z) / 2;
        corners[2] = origin + new Vector3(-boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[3] = origin + new Vector3(boxSize.x, boxSize.y, -boxSize.z) / 2;
        corners[4] = origin + new Vector3(-boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[5] = origin + new Vector3(boxSize.x, -boxSize.y, boxSize.z) / 2;
        corners[6] = origin + new Vector3(-boxSize.x, boxSize.y, boxSize.z) / 2;
        corners[7] = origin + new Vector3(boxSize.x, boxSize.y, boxSize.z) / 2;


        Debug.DrawLine(corners[0], corners[1], Color.green, debugDuration);
        Debug.DrawLine(corners[1], corners[3], Color.green, debugDuration);
        Debug.DrawLine(corners[3], corners[2], Color.green, debugDuration);
        Debug.DrawLine(corners[2], corners[0], Color.green, debugDuration);
        Debug.DrawLine(corners[4], corners[5], Color.green, debugDuration);
        Debug.DrawLine(corners[5], corners[7], Color.green, debugDuration);
        Debug.DrawLine(corners[7], corners[6], Color.green, debugDuration);
        Debug.DrawLine(corners[6], corners[4], Color.green, debugDuration);
        Debug.DrawLine(corners[0], corners[4], Color.green, debugDuration);
        Debug.DrawLine(corners[1], corners[5], Color.green, debugDuration);
        Debug.DrawLine(corners[2], corners[6], Color.green, debugDuration);
        Debug.DrawLine(corners[3], corners[7], Color.green, debugDuration);
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = new Vector3(0, originalPos.y + y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }


    public IEnumerator effectActive()
    {
        GameObject effectPos = GameObject.Find("EffectPos");
        if (effectPos != null)
        {
            effectPos.SetActive(true);
        }
        yield return new WaitForSeconds(1.0f);
        effectPos.SetActive(false);
    }

    //�ڷ�ƾ
    IEnumerator FireWithDelay(float fireDelay)
    {
        //GunUI.SetActive(true);
        isFire = false;

        if (currentWeaponMode == WeaponMode.Pistol)
        {
            ShotGunUI.SetActive(false);
            PistolUI.SetActive(true);
            fireDelay = 0.5f;
            FirePistol();
        }
        else if (currentWeaponMode == WeaponMode.ShotGun)
        {
            ShotGunUI.SetActive(true);
            PistolUI.SetActive(false);
            fireDelay = 1.0f;
            FireShotGun();
        }
        //else if (currentWeaponMode == WeaponMode.Rifle)
        //{
        //    fireDelay = 1.2f;
        //    FireRifle();
        //}
        //else if (currentWeaponMode == WeaponMode.SMG)
        //{
        //    fireDelay = 0.1f;
        //    FireSMG();
        //}
        ApplyRecoil();

        yield return new WaitForSeconds(fireDelay);

        isFire = true;

    }

    public void ActivateRagdoll()
    {
        animator.enabled = false;
    }

    private void SetRagdollStat(bool state)
    {
        foreach(Rigidbody body in ragdollbodies)
        {
            body.isKinematic = !state;
        }
        foreach(Collider collider in ragdollcolliders)
        {
            collider.enabled = state;
        }
    }

}





