using MelonLoader;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[assembly: MelonInfo(typeof(CustomTV.CustomTV), CustomTV.BuildInfo.Name, CustomTV.BuildInfo.Version, CustomTV.BuildInfo.Author, CustomTV.BuildInfo.DownloadLink)]
[assembly: MelonColor()]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace CustomTV
{
    public static class BuildInfo
    {
        public const string Name = "CustomTV";
        public const string Description = "Lets you play your own MP4 videos on the TV.";
        public const string Author = "Jumble";
        public const string Company = null;
        public const string Version = "1.3";
        public const string DownloadLink = "www.nexusmods.com/schedule1/mods/603";
    }

    public static class Config
    {
        public static KeyCode PauseKey { get; private set; } = KeyCode.Minus;
        public static KeyCode ResumeKey { get; private set; } = KeyCode.Equals;
        public static KeyCode NextVideoKey { get; private set; } = KeyCode.RightBracket;
        public static KeyCode PreviousVideoKey { get; private set; } = KeyCode.LeftBracket;
        public static KeyCode Seekforward { get; private set; } = KeyCode.RightArrow;
        public static KeyCode Seekbackward { get; private set; } = KeyCode.LeftArrow;
        public static float AudioVolume { get; private set; } = 1.0f;
        public static float SeekAmount { get; private set; } = 5.0f;
        public static bool Shuffle { get; private set; } = true;

        private static readonly string modsFolderPath = Path.Combine(Application.dataPath, "../Mods");
        private static readonly string tvFolderPath = Path.Combine(modsFolderPath, "TV");

        private static readonly string configPath = Path.Combine(tvFolderPath, "CustomTVConfig.ini");

        public static void Load()
        {
            if (!Directory.Exists(tvFolderPath))
            {
                Directory.CreateDirectory(tvFolderPath);
                MelonLogger.Msg("Created 'TV' folder in Mods directory.");
            }

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath,
    @"; Valid key names: https://docs.unity3d.com/ScriptReference/KeyCode.html

[Keybinds]
Pause = Minus
Resume = Equals
Skip = RightBracket
Previous = LeftBracket
Seek Forward = RightArrow
Seek Backward = LeftArrow

[Values]
Volume = 100
Seek Amount = 5
Shuffle = True");
                MelonLogger.Msg("CustomTVConfig.ini created with default values.");
            }

            string[] lines = File.ReadAllLines(configPath);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.StartsWith(";") || line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('=');
                if (parts.Length != 2) continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                switch (key.ToLowerInvariant())
                {
                    case "pause":
                        if (Enum.TryParse(value, out KeyCode pause))
                            PauseKey = pause;
                        break;
                    case "resume":
                        if (Enum.TryParse(value, out KeyCode resume))
                            ResumeKey = resume;
                        break;
                    case "skip":
                        if (Enum.TryParse(value, out KeyCode skip))
                            NextVideoKey = skip;
                        break;
                    case "previous":
                        if (Enum.TryParse(value, out KeyCode prev))
                            PreviousVideoKey = prev;
                        break;
                    case "seek forward":
                        if (Enum.TryParse(value, out KeyCode seekFwd))
                            Seekforward = seekFwd;
                        break;
                    case "seek backward":
                        if (Enum.TryParse(value, out KeyCode seekBack))
                            Seekbackward = seekBack;
                        break;
                    case "volume":
                        if (float.TryParse(value, out float volPercent))
                        {
                            volPercent = Mathf.Clamp(volPercent, 0f, 100f);
                            AudioVolume = volPercent / 100f;
                        }
                        break;
                    case "seek amount":
                        if (float.TryParse(value, out float seekVal))
                        {
                            SeekAmount = seekVal;
                        }
                        break;
                    case "shuffle":
                        if (bool.TryParse(value.ToLower(), out bool shuffle))
                            Shuffle = shuffle;
                        break;
                }
            }

            MelonLogger.Msg("CustomTVConfig.ini loaded.");
        }
    }

    public class CustomTV : MelonMod
    {
        private static RenderTexture sharedRenderTexture;
        private static bool wasPausedByTimescale = false;
        private static bool wasPaused = false;
        private static bool settingUp = false;
        private static bool settingUpdate = false;
        private static double savedPlaybackTime = 0;
        private static string videoFilePath = Path.Combine(Application.dataPath, "../Mods/TV/video.mp4");
        private static List<string> videoFiles = new();
        private static int currentVideoIndex = 0;
        private static readonly System.Random rng = new();
        private static readonly Action<VideoPlayer> videoEndHandler = OnVideoEnd;
        private static readonly List<VideoPlayer> passiveVideoPlayers = new();
        private static List<Transform> tvInterfaces = new();

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
            MelonCoroutines.Start(SetupVideoPlayer(true));
        }

        public override void OnInitializeMelon()
        {
            Config.Load();
        }

        public override void OnUpdate()
        {
            if (!settingUpdate) MelonCoroutines.Start(HandleUpdate());
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                if (Input.GetKeyDown(Config.PauseKey))
                {
                    if (passiveVideoPlayers.Count > 0 && !wasPaused)
                    {
                        var master = passiveVideoPlayers[0];
                        savedPlaybackTime = master.time;
                        master.Pause();
                        wasPaused = true;
                        for (int i = 0; i < passiveVideoPlayers.Count; i++)
                        {
                            if (i != 0)
                            {
                                passiveVideoPlayers[i].Pause();
                            }
                        }
                    }
                }
                else if (Input.GetKeyDown(Config.ResumeKey))
                {
                    if (passiveVideoPlayers.Count > 0 && wasPaused && Time.timeScale > 0.1f)
                    {
                        var master = passiveVideoPlayers[0];
                        master.time = savedPlaybackTime;
                        master.Play();
                        wasPaused = false;
                        wasPausedByTimescale = false;
                        for (int i = 0; i < passiveVideoPlayers.Count; i++)
                        {
                            if (i != 0)
                            {
                                passiveVideoPlayers[i].Play();
                            }
                        }
                    }
                }
                if (Input.GetKeyDown(Config.NextVideoKey))
                {
                    savedPlaybackTime = 0;
                    MelonCoroutines.Start(SetupVideoPlayer());
                }
                if (Input.GetKeyDown(Config.PreviousVideoKey))
                {
                    savedPlaybackTime = 0;
                    MelonCoroutines.Start(SetupVideoPlayer(false, false));
                }
                if (Input.GetKeyDown(Config.Seekforward))
                {
                    savedPlaybackTime += Config.SeekAmount;
                    MelonCoroutines.Start(HandleSeek(savedPlaybackTime));
                }
                if (Input.GetKeyDown(Config.Seekbackward))
                {
                    savedPlaybackTime -= Config.SeekAmount;
                    MelonCoroutines.Start(HandleSeek(savedPlaybackTime));
                }
            }          
        }

        private static IEnumerator HandleSeek(double newTime)
        {
            if (passiveVideoPlayers[0] == null)
                yield break;
            
            for (int i = 0; i < passiveVideoPlayers.Count; i++)
            {
                passiveVideoPlayers[i].time = newTime;            
            }
        }

        private static IEnumerator HandleUpdate()
        {
            if (settingUpdate) yield break;
            settingUpdate = true;
            try
            {
                if (passiveVideoPlayers.Count == 0)
                {
                    settingUpdate = false;
                    yield break;
                }
                var vp = passiveVideoPlayers[0];
                if (vp == null)
                {
                    settingUpdate = false;
                    yield break;
                }

                if (!wasPausedByTimescale && !wasPaused && vp.isPlaying)
                {
                    double currentTime = vp.time;
                    double videoLength = vp.length;
                    if (currentTime > 0.1 || (videoLength > 0 && (currentTime >= videoLength - 0.5 || savedPlaybackTime >= videoLength - 0.5)))
                    {
                        savedPlaybackTime = currentTime;
                    }
                }

                if (Time.timeScale == 0f && vp.isPlaying)
                {
                    vp.Pause();
                    wasPausedByTimescale = true;
                    for (int i = 0; i < passiveVideoPlayers.Count; i++)
                    {
                        if (i != 0)
                        {
                            passiveVideoPlayers[i].Pause();
                        }
                    }
                }
                else if (Time.timeScale > 0.1f && !wasPaused && (!vp.isPlaying || wasPausedByTimescale))
                {
                    vp.time = savedPlaybackTime;
                    vp.Play();
                    wasPausedByTimescale = false;
                    for (int i = 0; i < passiveVideoPlayers.Count; i++)
                    {
                        if (i != 0)
                        {
                            passiveVideoPlayers[i].Play();
                        }
                    }
                }

                for (int i = 0; i < tvInterfaces.Count; i++)
                {
                    var tvInterface = tvInterfaces[i];
                    if (tvInterface == null) continue;
                    var timeChild = tvInterface.Find("Time");
                    if (timeChild == null) continue;
                    var renderer = timeChild.GetComponent<MeshRenderer>();
                    if (renderer == null || renderer.material == null) continue;

                    Material mat = renderer.material;
                    bool needsUpdate = (mat.shader == null || mat.shader.name != "UI/Default" || mat.mainTexture != sharedRenderTexture);
                    if (needsUpdate)
                    {
                        MelonCoroutines.Start(SetupPassiveDisplay(timeChild, i == 0));
                    }
                }
            }
            catch { }
            settingUpdate = false;

        }

        private static IEnumerator SetupVideoPlayer(bool first = false, bool forward = true)
        {
            if (first) yield return new WaitForSeconds(7f);
            if (settingUp) yield break;
            settingUp = true;

            tvInterfaces.Clear();
            passiveVideoPlayers.Clear();
            tvInterfaces = GameObject.FindObjectsOfType<Transform>().Where(t => t.name == "TVInterface").ToList();
            sharedRenderTexture = new RenderTexture(1920, 1080, 0);

            if (tvInterfaces.Count == 0)
            {
                settingUp = false;
                yield break;
            }

            string videoDir = Path.Combine(Application.dataPath, "../Mods/TV/");
            if (!Directory.Exists(videoDir))
            {
                settingUp = false;
                MelonLogger.Error("Could not find TV folder. You must create a TV folder in your mods folder and place your MP4s inside it.");
                yield break;
            }

            var newVideoFiles = Directory.GetFiles(videoDir, "*.mp4").ToList();
            if (newVideoFiles.Count == 0)
            {
                settingUp = false;
                MelonLogger.Error("Could not find MP4s.");
                yield break;
            } else if (newVideoFiles.Count != videoFiles.Count)
            {
                newVideoFiles.Shuffle(rng);
                videoFiles = newVideoFiles;
                if (Config.Shuffle)
                {
                    MelonLogger.Msg("Shuffling videos.");
                    videoFiles.Shuffle(rng);
                }
                else
                {
                    MelonLogger.Msg("Sorting videos.");
                    videoFiles = newVideoFiles.OrderBy(f => f, new SmartEpisodeComparer()).ToList();
                }
            }
            int newIndex = first ? 0 : forward ? (currentVideoIndex + 1) % videoFiles.Count : (currentVideoIndex - 1 + videoFiles.Count) % videoFiles.Count;         

            currentVideoIndex = newIndex;
            videoFilePath = videoFiles[currentVideoIndex];

            for (int i = 0; i < tvInterfaces.Count; i++)
            {
                bool isMaster = (i == 0);
                if (tvInterfaces[i] == null) continue;
                var timeChild = tvInterfaces[i].Find("Time");
                MelonCoroutines.Start(SetupPassiveDisplay(timeChild, isMaster));
            }

            while (passiveVideoPlayers.Count != tvInterfaces.Count) yield return null;
            settingUp = false;
        }

        private static IEnumerator SetupPassiveDisplay(Transform timeChild, bool makePlayer)
        {
            if (timeChild == null) yield break;

            for (int i = timeChild.childCount - 1; i >= 0; i--) GameObject.Destroy(timeChild.GetChild(i).gameObject);

            MeshRenderer renderer = timeChild.GetComponent<MeshRenderer>() ?? timeChild.gameObject.AddComponent<MeshRenderer>();
            MeshFilter meshFilter = timeChild.GetComponent<MeshFilter>() ?? timeChild.gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateQuadMesh();

            Shader unlitShader = Shader.Find("UI/Default");
            if (unlitShader == null) yield break;

            Material cleanMat = new(unlitShader)
            {
                mainTexture = sharedRenderTexture
            };
            renderer.material = cleanMat;

            timeChild.localPosition = Vector3.zero;
            timeChild.localScale = new Vector3(680f, 400f, 0f);

            VideoPlayer vp = timeChild.GetComponent<VideoPlayer>() ?? timeChild.gameObject.AddComponent<VideoPlayer>();
            AudioSource audioSrc = timeChild.GetComponent<AudioSource>() ?? timeChild.gameObject.AddComponent<AudioSource>();
            audioSrc.spatialBlend = 1.0f;
            audioSrc.minDistance = 1f;
            audioSrc.maxDistance = 10f;
            audioSrc.volume = Config.AudioVolume;
            audioSrc.rolloffMode = AudioRolloffMode.Logarithmic;

            vp.playOnAwake = false;
            vp.isLooping = (videoFiles.Count == 1);
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            vp.SetTargetAudioSource(0, audioSrc);
            vp.url = videoFilePath;

            if (makePlayer)
            {
                vp.renderMode = VideoRenderMode.RenderTexture;
                vp.targetTexture = sharedRenderTexture;
            }
            else
            {
                vp.renderMode = VideoRenderMode.APIOnly;
            }

            passiveVideoPlayers.Add(vp);
            vp.Prepare();
            while (!vp.isPrepared) yield return null;
            yield return new WaitForSeconds(0.1f);
            vp.time = savedPlaybackTime;
            if (makePlayer)
                yield return new WaitForSeconds(0.1f);
            if (makePlayer && !wasPaused && !wasPausedByTimescale || !makePlayer) vp.Play();
            vp.time = savedPlaybackTime;
            if (makePlayer)
                vp.loopPointReached += videoEndHandler;
        }

        private static void OnVideoEnd(VideoPlayer vp)
        {
            if (videoFiles.Count <= 1) return;
            vp.loopPointReached -= videoEndHandler;
            int newIndex;
            do
            {
                newIndex = rng.Next(videoFiles.Count);
            } while (videoFiles.Count > 1 && newIndex == currentVideoIndex);
            currentVideoIndex = newIndex;
            videoFilePath = videoFiles[currentVideoIndex];
            if (savedPlaybackTime > 0.2) savedPlaybackTime = 0;
            MelonCoroutines.Start(SetupVideoPlayer());
        }

        private static Mesh CreateQuadMesh()
        {
            Mesh mesh = new()
            {
                vertices = new Vector3[]
                {
                    new(-0.5f, -0.5f, 0),
                    new( 0.5f, -0.5f, 0),
                    new( 0.5f,  0.5f, 0),
                    new(-0.5f,  0.5f, 0)
                },

                uv = new Vector2[]
                {
                    new(0, 0),
                    new(1, 0),
                    new(1, 1),
                    new(0, 1)
                },

                triangles = new int[]
                {
                    0, 2, 1,
                    0, 3, 2
                }
            };

            mesh.RecalculateNormals();
            return mesh;
        }
    }
    public static class Extensions
    {
        public static void Shuffle<T>(this IList<T> list, System.Random rng)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
