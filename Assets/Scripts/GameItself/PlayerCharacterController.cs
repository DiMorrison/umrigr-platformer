using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterController : NetworkBehaviour
{
    // Start is called before the first frame update

    
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 10f;
    [SerializeField] private GameObject jumpUICanvasPrefab; // prefab canvasa sa indikatorima preostalih jumpova
    private GameObject jumpUICanvas;    // objekt koji drzi stvoreni canvas
    [SerializeField] private float maxDistance = 0.5f;
    private Rigidbody2D rb;
    private bool isGrounded;
    public LayerMask platforms;

    private Animator animator;

    public float groundedBuffer = 0.1f;
    // private float groundedTimer = 1f;

    [Networked] TickTimer timer { get; set; }

    [SerializeField] private int maxJumps = 3;


    private int jumpCount;
    private bool jumpPressed;


    private float defaultGravityScale = 1f;

    private bool Active = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
        animator.SetBool("isIdle", true);

        DisablePlayerControls();
        jumpUICanvas = Instantiate(jumpUICanvasPrefab);     // stvori canvas svakom igracu
    }
   

    public override void Spawned() {
        if (Runner.IsServer) InitServer();
    }

    private void InitServer() {
        //UnityEngine.Debug.Log($"{NetworkPlayerSpawner.Instance.getSpawnLocation().x} {NetworkPlayerSpawner.Instance.getSpawnLocation().y}");
        //rb.position = NetworkPlayerSpawner.Instance.getSpawnLocation();
    }


    public void DisablePlayerControls()
    {
        Active = false;
        defaultGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        UnityEngine.Debug.Log($"Gravity stored as  {defaultGravityScale}");
    }

    public void EnablePlayerControls()
    {
        Active = true;
        animator.SetBool("isIdle", true);
        rb.gravityScale = defaultGravityScale;
    }


    public override void FixedUpdateNetwork()
    {

        if (Active && GetInput(out NetorkData input)){
       
        
            // Horizontal movement
            float moveX = input.Movement.x;
            rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

            // UnityEngine.Debug.Log(moveX); NE RADI? mozda sync neki
            if (moveX < 0)
            {
                rb.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
            else if (moveX > 0)
            {
                rb.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            // Debounce jump input
            if (input.Jump && !jumpPressed && jumpCount < maxJumps)
            {

                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpPressed = true;
                jumpCount++;
                UnityEngine.Debug.Log("Jumped");
                UpdateJumps();  // nakon svakog skoka azuriraj jump cavnas

                ResetAnimator();
                animator.SetBool("isJumping", true);
            }

            if (!input.Jump)
            {
                jumpPressed = false;
            }


            if (timer.IsRunning)
            {
                if (!input.Jump){
                    if (moveX != 0)
                    {
                        ResetAnimator();
                        animator.SetBool("isRunning", true);
                    }
                }
                // groundedTimer += Time.deltaTime;
            }
            else
            {
                ResetAnimator();
                animator.SetBool("isFalling", true);
            }
        }
    }

private void ResetAnimator()
{
    animator.SetBool("isIdle", false);
    animator.SetBool("isRunning", false);
    animator.SetBool("isJumping", false);
    // animator.ResetTrigger(Hurt);
    // animator.ResetTrigger(Punch);
    animator.SetBool("isDead", false);
    animator.SetBool("isFalling", false);
}

private void UpdateJumps()
{
    // dohvati holder 3 jump indikator gameobjekta
    GameObject jumpCirclesHolder = jumpUICanvas.transform.GetChild(0).GetChild(0).gameObject;

    for (int i = 0; i < maxJumps; i++)
    {
        // Dodi do indikatora kruga
        var jumpCircle = jumpCirclesHolder.transform.GetChild(i).transform.GetChild(0).GetComponent<Image>();

        if (i < jumpCount)  // postavi boju
        {
            jumpCircle.color = Color.grey;
        }
        else
        {
            jumpCircle.color = Color.yellow;
        }
    }
}


    private bool checkGrounded() {
        
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, maxDistance, platforms);
        UnityEngine.Debug.DrawRay(rb.position, Vector2.down * maxDistance, Color.red);

        if (hit.collider != null) {
            UnityEngine.Debug.Log("Hit object: " + hit.collider.gameObject.name + " at distance: " + hit.distance);

            return true;
        }

        return false;
    }



    

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.1f)
        {
            isGrounded = true;
            jumpCount = 0;
            UpdateJumps();
            timer = TickTimer.CreateFromSeconds(Runner, groundedBuffer);
        }
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }

    public void HandleDeath()
    {
        if (Object.HasStateAuthority)
        {
            UnityEngine.Debug.Log($"Player {Object.InputAuthority.PlayerId} has died!");
            gameObject.SetActive(false);
        }
    }
}
