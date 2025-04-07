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

    public float recoilStrength = 2f; //반동의 세기
    public float maxRecoliAngle = 10.0f; //반동의 최대 각도
    private float currenRecoil = 0f; //현재 반동 값을 저장하는 변수

    public LayerMask hitLayer;
    public float rayDistance = 100f;

    private int shotGunRayCount = 5; //샷건 총알이 퍼지는 수
    private float shotGunSpreadAngle = 10f;

    float moveWalkSpeed = 3.0f; //일반 속도
    float moveRunSpeed = 5.0f; //달리기 속도
    float currentSpeed = 1.0f; //변경속도
    bool isRunning = false;
    float gravity = -9.81f; //중력값
    Vector3 velocity; //현재 속도 저장
    CharacterController characterController;

    float mouseSensitivity = 100f; //마우스 감도
    public Transform cameraTransfrom; // 카메라 Transfrom
    public Transform playerHead; //플레이어 머리 위치(1인칭 모드일때 사용)
    public float thirdPersonDistance = 3.0f; // 3인칭 모드에서 플레이어와 카메라 시야 거리
    public Vector3 thirdPersonOffset = new Vector3(0, 1.5f, 0); // 3인칭 모드에서 카메라 오프셋
    public Transform playerLookObj; //플레이어의 시야 위치

    public float zoomedDistance = 1.0f; //카메라가 확대될 때의 거리(3인칭 모드일때사용)
    public float zoomSpeed = 5f; //확대 축소가 되는 속도
    public float defaultFov = 60f; //기본 카메라 시야각
    public float zoomedFov = 40f; // 확대 시 카메라 시야각
   private float defaultDistance = 10.0f;

    float currenDistance; //현재 카메라와의 거리
    float targetDistance; //목표 카메라 거리
    float targetFov; //목표 FOV
    bool isZoomed = false; //확대 여부
    private Coroutine zoomCoroutine; //코루틴을 사용하여 확대/축소
    Camera mainCamera; //카메라 컴포넌트

    float pitch = 0f; //위아래 회전값
    float yaw = 0f; //좌우 회전값
    bool isFirstPerson = false; //1인칭 모드여부
    bool rotaterAroundPlayer = true; //카메라가 플레이어 주위를 회전하는 여부
    public float jumpHeight = 2f; //점프 높이
    bool isGround; //땅에 충돌 여부
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

    bool isJumping = false;  // 캐릭터가 점프 중인지 추적

    public float hp = 100;

    bool isAiming = false;
    bool isFiring = false;

    public Transform upperBody; //상체 본을 할당(Spine, UpperChest)
    public float upperBodyRotationAngle = -30f; //상체 Aim 모드에서만 회전을 한다.
    private Quaternion originalUpperBodyRotation; //원래 상체 회전 값

    public Transform aimTarget;

    public float slowMotionScale = 0.5f;
    private float defaultTimeScale = 1.0f;
    private bool isSlowMotion = false;

    //아이템 변수
    public Vector3 boxSize = new Vector3(1, 1, 1);
    public float castDistance = 5f; //BoxCast 멀리 감지 거리 
    public LayerMask itemLayer; //아이템레이어 
    public Transform itemGetPos; // BoxCast 위치 
    public float debugDuration = 2.0f; // 디버그 라인 여부 

    bool isGetItemAction = false;


    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.1f;

    private Vector3 originalPos;

    public GameObject NoneCrossHair;
    public GameObject CrossHair;

    private bool isFire = true; // 발사여부  -> false일때면 false
    public float fireDelay = 0.1f; // 전역변수로 놓고 수치를 변화해서 게임 내의 변화를 준다.

    //public float pistolFireDelay = 0.7f;  // 권총 발사 딜레이
    //public float shotGunDelay = 1.0f;  // 샷건 딜레이
    //public float rifleDelay = 1.2f; //라이플 딜레이 
    //public float SMGFireDelay = 0.1f; //기관단총 딜레이

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
        currenDistance = thirdPersonDistance; //초기 카메라 거리를 설정
        targetDistance = thirdPersonDistance; //목표 카메라 거리 설정
        targetFov = defaultFov; //초기 FOV 설정
        mainCamera = cameraTransfrom.GetComponent<Camera>();
        mainCamera.fieldOfView = defaultFov; //기본 fov 설정
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        previousPosition = transform.position; //시작시 현재 위치를 이전 위치로 초기화

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
        //움직이는 코드
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //if(isGetItemAction)  // 알아서 만들기
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
        //    Debug.Log(isFirstPerson ? "1인칭모드" : "3인칭모드");
        //}

        if (Input.GetKeyDown(KeyCode.LeftAlt) && !isFirstPerson)
        {
            rotaterAroundPlayer = !rotaterAroundPlayer;
            Debug.Log(rotaterAroundPlayer ? "카메라가 플레이어 주위를 회전" : "플레이어가 직접 회전");
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
        //    if (itemGetPos == null)  // 무기가 없는 상태일 때
        //    {
        //        ShotGunUI.SetActive(false);
        //        PistolUI.SetActive(false);
        //        isFiring = false;
        //        isAiming = false;
        //        Debug.Log("No weapon equipped.");  // 디버그 메시지 추가
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
            if (itemGetPos == null)  // 무기가 없는 상태일 때
            {
                ShotGunUI.SetActive(false);
                PistolUI.SetActive(false);
                isFiring = false;
                Debug.Log("No weapon equipped.");  // 디버그 메시지 추가
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

        //현재 위치와 이전 위치의 차이를 계산하는 함수
        float distanceMoved = Vector3.Distance(transform.position, previousPosition);

        bool isMoving = distanceMoved > minMoveDistance;//이동 중에 대한 여부


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

            //현재 상태를 다음 프레임과 비교하기 위해 저장
            IsLeftFootGround = leftHit;
            IsRightFootGround = rightHit;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isSlowMotion = !isSlowMotion;
        }

        //현재 위치를 이전 위치로 저장(다음 프레임에서 비교하기위함)
        previousPosition = transform.position;


        if (Input.GetKeyDown(KeyCode.E))
        {
            GetItem();

        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            
            ToggleFlashLight();
        }

        if (hp <= 0) // 죽을 때 
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

        // 플래시 상태를 토글
        isFlashLightOn = !isFlashLightOn;
        if (flashLight != null)
        {
            flashLight.enabled = isFlashLightOn;
        }
        else
        {
            Debug.LogError("FlashLight가 할당되지 않았습니다.");
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

    void FirstPersonMovement() //1인칭
    {

        Vector3 moveDirection = cameraTransfrom.forward * vertical + cameraTransfrom.right * horizontal;
        moveDirection.y = 0;
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        cameraTransfrom.position = playerHead.transform.position;
        cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);

        transform.rotation = Quaternion.Euler(0f, cameraTransfrom.eulerAngles.y, 0);

        return;
    }

    //void ThirdPersonMovement() //3인칭
    //{

    //    Vector3 move = transform.right * horizontal + transform.forward * vertical;
    //    characterController.Move(move * currentSpeed * Time.deltaTime);

    //    UpdateCameraPostion();
    //}

    void UpdateCameraPostion()
    {
        if (rotaterAroundPlayer)
        {
            Vector3 direction = new Vector3(0, 0, -currenDistance); //카메라 거리 설정
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransfrom.position = transform.position + thirdPersonOffset + rotation * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransfrom.LookAt(transform.position + new Vector3(0, thirdPersonOffset.y, 0));
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            Vector3 direction = new Vector3(0, 0, -currenDistance);
            //cameraTransfrom.rotation = Quaternion.Euler(pitch, yaw, 0);
            //카메라를 플레이어의 오른쪽에서 고정된 위치로 이동
            cameraTransfrom.position = playerLookObj.position + thirdPersonOffset + Quaternion.Euler(pitch, yaw, 0) * direction;

            //카메라가 플레이어의 위치를 따라가도록 설정
            cameraTransfrom.LookAt(playerLookObj.position + new Vector3(0, thirdPersonOffset.y, 0));

            UpdateAimTarget();
        }
    }

    void JumpAction()
    {
        
        isGround = CheckIfGrounded();

        // "JumpUp" 애니메이션이 완료되었는지 확인 (normalizedTime >= 1)
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
        // 캐릭터 발 아래의 중심 위치 설정 (약간 위로 올려서 시작)
        Vector3 boxCenter = transform.position + Vector3.up * 0.1f;

        // BoxCast의 크기 설정 (캐릭터의 발 크기와 맞게 조정)
        Vector3 boxSize = new Vector3(0.5f, 0.1f, 0.5f);

        // BoxCast 발사
        RaycastHit hit;
        bool isGrounded = Physics.BoxCast(boxCenter, boxSize / 2, Vector3.down, out hit, Quaternion.identity, 0.2f, groundLayer);

        // 디버깅용 BoxCast 시각화
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
        currenDistance = targetDistance; //목표 거리에 도달한 후 값을 고정
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
        
        Debug.Log("현재 HP :"+ hp );
        
        
    }

    void FixedUpdate()
    {
        JumpAction(); // 중력 처리와 이동을 동시에 수행
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
            Debug.Log("슬로우 모션 해제");
        }
        else
        {
            Time.timeScale = slowMotionScale;
            //currentSpeed *= 2;
            Debug.Log("슬로우 모션 활성화");
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

        //아래 만들어진 코드를 코루틴함수에 넣어주어 코루틴함수에서 실행되도록 변경
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
        //현재 카메라의 월드 회전을 가져옴
        Quaternion currenRotation = Camera.main.transform.rotation;

        // 반동을 계산하여 X축(상하) 회전에 추가(위로 올라가는 반동)
        Quaternion recoilRotation = Quaternion.Euler(-currenRecoil, 0, 0);

        // 현재 회전 값에 반동을 곱하여 새로운 회전값을 적용
        Camera.main.transform.rotation = currenRotation * recoilRotation;

        //반동 값을 증가시킴
        currenRecoil += recoilStrength;

        //반동 값을 Max에 맞춰서 제한
        currenRecoil = Mathf.Clamp(currenRecoil, 0, maxRecoliAngle);

        /*StartCoroutine(CameraShake(0.1f, 0.1f));*/ //모드에 따라 변경
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
        //    Debug.LogError("ParticleManager 인스턴스가 없습니다.");
        //    return;
        //}

        Debug.Log("Fire Pistol 애니메이션 실행 중");

        RaycastHit hit;

        Vector3 origin = Camera.main.transform.position;  //  카메라에서 쏨
        Vector3 direction = Camera.main.transform.forward;

        rayDistance = 150f; //총 사정거리

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

                    zombieAI.TakeDamage(5, hit.collider.tag, hit.point); // hit.point 추가

                }
            }
            else if(hit.collider.tag == "Head")
            {
                ZombieAI zombieAI = hit.collider.GetComponentInParent<ZombieAI>();
                if(zombieAI != null)
                {
                    zombieAI.TakeDamage(10, hit.collider.tag, hit.point); // hit.point 추가

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

        //    Vector3 origin = Camera.main.transform.position;  //  카메라에서 쏨

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

            Vector3 origin = Camera.main.transform.position;  //  카메라에서 쏨

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
                        zombieAI.TakeDamage(8, hit.collider.tag, hit.point); // 샷건 데미지
                    }
                }
                else if (hit.collider.tag == "Head")
                {
                    ZombieAI zombieAI = hit.collider.GetComponentInParent<ZombieAI>();
                    if (zombieAI != null)
                    {
                        zombieAI.TakeDamage(15, hit.collider.tag, hit.point); // 헤드샷 데미지
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
    //    Debug.Log("FireRifle 애니메이션 실행 중");

    //    RaycastHit hit;

    //    Vector3 origin = Camera.main.transform.position;  //  카메라에서 쏨
    //    Vector2 direction = Camera.main.transform.forward;

    //    rayDistance = 1000f; //총 사정거리

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
    //    Debug.Log("FireSMG 애니메이션 실행 중");

    //    RaycastHit hit;

    //    Vector3 origin = Camera.main.transform.position;  //  카메라에서 쏨
    //    Vector2 direction = Camera.main.transform.forward;

    //    rayDistance = 100f; //총 사정거리

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

                Debug.Log($"아이템감지: {item.name}");
                item.SetActive(false);
            }
            else
            {
                return;
            }
            animator.SetTrigger("PickUp");
            SoundManager.instance.PlaySFX("Get");
            Debug.Log("아이템을 집었습니다.");
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

    //코루틴
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





