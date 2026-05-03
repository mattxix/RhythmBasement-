using UnityEngine;
using System.Collections;

public enum BeatType { None = 0, Red = 1, Blue = 2, Key = 3 }

[System.Serializable]
public class MusicTrack
{
    public AudioClip clip;
    public float bpm;
    public int[] beatMap; // 0=none, 1=red, 2=blue, 3=key
    public float offset;
}

public class BeatManager : MonoBehaviour
{
    public MusicTrack[] tracks;
    public AudioSource sourceA;
    public AudioSource sourceB;
    public float crossfadeDuration = 2f;
    public float downtimeDuration = 3f;
    public float beatSpeed = 5f;
    public RectTransform spawnPoint;
    public GameObject beatPrefabRed;   
    public GameObject beatPrefabBlue;  
    public GameObject beatPrefabKey;   
    public Transform canvas;

    private int currentTrackIndex = 0;
    private int currentBeat = 0;
    private float nextBeatTime = 0f;
    private float beatInterval;
    [HideInInspector] public bool isInDowntime = false;
    private AudioSource activeSource;


    

    private static readonly int[] BeatMap1 = new int[]
    {
    
    1,0,0,0, 1,0,0,0, 1,0,0,0, 1,0,0,0,  // 1–16
    1,0,0,0, 1,0,0,0, 1,0,1,0, 0,0,1,0,  // 17–32
    1,0,0,0, 1,0,0,0, 1,0,0,0, 1,0,0,0,  // 33–48
    1,0,1,0, 0,0,1,0, 1,0,0,0, 1,0,0,0,  // 49–64

    
    1,0,0,0, 1,0,0,0, 2,0,0,0, 1,0,0,0,  // 65–80
    1,0,1,0, 0,0,1,0, 1,0,2,0, 0,0,1,0,  // 81–96
    1,0,0,0, 1,0,0,0, 2,0,0,0, 1,0,0,0,  // 97–112
    1,0,1,0, 1,0,0,0, 2,0,1,0, 0,0,1,0,  // 113–128

    
    1,0,1,0, 1,0,1,0, 1,0,1,0, 1,0,1,0,  // 129–144
    1,0,1,0, 2,0,1,0, 1,0,1,0, 1,0,1,0,  // 145–160
    1,0,1,0, 1,0,1,0, 2,0,1,0, 1,0,1,0,  // 161–176
    1,0,1,0, 2,0,1,0, 1,0,1,0, 3,0,1,0,  // 177–192

    
    1,0,1,0, 1,0,2,0, 1,0,1,0, 2,0,1,0,  // 193–208
    1,0,1,0, 2,0,1,0, 1,0,2,0, 1,0,1,0,  // 209–224
    2,0,1,0, 1,0,2,0, 1,0,1,0, 2,0,1,0,  // 225–240
    1,0,2,0, 1,0,1,0, 3,0,1,0, 1,0,2,0,  // 241–256

    
    1,0,1,0, 1,1,1,0, 2,0,1,0, 1,1,1,0,  // 257–272
    1,0,2,0, 1,1,2,0, 1,0,1,0, 2,1,1,0,  // 273–288
    1,1,1,0, 2,0,1,1, 1,0,2,0, 1,1,1,0,  // 289–304
    2,0,1,1, 1,0,2,0, 3,0,1,0, 2,1,1,0,  // 305–320

    
    1,0,1,1, 2,0,1,0, 1,1,2,0, 3,0,1,0,  // 321–336
    1,1,1,0, 2,1,1,0, 1,0,2,1, 1,0,1,0,  // 337–352
    2,0,1,1, 1,0,2,0, 1,1,1,0, 3,0,2,0,  // 353–368
    1,1,2,0, 1,1,1,0, 2,1,1,0, 3,1,1,0,  // 369–384

    
    1,1,2,0, 1,2,1,1, 2,1,1,0, 3,1,2,0,  // 385–400
    1,2,1,1, 2,1,2,0, 1,1,2,1, 3,0,1,1,  // 401–416
    2,1,1,1, 1,2,1,0, 2,1,1,1, 3,1,2,1,  // 417–432
    1,2,1,1, 2,1,1,1, 1,2,2,1, 3,1,1,1,  // 433–448

    
    2,1,2,1, 1,2,1,0, 2,1,2,0, 1,2,1,1,  // 449–464
    2,0,2,1, 1,2,2,0, 3,1,2,0, 2,1,1,1,  // 465–480
    1,2,2,1, 2,1,1,2, 1,2,1,1, 3,2,1,0,  // 481–496
    2,1,2,1, 1,1,2,1, 3,2,1,1, 2,1,2,0,  // 497–512

    
    1,0,0,0, 0,0,1,0, 0,0,0,1, 0,0,0,0,  // 513–528
    0,0,1,0, 0,0,0,0, 2,0,0,0, 0,0,1,0,  // 529–544
    0,0,0,0, 1,0,0,0, 0,1,0,0, 0,0,2,0,  // 545–560
    0,0,1,0, 0,0,0,1, 0,0,3,0, 0,0,1,0,  // 561–576

    
    1,0,0,0, 1,0,1,0, 1,0,1,0, 2,0,1,0,  // 577–592
    1,0,1,1, 2,0,1,0, 1,1,2,0, 3,0,1,0,  // 593–608
    1,1,1,0, 2,1,1,0, 1,1,2,1, 3,1,1,0,  // 609–624
    1,2,1,1, 2,1,2,1, 3,2,1,1, 2,1,1,1,  // 625–640
    };

    private static readonly int[] BeatMap2 = new int[]
    {
    
    1,0,0,0, 0,0,1,0, 0,1,0,0, 0,0,1,0,  // 1–16
    0,0,1,0, 0,0,0,1, 0,0,1,0, 0,1,0,0,  // 17–32
    2,0,0,0, 0,0,2,0, 0,0,1,0, 0,0,2,0,  // 33–48
    0,0,1,0, 2,0,0,0, 0,1,0,0, 2,0,0,0,  // 49–64

    
    1,0,2,0, 0,0,1,0, 2,0,0,0, 1,0,2,0,  // 65–80
    0,1,0,0, 2,0,1,0, 0,0,2,0, 1,0,0,0,  // 81–96
    2,0,1,0, 0,2,0,0, 1,0,2,0, 0,0,1,0,  // 97–112
    2,0,0,1, 0,2,0,0, 1,0,2,0, 3,0,0,0,  // 113–128

    
    1,0,2,0, 1,0,2,0, 1,0,2,0, 1,0,2,0,  // 129–144
    2,0,1,0, 2,0,1,0, 2,0,1,0, 2,0,1,0,  // 145–160
    1,0,2,0, 1,0,2,0, 2,0,1,0, 2,0,1,0,  // 161–176
    1,0,2,0, 2,0,1,0, 1,0,2,0, 3,0,1,0,  // 177–192

    
    1,0,0,2, 0,1,2,0, 1,0,2,0, 0,2,1,0,  // 193–208
    2,0,1,0, 0,2,0,1, 2,0,0,1, 0,2,1,0,  // 209–224
    1,2,0,1, 0,2,1,0, 2,0,1,2, 0,1,0,2,  // 225–240
    1,0,2,0, 1,2,0,1, 3,0,2,0, 1,0,2,0,  // 241–256

    
    2,0,2,1, 1,2,0,2, 1,2,1,0, 2,2,1,0,  // 257–272
    1,2,2,0, 2,1,2,0, 1,2,1,2, 0,2,1,0,  // 273–288
    2,1,2,0, 1,2,2,1, 2,0,2,1, 1,2,0,2,  // 289–304
    2,1,0,2, 1,2,1,0, 3,0,2,1, 2,1,2,0,  // 305–320

    
    1,2,1,2, 0,2,1,2, 2,1,2,0, 3,2,1,0,  // 321–336
    2,1,2,1, 1,2,0,2, 1,2,1,0, 3,1,2,0,  // 337–352
    2,0,2,1, 2,1,2,0, 1,2,2,1, 3,2,1,2,  // 353–368
    1,2,2,1, 2,2,1,0, 3,2,1,2, 2,1,3,0,  // 369–384

   
    2,1,2,1, 2,1,2,1, 1,2,1,2, 3,2,1,2,  // 385–400
    2,1,1,2, 2,2,1,2, 1,2,2,1, 3,1,2,2,  // 401–416
    1,2,1,2, 2,1,2,1, 3,2,1,2, 2,1,2,1,  // 417–432
    2,2,1,2, 1,2,2,1, 2,1,3,2, 1,2,1,2,  // 433–448

    
    1,2,1,2, 1,1,2,1, 2,1,1,2, 3,1,2,1,  // 449–464
    1,1,2,1, 2,1,1,2, 1,2,1,1, 3,2,1,1,  // 465–480
    2,1,1,2, 1,1,2,1, 1,2,1,1, 3,1,1,2,  // 481–496
    1,2,1,1, 2,2,1,1, 3,1,2,2, 1,2,1,1,  // 497–512

    
    2,0,0,0, 0,0,0,1, 0,0,2,0, 0,0,0,0,  // 513–528
    0,1,0,0, 0,0,2,0, 0,0,0,0, 0,2,0,0,  // 529–544
    1,0,0,0, 0,0,0,2, 0,1,0,0, 0,0,0,0,  // 545–560
    0,0,2,0, 0,0,1,0, 0,0,0,3, 0,0,0,0,  // 561–576

    
    2,0,0,0, 2,0,1,0, 2,0,2,0, 1,0,2,0,  // 577–592
    2,0,2,1, 1,2,2,0, 2,1,2,0, 3,0,2,0,  // 593–608
    2,1,2,0, 1,2,2,1, 2,1,1,2, 3,2,1,0,  // 609–624
    2,1,2,1, 1,2,1,2, 3,2,2,1, 2,1,2,1,  // 625–640
    };

    void Awake()
    {
        // If no tracks assigned in Inspector, build default track shells with our beatmaps
        if (tracks == null || tracks.Length == 0)
        {
            tracks = new MusicTrack[2];
            tracks[0] = new MusicTrack { bpm = 124f, beatMap = BeatMap1, offset = 0f };
            tracks[1] = new MusicTrack { bpm = 90f, beatMap = BeatMap2, offset = 15f };
        }
        else
        {
            // Overwrite beatmaps in-place so Inspector clip/BPM settings are kept
            if (tracks.Length > 0) tracks[0].beatMap = BeatMap1;
            if (tracks.Length > 1) tracks[1].beatMap = BeatMap2;
        }
    }

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

        if (track.clip != null)
        {
            activeSource.clip = track.clip;
            activeSource.Play();
        }
    }

    void Update()
    {
        if (isInDowntime) return;
        if (tracks[currentTrackIndex].clip != null && !activeSource.isPlaying) return;

        MusicTrack track = tracks[currentTrackIndex];

        if (currentBeat >= track.beatMap.Length)
        {
            currentBeat = 0;
            return;
        }

        float timeCheck = (track.clip != null) ? activeSource.time : Time.time;

        if (timeCheck >= nextBeatTime)
        {
            int beatVal = track.beatMap[currentBeat];
            if (beatVal != 0)
                SpawnBeat((BeatType)beatVal);

            currentBeat++;
            nextBeatTime += beatInterval;
        }
    }

    void SpawnBeat(BeatType type)
    {
        GameObject prefab = type switch
        {
            BeatType.Blue => beatPrefabBlue,
            BeatType.Key => beatPrefabKey,
            _ => beatPrefabRed,
        };

        if (prefab == null)
        {
            Debug.LogWarning($"BeatManager: No prefab assigned for beat type {type}");
            return;
        }

        GameObject beat = Instantiate(prefab, canvas);
        beat.GetComponent<RectTransform>().anchoredPosition = spawnPoint.anchoredPosition;

        Beat beatScript = beat.GetComponent<Beat>();
        beatScript.speed = beatSpeed;
        beatScript.beatType = type;
    }

    public void NextSong()
    {
        StartCoroutine(TransitionToNextTrack());
    }

    IEnumerator TransitionToNextTrack()
    {
        isInDowntime = true;

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

        float remaining = downtimeDuration - crossfadeDuration;
        if (remaining > 0) yield return new WaitForSeconds(remaining);

        currentTrackIndex = (currentTrackIndex + 1) % tracks.Length;
        activeSource = incoming;
        LoadTrack(currentTrackIndex);

        isInDowntime = false;
    }
}