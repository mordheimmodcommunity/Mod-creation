using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class VideoPlayer : MonoBehaviour
{
    private RawImage rawImage;

    private AudioSource videoSound;

    private MovieTexture currentVideo;

    private Action onVideoEnd;

    private bool videoStarted;

    private bool videoEnded;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        videoSound = GetComponent<AudioSource>();
    }

    public void Stop()
    {
        if (currentVideo != null)
        {
            currentVideo.Stop();
        }
    }

    private void Update()
    {
        if (videoStarted && currentVideo != null && !currentVideo.isPlaying)
        {
            videoStarted = false;
            OnVideoEnd();
        }
    }

    public IEnumerator Play(string path, Action onVideoDone)
    {
        onVideoEnd = onVideoDone;
        ResourceRequest resourceRequest = Resources.LoadAsync<MovieTexture>("video/" + path);
        yield return resourceRequest;
        currentVideo = (MovieTexture)resourceRequest.asset;
        rawImage.set_texture((Texture)currentVideo);
        videoSound.clip = currentVideo.audioClip;
        ((Graphic)rawImage).set_color(Color.white);
        videoStarted = true;
        currentVideo.Play();
        videoSound.Play();
    }

    private void OnVideoEnd()
    {
        if (onVideoEnd != null)
        {
            onVideoEnd();
        }
    }
}
