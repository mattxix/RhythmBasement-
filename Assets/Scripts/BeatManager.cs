using UnityEngine;
using System.Collections;

[System.Serializable]
public class MusicTrack
{
    public AudioClip clip;
    public float bpm;
    public int[] beatMap;
    public float offset;
}
public class BeatManager : MonoBehaviour
{
    public MusicTrack[] tracks;
    public AudioSource sourceA;
    public AudioSource sourceB; // crossfade needs two sources
    public float crossfadeDuration = 2f;
    public float downtimeDuration = 3f; // silence between tracks
    public float beatSpeed = 5f;
    public RectTransform spawnPoint;
    public GameObject beatPrefab;
    public Transform canvas;

    private int currentTrackIndex = 0;
    private int currentBeat = 0;
    private float nextBeatTime = 0f;
    private float beatInterval;
    private bool isInDowntime = false;
    private AudioSource activeSource;

    void Start()
    {
        activeSource = sourceA;
        LoadTrack(currentTrackIndex);
    }

    void LoadTrack(int index)
    {
        MusicTrack track = tracks[index];
        beatInterval = 60f / track.bpm;
        nextBeatTime = track.offset;
        currentBeat = 0;

        activeSource.clip = track.clip;
        activeSource.Play();
    }

    void Update()
    {
        if (isInDowntime) return;
        if (!activeSource.isPlaying) return;

        MusicTrack track = tracks[currentTrackIndex];

        // Song finished, start downtime
        if (currentBeat >= track.beatMap.Length)
        {
            StartCoroutine(TransitionToNextTrack());
            return;
        }

        if (activeSource.time >= nextBeatTime)
        {
            if (track.beatMap[currentBeat] == 1)
                SpawnBeat();

            currentBeat++;
            nextBeatTime += beatInterval;
        }
    }

    void SpawnBeat()
    {
        GameObject beat = Instantiate(beatPrefab, canvas);
        beat.GetComponent<RectTransform>().anchoredPosition = spawnPoint.anchoredPosition;
        beat.GetComponent<Beat>().speed = beatSpeed;
    }

    IEnumerator TransitionToNextTrack()
    {
        isInDowntime = true;

        // Crossfade out current track
        AudioSource incoming = (activeSource == sourceA) ? sourceB : sourceA;
        float timer = 0f;

        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            activeSource.volume = Mathf.Lerp(1f, 0f, timer / crossfadeDuration);
            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = 1f;

        // Wait out remaining downtime
        float remaining = downtimeDuration - crossfadeDuration;
        if (remaining > 0) yield return new WaitForSeconds(remaining);

        // Load next track
        currentTrackIndex = (currentTrackIndex + 1) % tracks.Length;
        activeSource = incoming;
        LoadTrack(currentTrackIndex);

        isInDowntime = false;
    }

}
