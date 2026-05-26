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
    // Section 1 — Intro: sparse, steady pulse
    1,0,0,0, 0,0,0,0, 1,0,0,0, 0,0,0,0,  // 1–16
    1,0,0,0, 0,0,0,0, 1,0,0,0, 0,0,1,0,  // 17–32
    1,0,0,0, 0,0,1,0, 0,0,0,0, 1,0,0,0,  // 33–48
    0,0,1,0, 0,0,0,0, 1,0,0,0, 0,0,1,0,  // 49–64

    // Section 2 — Build: occasional doubles added
    1,0,0,0, 1,0,0,0, 0,0,0,0, 1,0,0,0,  // 65–80
    1,0,0,0, 0,0,1,0, 1,0,0,0, 0,0,1,0,  // 81–96
    1,0,0,0, 1,0,0,0, 2,0,0,0, 0,0,0,0,  // 97–112
    1,0,0,0, 0,0,1,0, 2,0,0,0, 0,0,1,0,  // 113–128

    // Section 3 — Groove: consistent but spaced
    1,0,0,0, 1,0,1,0, 0,0,0,0, 1,0,1,0,  // 129–144
    1,0,0,0, 2,0,0,0, 1,0,1,0, 0,0,0,0,  // 145–160
    1,0,1,0, 0,0,0,0, 2,0,0,0, 1,0,1,0,  // 161–176
    0,0,1,0, 2,0,0,0, 1,0,1,0, 0,0,0,0,  // 177–192

    // Section 4 — Mid intensity: notes per beat, not per step
    1,0,1,0, 0,0,2,0, 0,0,1,0, 2,0,0,0,  // 193–208
    1,0,0,0, 2,0,1,0, 0,0,2,0, 0,0,1,0,  // 209–224
    2,0,0,0, 1,0,2,0, 0,0,1,0, 2,0,0,0,  // 225–240
    1,0,2,0, 0,0,1,0, 3,0,0,0, 0,0,2,0,  // 241–256

    // Section 5 — Syncopation introduced, still roomy
    1,0,1,0, 1,0,0,0, 2,0,1,0, 0,0,0,0,  // 257–272
    1,0,2,0, 0,1,0,0, 1,0,1,0, 2,0,0,0,  // 273–288
    1,0,1,0, 2,0,0,1, 0,0,2,0, 1,0,0,0,  // 289–304
    2,0,0,0, 1,0,2,0, 0,0,1,0, 2,0,0,0,  // 305–320

    // Section 6 — Density increases, gap after every cluster
    1,0,1,1, 0,0,1,0, 1,0,2,0, 0,0,0,0,  // 321–336
    1,0,1,0, 2,0,0,0, 1,0,2,0, 0,0,1,0,  // 337–352
    2,0,0,1, 0,0,2,0, 1,0,1,0, 3,0,0,0,  // 353–368
    1,0,2,0, 0,1,1,0, 2,0,0,0, 3,0,1,0,  // 369–384

    // Section 7 — Peak density, but beats cluster then rest
    1,0,2,0, 1,2,0,0, 2,0,1,0, 0,0,2,0,  // 385–400
    1,2,0,0, 2,0,2,0, 0,0,2,0, 3,0,0,0,  // 401–416
    2,0,1,0, 1,2,0,0, 2,0,1,0, 3,0,0,1,  // 417–432
    1,2,0,0, 2,0,1,0, 1,2,0,0, 3,0,1,0,  // 433–448

    // Section 8 — Sustain peak, slight pullback on clustering
    2,0,2,0, 0,2,1,0, 2,0,0,0, 1,2,0,0,  // 449–464
    2,0,2,0, 1,2,0,0, 3,0,2,0, 0,0,1,0,  // 465–480
    1,2,0,0, 2,0,1,0, 1,2,0,0, 3,0,0,0,  // 481–496
    2,0,2,0, 1,0,2,0, 3,0,1,0, 2,0,0,0,  // 497–512

    // Section 9 — Outro breakdown: very sparse again
    1,0,0,0, 0,0,0,0, 0,0,0,1, 0,0,0,0,  // 513–528
    0,0,1,0, 0,0,0,0, 2,0,0,0, 0,0,0,0,  // 529–544
    0,0,0,0, 1,0,0,0, 0,0,0,0, 0,0,2,0,  // 545–560
    0,0,0,0, 0,0,0,1, 0,0,3,0, 0,0,0,0,  // 561–576

    // Section 10 — Outro rebuild: gradual, doesn't reach full density
    1,0,0,0, 1,0,0,0, 1,0,1,0, 2,0,0,0,  // 577–592
    1,0,1,0, 2,0,0,0, 1,0,2,0, 3,0,0,0,  // 593–608
    1,0,1,0, 2,0,1,0, 1,0,2,0, 3,0,1,0,  // 609–624
    1,2,0,0, 2,0,2,0, 3,0,1,0, 2,0,1,0,  // 625–640
    };

    private static readonly int[] BeatMap2 = new int[]
    {
    // Section 1 — Intro: offset pulse pattern, sparse
    1,0,0,0, 0,0,1,0, 0,0,0,0, 0,0,1,0,  // 1–16
    0,0,1,0, 0,0,0,0, 0,0,1,0, 0,0,0,0,  // 17–32
    2,0,0,0, 0,0,0,0, 0,0,1,0, 0,0,2,0,  // 33–48
    0,0,0,0, 2,0,0,0, 0,0,0,0, 0,0,1,0,  // 49–64

    // Section 2 — Build: note pairs with space between
    1,0,2,0, 0,0,0,0, 2,0,0,0, 0,0,2,0,  // 65–80
    0,0,0,0, 2,0,1,0, 0,0,0,0, 1,0,0,0,  // 81–96
    2,0,1,0, 0,0,0,0, 1,0,2,0, 0,0,0,0,  // 97–112
    2,0,0,0, 0,2,0,0, 1,0,2,0, 0,0,0,0,  // 113–128

    // Section 3 — Alternating pattern, consistent gaps
    1,0,2,0, 0,0,2,0, 1,0,0,0, 1,0,2,0,  // 129–144
    2,0,0,0, 2,0,1,0, 0,0,0,0, 2,0,1,0,  // 145–160
    1,0,2,0, 0,0,0,0, 2,0,1,0, 0,0,1,0,  // 161–176
    1,0,2,0, 0,0,1,0, 1,0,2,0, 0,0,0,0,  // 177–192

    // Section 4 — Offbeats appear, still breathing
    1,0,0,2, 0,0,2,0, 0,0,2,0, 0,2,0,0,  // 193–208
    2,0,1,0, 0,2,0,0, 2,0,0,0, 0,2,1,0,  // 209–224
    1,2,0,0, 0,2,1,0, 2,0,0,2, 0,0,0,0,  // 225–240
    1,0,2,0, 0,2,0,0, 3,0,0,0, 1,0,2,0,  // 241–256

    // Section 5 — Busier but clusters are isolated
    2,0,2,0, 1,2,0,0, 1,2,1,0, 0,0,0,0,  // 257–272
    1,2,2,0, 0,0,2,0, 1,2,0,0, 0,2,1,0,  // 273–288
    2,0,2,0, 1,2,0,0, 2,0,2,0, 0,0,0,2,  // 289–304
    2,0,0,0, 1,2,1,0, 3,0,0,0, 2,0,2,0,  // 305–320

    // Section 6 — Density picks up, clear rests between groups
    1,2,0,2, 0,0,1,2, 2,0,2,0, 0,0,0,0,  // 321–336
    2,0,2,0, 1,2,0,0, 1,2,0,0, 3,0,0,0,  // 337–352
    2,0,2,0, 2,0,2,0, 1,2,0,0, 3,0,0,2,  // 353–368
    1,2,0,0, 2,2,0,0, 3,0,1,2, 0,0,0,0,  // 369–384

    // Section 7 — Peak: dense bursts with mandatory rest beats
    2,0,2,0, 2,0,2,0, 1,2,0,2, 0,0,0,0,  // 385–400
    2,0,1,2, 2,0,1,2, 1,2,0,0, 3,0,0,2,  // 401–416
    1,2,0,2, 2,0,2,0, 3,0,0,2, 0,0,0,0,  // 417–432
    2,2,0,0, 1,2,0,1, 2,0,3,0, 0,0,0,0,  // 433–448

    // Section 8 — Sustain peak, triplets isolated
    1,2,0,2, 0,0,2,0, 2,0,0,2, 0,0,0,0,  // 449–464
    1,0,2,0, 2,0,0,2, 1,2,0,0, 3,0,0,0,  // 465–480
    2,0,0,2, 1,0,2,0, 1,2,0,0, 3,0,0,2,  // 481–496
    1,2,0,0, 2,2,0,0, 3,0,0,2, 0,0,0,0,  // 497–512

    // Section 9 — Outro breakdown: isolated hits only
    2,0,0,0, 0,0,0,0, 0,0,2,0, 0,0,0,0,  // 513–528
    0,0,0,0, 0,0,2,0, 0,0,0,0, 0,0,0,0,  // 529–544
    1,0,0,0, 0,0,0,0, 0,1,0,0, 0,0,0,0,  // 545–560
    0,0,2,0, 0,0,0,0, 0,0,0,3, 0,0,0,0,  // 561–576

    // Section 10 — Outro rebuild: slower climb, ends before full density
    2,0,0,0, 2,0,0,0, 2,0,2,0, 0,0,2,0,  // 577–592
    2,0,2,0, 1,2,0,0, 2,0,2,0, 3,0,0,0,  // 593–608
    2,0,2,0, 1,2,0,1, 2,0,0,2, 3,0,1,0,  // 609–624
    2,0,2,0, 1,2,0,2, 3,0,2,0, 2,0,2,0,  // 625–640
    };

    

    private static readonly int[] BeatMap3 = new int[]
    {
    // Section 1 — Intro: clean quarter-note pulse
    1,0,0,0, 0,0,0,0, 1,0,0,0, 0,0,0,0,  // 1–16
    1,0,0,0, 0,0,0,0, 1,0,0,0, 0,0,0,0,  // 17–32
    1,0,0,0, 0,0,0,0, 1,0,0,0, 0,0,0,0,  // 33–48
    1,0,0,0, 0,0,0,0, 1,0,0,0, 0,0,0,0,  // 49–64

    // Section 2 — Build: add a hit on beat 3
    1,0,0,0, 1,0,0,0, 0,0,0,0, 1,0,0,0,  // 65–80
    1,0,0,0, 1,0,0,0, 0,0,0,0, 1,0,0,0,  // 81–96
    1,0,0,0, 1,0,0,0, 2,0,0,0, 1,0,0,0,  // 97–112
    1,0,0,0, 1,0,0,0, 2,0,0,0, 1,0,0,0,  // 113–128

    // Section 3 — Groove: steady two-beat pattern established
    1,0,0,0, 1,0,0,0, 1,0,0,0, 1,0,0,0,  // 129–144
    1,0,0,0, 2,0,0,0, 1,0,0,0, 1,0,0,0,  // 145–160
    1,0,0,0, 1,0,0,0, 2,0,0,0, 1,0,0,0,  // 161–176
    1,0,0,0, 2,0,0,0, 1,0,0,0, 1,0,0,0,  // 177–192

    // Section 4 — Mid: half-beat fills start appearing between pulses
    1,0,0,0, 1,0,1,0, 0,0,0,0, 1,0,0,0,  // 193–208
    1,0,1,0, 0,0,0,0, 2,0,0,0, 1,0,1,0,  // 209–224
    1,0,0,0, 2,0,1,0, 0,0,0,0, 1,0,0,0,  // 225–240
    2,0,1,0, 0,0,0,0, 1,0,0,0, 2,0,1,0,  // 241–256

    // Section 5 — Syncopation: occasional early hit, then rest
    1,0,1,0, 1,0,0,0, 0,0,1,0, 1,0,0,0,  // 257–272
    2,0,0,0, 1,0,1,0, 0,0,0,0, 2,0,0,0,  // 273–288
    1,0,1,0, 2,0,0,0, 1,0,0,0, 0,0,1,0,  // 289–304
    2,0,0,0, 1,0,1,0, 2,0,0,0, 0,0,1,0,  // 305–320

    // Section 6 — Density lift: beats cluster in twos, gaps preserved
    1,0,1,0, 0,0,0,0, 2,0,1,0, 0,0,0,0,  // 321–336
    1,0,1,0, 2,0,0,0, 0,0,1,0, 2,0,0,0,  // 337–352
    1,0,1,0, 0,0,2,0, 1,0,0,0, 2,0,1,0,  // 353–368
    0,0,0,0, 1,0,2,0, 0,0,1,0, 3,0,0,0,  // 369–384

    // Section 7 — Peak: triplets used as accents, not walls
    1,0,2,0, 1,0,0,0, 2,0,1,0, 0,0,0,0,  // 385–400
    1,0,2,0, 3,0,0,0, 1,0,2,0, 0,0,1,0,  // 401–416
    2,0,0,0, 1,0,2,0, 3,0,0,0, 1,0,2,0,  // 417–432
    0,0,1,0, 2,0,0,0, 3,0,1,0, 0,0,2,0,  // 433–448

    // Section 8 — Sustain: same peak density, no new tricks
    1,0,2,0, 0,0,1,0, 2,0,0,0, 3,0,1,0,  // 449–464
    0,0,2,0, 1,0,0,0, 2,0,1,0, 0,0,3,0,  // 465–480
    1,0,0,0, 2,0,1,0, 0,0,2,0, 3,0,0,0,  // 481–496
    1,0,2,0, 0,0,1,0, 3,0,0,0, 2,0,1,0,  // 497–512

    // Section 9 — Outro breakdown: back to lone quarter notes
    1,0,0,0, 0,0,0,0, 0,0,0,0, 0,0,0,0,  // 513–528
    0,0,0,0, 1,0,0,0, 0,0,0,0, 0,0,0,0,  // 529–544
    2,0,0,0, 0,0,0,0, 0,0,0,0, 1,0,0,0,  // 545–560
    0,0,0,0, 0,0,0,0, 3,0,0,0, 0,0,0,0,  // 561–576

    // Section 10 — Outro rebuild: walks back up the same staircase
    1,0,0,0, 1,0,0,0, 1,0,0,0, 2,0,0,0,  // 577–592
    1,0,0,0, 2,0,1,0, 0,0,0,0, 1,0,0,0,  // 593–608
    2,0,1,0, 0,0,0,0, 1,0,2,0, 3,0,0,0,  // 609–624
    1,0,2,0, 0,0,1,0, 2,0,3,0, 1,0,2,0,  // 625–640
    };

   

    private static readonly int[] BeatMap4 = new int[]
    {
    // Section 1 — Intro: single isolated hits, not on beat 1
    0,0,1,0, 0,0,0,0, 0,1,0,0, 0,0,0,0,  // 1–16
    0,0,0,1, 0,0,0,0, 0,0,1,0, 0,0,0,0,  // 17–32
    0,1,0,0, 0,0,0,0, 0,0,0,1, 0,0,0,0,  // 33–48
    0,0,1,0, 0,0,0,0, 0,1,0,0, 0,0,0,1,  // 49–64

    // Section 2 — Build: pairs of offbeats, still quiet
    0,0,1,0, 0,1,0,0, 0,0,0,0, 0,0,1,0,  // 65–80
    0,1,0,0, 0,0,0,1, 0,0,1,0, 0,0,0,0,  // 81–96
    0,0,2,0, 0,1,0,0, 0,0,0,0, 0,2,0,0,  // 97–112
    0,0,1,0, 0,0,2,0, 0,1,0,0, 0,0,0,0,  // 113–128

    // Section 3 — Groove: alternating offbeat rhythm, loose feel
    0,1,0,0, 0,0,2,0, 0,1,0,0, 0,0,0,0,  // 129–144
    0,0,2,0, 0,1,0,0, 0,0,1,0, 0,2,0,0,  // 145–160
    0,1,0,0, 0,0,0,2, 0,0,1,0, 0,1,0,0,  // 161–176
    0,0,2,0, 0,1,0,0, 0,0,0,2, 0,0,1,0,  // 177–192

    // Section 4 — Mid: occasional downbeat sneak-in for contrast
    1,0,0,0, 0,1,0,0, 0,0,2,0, 0,1,0,0,  // 193–208
    0,0,0,2, 0,1,0,0, 1,0,0,0, 0,0,2,0,  // 209–224
    0,1,0,0, 0,0,0,2, 0,1,0,0, 1,0,0,0,  // 225–240
    0,0,2,0, 0,1,0,0, 0,0,0,1, 3,0,0,0,  // 241–256

    // Section 5 — Syncopation: hits shift around the beat deliberately
    0,1,2,0, 0,0,0,0, 0,2,0,1, 0,0,0,0,  // 257–272
    0,0,1,0, 0,2,0,0, 0,1,0,0, 0,0,2,0,  // 273–288
    0,1,0,2, 0,0,0,0, 1,0,0,2, 0,1,0,0,  // 289–304
    0,0,2,0, 0,1,0,2, 0,0,0,0, 3,0,0,0,  // 305–320

    // Section 6 — Density lift: quick two-hit bursts then silence
    0,1,0,2, 0,0,0,0, 0,2,0,1, 0,0,0,0,  // 321–336
    0,1,2,0, 0,0,0,0, 1,0,0,2, 0,1,0,0,  // 337–352
    0,0,2,1, 0,0,0,0, 0,1,0,2, 0,0,3,0,  // 353–368
    0,0,0,0, 0,2,1,0, 0,0,2,0, 3,0,0,1,  // 369–384

    // Section 7 — Peak: bursts of 3 offbeat hits then a full bar rest
    0,1,2,1, 0,0,0,0, 0,2,1,0, 0,0,0,0,  // 385–400
    0,1,0,2, 0,1,0,0, 3,0,0,0, 0,0,0,0,  // 401–416
    0,2,1,0, 0,1,2,0, 0,0,0,0, 3,0,0,1,  // 417–432
    0,0,2,0, 0,1,0,2, 0,0,0,0, 0,3,0,1,  // 433–448

    // Section 8 — Sustain: same energy, positions keep shifting
    0,2,0,1, 0,0,2,0, 0,1,0,0, 3,0,0,0,  // 449–464
    0,0,1,2, 0,0,0,0, 0,2,0,1, 0,1,0,0,  // 465–480
    3,0,0,0, 0,1,2,0, 0,0,1,0, 0,2,0,1,  // 481–496
    0,0,0,0, 0,2,1,0, 3,0,0,1, 0,0,2,0,  // 497–512

    // Section 9 — Outro breakdown: one hit, long silence, repeat
    0,0,1,0, 0,0,0,0, 0,0,0,0, 0,0,0,0,  // 513–528
    0,0,0,0, 0,1,0,0, 0,0,0,0, 0,0,0,0,  // 529–544
    0,0,0,0, 0,0,0,2, 0,0,0,0, 0,0,0,0,  // 545–560
    0,0,0,1, 0,0,0,0, 0,0,0,0, 3,0,0,0,  // 561–576

    // Section 10 — Outro rebuild: offbeat pairs return, climb is gentle
    0,0,1,0, 0,1,0,0, 0,0,2,0, 0,1,0,0,  // 577–592
    0,2,0,0, 0,1,2,0, 0,0,0,0, 0,2,0,1,  // 593–608
    0,1,2,0, 0,0,2,1, 0,0,0,0, 3,0,0,1,  // 609–624
    0,2,1,0, 0,1,2,0, 3,0,1,0, 0,2,0,1,  // 625–640
    };

    void Awake()
    {
        if (tracks == null || tracks.Length == 0)
        {
            tracks = new MusicTrack[4];
            tracks[0] = new MusicTrack { bpm = 73, beatMap = BeatMap1, offset = 0f };
            tracks[1] = new MusicTrack { bpm = 80, beatMap = BeatMap2, offset = 15f };
            tracks[2] = new MusicTrack { bpm = 124, beatMap = BeatMap3, offset = 0f };
            tracks[3] = new MusicTrack { bpm = 90, beatMap = BeatMap4, offset = 0f };
        }
        else
        {
            if (tracks.Length > 0) tracks[0].beatMap = BeatMap1;
            if (tracks.Length > 1) tracks[1].beatMap = BeatMap2;
            if (tracks.Length > 2) tracks[2].beatMap = BeatMap3;
            if (tracks.Length > 3) tracks[3].beatMap = BeatMap4;
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