using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Floor : MonoBehaviour
{
    public AudioClip clip;
    public bool isInitialMelody, isLastFloor, isDialogue;
    public Color customColor;
    public AnimationCurve steppedOn;


    SpriteRenderer spriteRenderer;
    BoxCollider2D boxCollider2D;
    bool keepPlaying;

    void Start()
    {
        keepPlaying = true;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();


        if (clip != null)
        {
            spriteRenderer.sortingOrder = 30;
            spriteRenderer.color = customColor;
        }
    }

    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(Player.customTag))
        {
            if ((clip != null && keepPlaying) || isDialogue || isLastFloor)
            {
                if (isLastFloor)
                {
                    GameManager.instance.ShowEnding();
                    return;
                }

                if (isDialogue)
                {
                    GameManager.instance.ShowDialogue();
                    boxCollider2D.enabled = false;
                    transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBack);
                    return;
                }

                if (isInitialMelody)
                {
                    Deactivate();
                }
                else
                {
                    spriteRenderer.DOKill();
                    spriteRenderer.color = Color.black;
                    spriteRenderer.DOColor(Color.white, .3f).SetEase(steppedOn);
                }
                GameManager.instance.PlayMelody(clip);
            }
        }
    }

    public void Deactivate()
    {
        keepPlaying = false;
        spriteRenderer.DOKill();
        transform.DOScale(Vector3.one * 3f, 1f);
        spriteRenderer.DOColor(Color.clear, 1f);
    }
}
