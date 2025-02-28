using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Random=UnityEngine.Random;

public class FirstPersonController : MonoBehaviour
{
    public bool canMove = true;
    public bool isHiding = false;
    public bool isCrouching;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey) && characterController.isGrounded;
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && (coyoteDuration>0) && (!isJumping);
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchingAnimation && characterController.isGrounded;

    [Header("Functional options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private bool willSlide = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float slopeSpeed = 8.0f;
    
    [Header("Look parameters")]
    [SerializeField, Range(1,10)] private float lookSpeedX =2.0f;
    [SerializeField, Range(1,10)] private float lookSpeedY =2.0f;
    [SerializeField, Range(1,180)] private float upperLookLimit =80.0f;
    [SerializeField, Range(1,180)] private float lowerLookLimit =80.0f;
    public bool lockLook = false;

    [Header("Jumping parameters")]
    [SerializeField] private float jumpForce =8.0f;
    [SerializeField] private float gravity = 30.0f;
    [SerializeField] private float coyoteDuration = 0.5f;

    [SerializeField] private bool isJumping = false;


     [Header("Crouch parameters")]
     [SerializeField] private float crouchHeight =0.5f;
     [SerializeField] private float standingHeight =2f;
     [SerializeField] private float timeToCrouch =0.25f;
     [SerializeField] private Vector3 crouchingCenter = new Vector3(0,0.5f,0);
     [SerializeField] private Vector3 standingCenter = new Vector3(0,0,0);
     private bool duringCrouchingAnimation;

     [Header("Headbob parameters")]
     [SerializeField] private float walkBobSpeed =14f;
     [SerializeField] private float walkBobAmount =0.05f;
     [SerializeField] private float sprintBobSpeed =18f;
     [SerializeField] private float sprintBobAmount =0.1f;
     [SerializeField] private float crouchBobSpeed =8f;
     [SerializeField] private float crouchBobAmount =0.025f;
     private float defaultYPos = 0;
     private float timer;

     //SLIDING PARAMETERS

     private Vector3 hitPointNormal;
     public LayerMask slideMask;
     private bool IsSliding
     {
        get
        {
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f, slideMask))
            {
                hitPointNormal=slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up)> characterController.slopeLimit;
                
            }
            else
            {
                return false;
            }
        }
     }

     [Header("Footsteps parameters")]
     [SerializeField] private float baseStepSpeed = 0.5f;
     [SerializeField] private float crouchStepMultiplier = 1.5f;
     [SerializeField] private float sprintStepMultiplier = 0.6f;
     [SerializeField] private AudioSource footstepsAudioSource = default;
     [SerializeField] private AudioClip[] groundClip = default;
     private float footstepsTimer = 0;
     private float GetCurrentOffset => isCrouching? baseStepSpeed * crouchStepMultiplier : IsSprinting? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;


     [Header("Stamina parameters")]
     public AudioClip playerPanting;
     [SerializeField] private float maxStamina = 100;
     [SerializeField] private float staminaUseMultiplier = 5;
     [SerializeField] private float timeBeforeStaminaRegenStarts = 3;
     [SerializeField] private float staminaValueIncrement = 2;
     [SerializeField] private float staminaTimeIncrement = 0.1f;
     private float currentStamina;
     private Coroutine regeneratingStamina;
     public static Action<float> OnStaminaChange;

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX=0;
    
    void Awake()
    {
        Time.timeScale=1f;
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y;
        currentStamina=maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    
    void Update()
    {
        if(canMove)
        {
            HandleMovementInput();
            if(lockLook==false)
                HandleMouseLook();
            if(canJump)
                HandleJump();
                HandleCoyote();
            if(canCrouch)
                HandleCrouch();
            if(canUseHeadbob)
                HandleHeadbob();
            if(useStamina)
                HandleStamina();
            ApplyFinalMovements();
            if(useFootsteps)
                HandleFootsteps();
        }
        if(!characterController.isGrounded && Input.GetKeyDown(jumpKey))
        {
            isJumping=true;
        }
        else if(characterController.isGrounded)
        {
            isJumping=false;
        }
    }

    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed)*Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed)*Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward)*currentInput.x) + (transform.TransformDirection(Vector3.right)*currentInput.y);
        moveDirection.y = moveDirectionY;
    }
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX,0,0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X")* lookSpeedX,0);
    }
    private void HandleJump()
    {
        if(ShouldJump)
            moveDirection.y = jumpForce;

    }
    private void HandleCoyote()
    {
        if(!characterController.isGrounded)
        {
            coyoteDuration -= Time.deltaTime;
        }
        else
        {
            coyoteDuration = 0.5f;
        }
    }
    private void HandleCrouch()
    {
        if(ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void HandleHeadbob()
    {
        if(!characterController.isGrounded) return;

        if(Mathf.Abs(moveDirection.x)>0.1f || Mathf.Abs(moveDirection.z)>0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    private void HandleFootsteps()
    {
        if(!characterController.isGrounded) return;
        if(currentInput == Vector2.zero) return;

        footstepsTimer -= Time.deltaTime;

        if(footstepsTimer <= 0)
        {
            if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    case "Footsteps/GROUND":
                        footstepsAudioSource.PlayOneShot(groundClip[Random.Range(0, groundClip.Length -1)]);
                    break;
                }
            }
            footstepsTimer = GetCurrentOffset;
        }
    }
    private void HandleStamina()
    {
        if(IsSprinting && currentInput!=Vector2.zero)
        {
            if(regeneratingStamina!=null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina=null;
            }

            currentStamina-= staminaUseMultiplier*Time.deltaTime;
            if(currentStamina <0)
                currentStamina=0;
            OnStaminaChange?.Invoke(currentStamina);
            if(currentStamina <=0)
                canSprint=false;
        }

        //if(canSprint==false)
        //    footstepsAudioSource.PlayOneShot(playerPanting);

        if(!IsSprinting && currentStamina<maxStamina && regeneratingStamina == null)
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }
    private void ApplyFinalMovements()
    {
        if(!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        
        if(willSlide&&IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) *slopeSpeed;
            walkSpeed = walkSpeed=14f;
        } else {walkSpeed=6f;}

        characterController.Move(moveDirection*Time.deltaTime);
        if(characterController.velocity.y < -1 && characterController.isGrounded)
        {
            moveDirection.y = 0;
        }
    }

    private IEnumerator CrouchStand()
    {
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up,3f))
            yield break;
        duringCrouchingAnimation = true;
        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight,targetHeight,timeElapsed/timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter,targetCenter,timeElapsed/timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchingAnimation = false;
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while(currentStamina<maxStamina)
        {
            if(currentStamina>0)
                canSprint=true;
            
            currentStamina+=staminaValueIncrement;

            if(currentStamina>maxStamina)
                currentStamina=maxStamina;

            OnStaminaChange?.Invoke(currentStamina);

            yield return timeToWait;

            
        }
        regeneratingStamina = null;
    }
}
