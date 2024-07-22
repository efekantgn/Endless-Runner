using System;
using System.Collections;
using Enums;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(InputHandler))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float initialPlayerSpeed = 4f;
    [SerializeField] private float maxPlayerSpeed = 30f;
    [SerializeField] private float playerSpeedIncreaseRate = .1f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float initialGravityValue = -9.81f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask turnLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private AnimationClip slideAnimationClip;

    [SerializeField] private float playerSpeed;
    [SerializeField] private float scoreMultiplier = 10;
    private float gravity;
    private Vector3 movementDirection = Vector3.forward;
    private Vector3 playerVelocity;

    private InputHandler inputHandler;
    private CharacterController controller;
    private Animator animator;

    private int slidingAnimationID;
    private bool sliding;
    private bool turning;
    private float score = 0;

    [SerializeField] private UnityEvent<Vector3> turnEvent;
    [SerializeField] private UnityEvent<int> gameOverEvent;
    [SerializeField] private UnityEvent<int> scoreUpdateEvent;

    private void Awake()
    {
        inputHandler = GetComponent<InputHandler>();
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        slidingAnimationID = Animator.StringToHash("Slide");
    }
    private void OnEnable()
    {
        inputHandler.OnPlayerJumpPerformedAction += JumpPerformed;
        inputHandler.OnPlayerSlidePerformedAction += SlidePerformed;
        inputHandler.OnPlayerTurnPerformedAction += TurnPerformed;
        inputHandler.OnPlayerSwipeAction += Swipe;
    }


    private void OnDisable()
    {
        inputHandler.OnPlayerJumpPerformedAction -= JumpPerformed;
        inputHandler.OnPlayerSlidePerformedAction -= SlidePerformed;
        inputHandler.OnPlayerTurnPerformedAction -= TurnPerformed;
        inputHandler.OnPlayerSwipeAction -= Swipe;

    }
    private void Start()
    {
        playerSpeed = initialPlayerSpeed;
        gravity = initialGravityValue;
    }
    private void Update()
    {
        if (!IsGrounded(20f))
        {
            GameOver();
            return;
        }

        score += scoreMultiplier * Time.deltaTime;
        scoreUpdateEvent?.Invoke((int)score);

        controller.Move(transform.forward * playerSpeed * Time.deltaTime);
        ApplyGravity();

        if (playerSpeed < maxPlayerSpeed)
        {
            playerSpeed += Time.deltaTime * playerSpeedIncreaseRate;
            gravity = initialGravityValue - playerSpeed;

            if (animator.speed < 1.25f)
            {
                animator.speed += (1 / playerSpeed) * Time.deltaTime;
            }
        }
    }



    private void Swipe(Vector2 swipeDirection)
    {
        if (swipeDirection.y != 0) Turn(swipeDirection.y);
        else if (swipeDirection.x == 1) Jump();
        else if (swipeDirection.x == -1) SlideStart();

    }
    private void TurnPerformed(InputAction.CallbackContext context)
    {
        Turn(context.ReadValue<float>());
    }

    private void Turn(float turnValue)
    {
        Vector3? turnPosition = CheckTurn(turnValue);
        if (!turnPosition.HasValue)
        {
            GameOver();
            return;
        }

        Vector3 targetDirection = Quaternion.AngleAxis(90 * turnValue, Vector3.up) * movementDirection;
        turnEvent?.Invoke(targetDirection);
        Turn(turnValue, turnPosition.Value);
    }

    private void Turn(float turnValue, Vector3 turnPosition)
    {
        Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
        controller.enabled = false;
        transform.position = tempPlayerPosition;
        controller.enabled = true;

        Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
        transform.rotation = targetRotation;
        movementDirection = transform.forward.normalized;
    }

    private void SlidePerformed(InputAction.CallbackContext context)
    {
        SlideStart();
    }

    private void SlideStart()
    {
        if (IsGrounded() && !sliding)
        {
            StartCoroutine(Slide());
        }
    }

    private IEnumerator Slide()
    {
        Vector3 originalControllerCenter = controller.center;
        Vector3 newControllerCenter = originalControllerCenter;
        controller.height /= 2;
        newControllerCenter.y -= controller.height / 2;
        controller.center = newControllerCenter;

        sliding = true;
        animator.Play(slidingAnimationID);
        yield return new WaitForSeconds(slideAnimationClip.length / animator.speed);
        controller.height *= 2;
        controller.center = originalControllerCenter;

        sliding = false;

    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }

    private Vector3? CheckTurn(float turnValue)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
        if (hitColliders.Length != 0)
        {
            Tile tile = hitColliders[0].transform.parent.GetComponentInChildren<Tile>();
            TileType type = tile.type;
            if ((type == TileType.LEFT && turnValue == -1) ||
            (type == TileType.RIGHT && turnValue == 1) ||
            (type == TileType.SIDEWAYS))
            {
                return tile.pivot.position;
            }
        }
        return null;
    }
    private void ApplyGravity()
    {
        if (IsGrounded() && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private bool IsGrounded(float length = .2f)
    {
        Vector3 raycastOriginFirst = transform.position;
        raycastOriginFirst.y -= controller.height / 2f;
        raycastOriginFirst.y += .1f;

        Vector3 raycastOriginSecond = raycastOriginFirst;
        raycastOriginFirst -= transform.forward * .2f;
        raycastOriginSecond += transform.forward * .2f;

        Debug.DrawLine(raycastOriginFirst, raycastOriginFirst + Vector3.down * length, Color.green, .1f);
        Debug.DrawLine(raycastOriginSecond, raycastOriginSecond + Vector3.down * length, Color.red, .1f);

        if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) ||
        Physics.Raycast(raycastOriginSecond, Vector3.down, out RaycastHit hit2, length, groundLayer))
        {
            return true;

        }
        return false;
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        gameOverEvent?.Invoke((int)score);
        gameObject.SetActive(false);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
        {
            GameOver();
        }
    }
}