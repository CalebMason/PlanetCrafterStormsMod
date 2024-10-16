using BepInEx.Logging;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;

namespace Storms
{
    internal class Storm
    {
        private Vector3 stormPosition;
        private Vector3 startPosition;
        private Vector3 velocity; // New field for storm velocity
        private ManualLogSource logger;
        private GameObject stormAudioObject;
        private AudioSource audioSource;

        public Storm(Vector3 position, Vector3 velocity, ManualLogSource logSource)
        {
            startPosition = position;
            stormPosition = position;
            this.velocity = velocity; // Initialize the velocity
            logger = logSource;
        }

        public IEnumerator Start()
        {
            // Create and configure the audio GameObject
            stormAudioObject = new GameObject("StormAudio");
            stormAudioObject.transform.position = stormPosition;

            logger.LogInfo(stormAudioObject.transform.position.ToString());

            audioSource = stormAudioObject.AddComponent<AudioSource>();

            string path = $"{BepInEx.Paths.PluginPath}/Caleb Mason - (Feat) Adds Storms to The Planet/thunderstorm-14708.mp3";
            logger.LogInfo($"Loading audio from: {path}");

            yield return LoadAndPlayAudio(path, audioSource);

            AudioSource rainAudioSource = stormAudioObject.AddComponent<AudioSource>();
            string rainPath = $"{BepInEx.Paths.PluginPath}/Caleb Mason - (Feat) Adds Storms to The Planet/rain-sound-188158.mp3";
            logger.LogInfo($"Loading rain audio from: {rainPath}");

            yield return LoadAndPlayAudio(rainPath, rainAudioSource);

            // Start moving the storm
            while (true)
            {
                MoveStorm();
                yield return null; // Wait one frame
            }
        }

        private void MoveStorm()
        {
            stormPosition += velocity * Time.deltaTime; // Update position based on velocity
            if (stormPosition.z > 2500 || stormPosition.x > 2500)
            {
                stormPosition = startPosition;
            }
            stormAudioObject.transform.position = stormPosition; // Update audio object position
            logger.LogInfo($"Storm {stormPosition}");
        }

        private IEnumerator LoadAndPlayAudio(string path, AudioSource audioSource)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    logger.LogError($"Failed to load audio clip: {www.error}");
                }
                else
                {
                    AudioClip stormSound = DownloadHandlerAudioClip.GetContent(www);
                    audioSource.clip = stormSound;

                    audioSource.spatialBlend = 1.0f; // 3D sound
                    audioSource.rolloffMode = AudioRolloffMode.Custom;

                    AnimationCurve customCurve = new AnimationCurve();
                    customCurve.AddKey(0.0f, 1.0f); 
                    customCurve.AddKey(800.0f, 0.7f);
                    customCurve.AddKey(900.0f, 0.4f);
                    customCurve.AddKey(1000.0f, 0.0f);

                    audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customCurve);
                    audioSource.loop = true;
                    logger.LogInfo("Playing storm sound.");
                    audioSource.Play();
                }
            }
        }
    }
}
