using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public struct Section
    {
        public Image[] melodyUI;
        public List<AudioClip> melody;
        public List<SpriteRenderer> doorToHide;
        public List<Transform> doorToMove;
        public float doorToMoveMoveXPos;
        public List<Floor> melodyFloors;
    }

    public static GameManager instance;


    const string Dialogue1 = "Leandro, aplica tus conocimientos de música antigua, tú eres un profesor espectacular.",
        Dialogue2 = "Recuerda nuestra tradición,los sonidos de los espíritus son parecidas a las notas musicales antiguas.",
        Dialogue3 = "No olvides lo que Christian te dijo en el bosque cuando se conocieron: \"El laberinto está hecho de oro y hay espíritus cuidandolo\".",
        Dialogue4 = "Si eres una persona de buen corazón, te mandaremos señales para que puedas salir del laberinto,de lo contrario te quedarás aquí para siempre",
        Dialogue5 = "Sabemos que desde que nos perdiste en el incendio ocurrido en la última reunión familiar, has estado deprimido,quiero que sepas que te amamos hijo.",
        Dialogue6 = "Que no se te olvide mantener la tradición viva y dejar tu legado para futuras generaciones.",
        Ending = "Haz pasado por tanto y finalmente llegaste aquí. Sabemos que eress un hombre de bien por eso decidimos ayudarte. También sabemos la razón por la cual estás aquí, así que te cumpliré ese deseo que tanto anhelas: Recuperar a tu familia. La única condición es que no debes decirle a nadie sobre cómo lograste salir del Laberynthpist para conseguir el deseo si no lo cumples, morirás como castigo a tu falta.";

    string[] dialogues = { Dialogue1, Dialogue2, Dialogue3, Dialogue4, Dialogue5, Dialogue6 };

    public Player player;
    public Section[] sections;
    public AudioSource sfxAudiosource, bgmAudioSource;
    public AudioClip wrongMelodyNoteClip, memoryClip, doorClip;
    public AudioClip bgmTittleClip, bgmGameplayClip, bgmEndingClip;

    public Image titleFade, fade;
    public Text endingText, restartText;
    public Color fadeLoopColor;
    public AnimationCurve startFadeAnimCurve;
    public Text dialogueText;
    public CanvasGroup titleScreenContainer;

    AudioSource audioSource;
    int sectionIndex, melodyIndex;
    Section CurrentSection => sections[sectionIndex];
    bool playedInitialMelodyNotes;
    int currentDialogue;
    bool canRestart;

    void Awake()
    {
        if (instance == null)
            instance = this;

        audioSource = GetComponent<AudioSource>();
        sectionIndex = melodyIndex = 0;

        // sectionIndex = 1;
    }

    void Start()
    {
        titleFade.DOColor(fadeLoopColor, 3f).SetEase(startFadeAnimCurve).SetLoops(-1);
    }

    void Update()
    {
        if (canRestart && Input.anyKeyDown)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void PlayMelody(AudioClip clip)
    {
        int clipIndex = CurrentSection.melody.IndexOf(clip);

        // Debug.Log($"{clipIndex} == {melodyIndex}");
        if (clipIndex == melodyIndex)
        {
            audioSource.PlayOneShot(clip);

            if (melodyIndex == CurrentSection.melody.Count - 1)
            {
                if (!playedInitialMelodyNotes)
                {
                    CurrentSection.melodyUI[melodyIndex].DOColor(Color.cyan, 2f);
                    melodyIndex = 0;
                    playedInitialMelodyNotes = true;
                }
                else
                {
                    sfxAudiosource.PlayOneShot(doorClip);

                    foreach(var melodyFloor in CurrentSection.melodyFloors)
                    {
                        melodyFloor.Deactivate();
                    }

                    foreach(var spriteRenderer in CurrentSection.doorToHide)
                    {
                        spriteRenderer.DOColor(Color.clear, 1f);
                    }

                    foreach(var doorToMove in CurrentSection.doorToMove)
                    {
                        doorToMove.DOLocalMoveX(CurrentSection.doorToMoveMoveXPos, 2f);
                    }

                    for (int i = 0; i < CurrentSection.melody.Count ; i++)
                    {
                        melodyIndex = i;
                        var melodyUI = CurrentSection.melodyUI[i];
                        melodyUI.DOColor(Color.clear, 1f).OnComplete(() => melodyUI.gameObject.SetActive(false));
                    }

                    melodyIndex = 0;
                    sectionIndex++;
                    playedInitialMelodyNotes = false;

                    DOVirtual.DelayedCall(1f, () =>
                    {
                        for (int i = 0; i < CurrentSection.melody.Count ; i++)
                            CurrentSection.melodyUI[i].gameObject.SetActive(true);
                    });
                }
            }
            else
            {
                CurrentSection.melodyUI[melodyIndex].DOKill();
                if (playedInitialMelodyNotes)
                {
                    CurrentSection.melodyUI[melodyIndex].color = Color.cyan;
                    CurrentSection.melodyUI[melodyIndex].DOColor(Color.white, .5f);
                    melodyIndex++;
                }
                else
                {
                    //CurrentSection.melodyUI[melodyIndex].color = Color.white;
                    CurrentSection.melodyUI[melodyIndex].DOColor(Color.cyan, 2f);
                    melodyIndex++;
                }
            }
        }
        else
        {
            sfxAudiosource.PlayOneShot(wrongMelodyNoteClip);

            for (int i = 0; i < melodyIndex ; i++)
            {
                CurrentSection.melodyUI[i].DOColor(Color.cyan, 1f);
            }

            CurrentSection.melodyUI[melodyIndex].DOKill();
            CurrentSection.melodyUI[melodyIndex].color = Color.red;
            CurrentSection.melodyUI[melodyIndex].DOColor(Color.cyan, 1f);

            melodyIndex = 0;
        }
    }

    public void ShowDialogue()
    {
        dialogueText.DOKill();

        sfxAudiosource.PlayOneShot(memoryClip);

        dialogueText.DOColor(Color.clear, 1f).OnComplete(() =>
        {
            dialogueText.text = string.Empty;
            dialogueText.color = Color.white;
            dialogueText.DOText(dialogues[currentDialogue], 4f);
            dialogueText.DOColor(Color.clear, 1f).SetDelay(6f);
            currentDialogue++;
        });

    }

    public void ShowEnding()
    {
        player.canMove = false;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = bgmEndingClip;
        bgmAudioSource.Play();

        fade.DOColor(Color.black, 1f);
        endingText.DOText(Ending, 20f).OnComplete(() =>
        {
            restartText.DOColor(Color.white, 1f);
            canRestart = true;
        });
    }

    public void StartGameplay()
    {
        player.canMove = true;
        bgmAudioSource.Stop();
        bgmAudioSource.clip = bgmGameplayClip;
        bgmAudioSource.Play();

        titleScreenContainer.DOFade(0f, 1f).OnComplete(() =>
        {
            titleScreenContainer.blocksRaycasts = false;
            titleScreenContainer.interactable = false;
        });

        titleFade.DOKill();
        titleFade.DOColor(Color.clear, 1f);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
