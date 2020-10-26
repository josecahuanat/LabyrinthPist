using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public const string customTag = "Player", WallTag = "Wall";
    public const string IdleAnimName = "Character_Idle", WalkAnimName = "Character_Walk";


    public float speed;

    public float raycastStartDistance, raycastLength;
    public Transform spriteRect;
    public bool canMove;

    Rigidbody2D rb2D;
    Animator animator;
    float lastZRotation;
    string CurrentAnimName => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    bool CanMove(Vector3 direction)
    {
        var hit = Physics2D.Raycast(transform.position + (direction * raycastStartDistance), direction, raycastLength);
        // Debug.DrawRay(transform.position + (direction * raycastStartDistance), direction * raycastLength);
        var collider = hit.collider;
        return !(collider != null && collider.CompareTag(WallTag));
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        //Movement
        Vector3 velocity = Vector3.zero;
        if (CanMove(-Vector3.right) && Input.GetKey(KeyCode.A))
            velocity.x = -(speed * Time.deltaTime);
        else if (CanMove(Vector3.right) && Input.GetKey(KeyCode.D))
            velocity.x = speed * Time.deltaTime;

        if (CanMove(Vector3.up) && Input.GetKey(KeyCode.W))
            velocity.y = speed * Time.deltaTime;
        else if (CanMove(-Vector3.up) && Input.GetKey(KeyCode.S))
            velocity.y = -(speed * Time.deltaTime);

        transform.position += velocity;


        //Sprite rotation
        if (velocity != Vector3.zero)
        {
            int zRotation = 0;

            if (Mathf.Abs(velocity.y) < 0.01f)
            {
                if (Input.GetKey(KeyCode.A))
                    zRotation = 180;
                else if (Input.GetKey(KeyCode.D))
                    zRotation = 0;
            }
            if (Mathf.Abs(velocity.x) < 0.01f)
            {
                if (Input.GetKey(KeyCode.W))
                    zRotation = 90;
                else if (Input.GetKey(KeyCode.S))
                    zRotation = 270;
            }
            else
            {
                if (velocity.x > 0f && velocity.y > 0f)
                    zRotation = 45;
                else if (velocity.x < 0f && velocity.y > 0f)
                    zRotation = 135;
                else if (velocity.x < 0f && velocity.y < 0f)
                    zRotation = 225;
                else if (velocity.x > 0f && velocity.y < 0f)
                    zRotation = 315;

            }

            zRotation -= 90;

            if (zRotation != lastZRotation)
            {
                lastZRotation = zRotation;
                spriteRect.DOKill();
                Vector3Int rotation = new Vector3Int(0, 0, zRotation);
                spriteRect.DORotate(rotation, .5f).SetEase(Ease.OutExpo);
            }

            if (CurrentAnimName != WalkAnimName)
                animator.SetTrigger(WalkAnimName);
        }
        else
        {
            if (CurrentAnimName != IdleAnimName)
                animator.SetTrigger(IdleAnimName);
        }
    }
}
