using UnityEngine;
using UnityEngine.Events;

public class TurnTimer
{
    private const float RESET_EMPTY_TIMER = -1f;

    private float turnDuration;

    private UnityAction timerDone;

    private bool tenSecondSoundPlayed;

    private bool startTurnSoundPlayed;

    private AudioClip tenSecondSound;

    private AudioClip startTurnSound;

    public float Timer
    {
        get;
        private set;
    }

    public bool Paused
    {
        get;
        private set;
    }

    public TurnTimer(float timer, UnityAction onDone)
    {
        turnDuration = timer;
        timerDone = onDone;
        Paused = true;
        PandoraSingleton<Pan>.Instance.GetSound("turn_begin", cache: true, delegate (AudioClip clip)
        {
            startTurnSound = clip;
        });
        PandoraSingleton<Pan>.Instance.GetSound("turn_end", cache: true, delegate (AudioClip clip)
        {
            tenSecondSound = clip;
        });
    }

    public void Pause()
    {
        Paused = true;
    }

    public void Resume()
    {
        Paused = false;
        if (!startTurnSoundPlayed)
        {
            startTurnSoundPlayed = true;
            if (startTurnSound != null)
            {
                PandoraSingleton<UIMissionManager>.Instance.audioSource.PlayOneShot(startTurnSound);
            }
        }
        PandoraSingleton<NoticeManager>.Instance.SendNotice(Notices.TIMER_STARTING, this);
    }

    public void Reset(float time = -1f)
    {
        Timer = ((!Mathf.Approximately(time, -1f)) ? time : turnDuration);
        startTurnSoundPlayed = false;
        tenSecondSoundPlayed = false;
    }

    public void Update()
    {
        if (Paused || !(Timer > 0f))
        {
            return;
        }
        Timer -= Time.deltaTime;
        if (!tenSecondSoundPlayed && Timer <= 10f)
        {
            tenSecondSoundPlayed = true;
            if (tenSecondSound != null)
            {
                PandoraSingleton<MissionManager>.Instance.GetCurrentUnit().audioSource.PlayOneShot(tenSecondSound);
            }
        }
        if (Timer <= 0f)
        {
            Timer = 0f;
            Pause();
            timerDone();
        }
    }
}
