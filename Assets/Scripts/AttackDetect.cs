using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDetect : MonoBehaviour
{

    // used to get direction of facing
    public GameObject player;
    private PlayerMovement playerMovement;

    // used to get box collider size
    public Transform attackZone;
    private BoxCollider2D bc;

    public float cooldown = 0.5f;
    public float attackForce = 8f;

    private float timeOfLastUse;

    // used to apply force to "enemy"
    private Rigidbody2D rb;
    
    
    void Start() {
        timeOfLastUse = Time.time;
        bc = attackZone.GetComponent<BoxCollider2D>();
        playerMovement = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space) && (Time.time - timeOfLastUse >= cooldown))
        {
            timeOfLastUse = Time.time;

            Vector2 positionXY = new Vector2(attackZone.position.x, attackZone.position.y);
            Vector2 scaleXY = new Vector2(attackZone.lossyScale.x, attackZone.lossyScale.y);

            Collider2D[] colliders = Physics2D.OverlapAreaAll(positionXY - scaleXY.x * (playerMovement.getIsFacingRight() ? 1 : -1) * (bc.size / 2), positionXY + scaleXY.y * (bc.size / 2));

            for (int i = 0; i < colliders.Length; i++){

                if (colliders[i].gameObject.tag == "Enemy"){
                    rb = colliders[i].gameObject.GetComponent<Rigidbody2D>();

                    if (playerMovement.getIsFacingRight()){
                        //add force to right
                        rb.velocity = new Vector2(attackForce, rb.velocity.y);
                    } else {
                        //add force to left
                        rb.velocity = new Vector2(-attackForce, rb.velocity.y);
                    }
                }
            }

            
        }
    }


}
