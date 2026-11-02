using System.Collections;
using UnityEngine;

/// <summary>Press Space to throw the Poké Ball and capture the Pokémon.</summary>
public class PokemonCapture : MonoBehaviour
{
    [Header("Scene objects")]
    public Transform ballStartPoint;
    public Transform capturePoint;
    public Transform pokemon;
    public Transform pokeball;
    public Transform captureEffectRoot;

    [Header("Capture audio")]
    [Tooltip("收服成功时播放的音效；将音频素材拖到 Inspector 的此栏。")]
    public AudioClip captureSuccessSound;
    [Range(0f, 1f)] public float captureSoundVolume = 1f;
    private AudioSource captureAudioSource;

    [Header("Throw")]
    [Min(0.1f)] public float throwDuration = 1.5f;
    [Min(0f)] public float throwHeight = 2f;
    public KeyCode throwKey = KeyCode.Space;

    private bool hasStarted;
    private Transform ballTransform;
    private Transform pokemonTransform;
    private bool initialized;

    private void Start()
    {
        // Keep audio on the persistent GameManager so hiding the Pokémon cannot stop it.
        captureAudioSource = GetComponent<AudioSource>();
        if (captureAudioSource == null)
            captureAudioSource = gameObject.AddComponent<AudioSource>();
        captureAudioSource.playOnAwake = false;
        captureAudioSource.spatialBlend = 0f; // 2D audio: no distance attenuation.
        captureAudioSource.loop = false;

        // The visible ball is the imported "new pokeball" prefab root.
        // pokeballModel is only an empty child and must not be moved by itself.
        GameObject importedBall = FindSceneObject("new pokeball");
        if (importedBall != null)
            pokeball = importedBall.transform;
        else if (pokeball == null)
        {
            GameObject ballModel = FindSceneObject("pokeballModel");
            if (ballModel != null) pokeball = ballModel.transform;
        }

        if (pokemon == null)
        {
            GameObject foundPokemon = FindSceneObject("Psyduck", "Pokemon");
            if (foundPokemon != null) pokemon = foundPokemon.transform;
        }
        if (ballStartPoint == null)
            ballStartPoint = FindSceneTransform("ballStartPoint", "ballStarPoint");
        if (capturePoint == null && pokemon != null)
            capturePoint = pokemon.transform;
        if (captureEffectRoot == null)
        {
            ParticleSystem foundEffect = FindObjectOfType<ParticleSystem>(true);
            if (foundEffect != null) captureEffectRoot = foundEffect.transform;
        }

        if (pokeball == null || pokemon == null || ballStartPoint == null || capturePoint == null)
        {
            Debug.LogError("PokemonCapture: assign a scene Poké Ball, Pokémon, start point and capture point in the Inspector.", this);
            enabled = false;
            return;
        }

        ballTransform = pokeball;
        pokemonTransform = pokemon.transform;
        initialized = true;

        pokeball.gameObject.SetActive(true);
        ballTransform.position = ballStartPoint.position;
        pokemon.gameObject.SetActive(true);

        if (captureEffectRoot != null)
        {
            ParticleSystem effect = captureEffectRoot.GetComponent<ParticleSystem>();
            if (effect != null) effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            captureEffectRoot.gameObject.SetActive(false);
        }

        Debug.Log("捕捉流程已就绪，按 Space 抛出精灵球。", this);
    }

    private void Update()
    {
        if (!initialized || !IsSceneObjectValid())
            return;

        if (!hasStarted && Input.GetKeyDown(throwKey))
        {
            hasStarted = true;
            StartCoroutine(ThrowPokeball());
        }
    }

    private IEnumerator ThrowPokeball()
    {
        if (!IsSceneObjectValid())
        {
            Debug.LogError("PokemonCapture: a required scene object was destroyed before the throw started.", this);
            yield break;
        }

        Vector3 start = ballStartPoint.position;
        Vector3 end = capturePoint != null ? capturePoint.position : pokemonTransform.position;
        float duration = Mathf.Max(0.1f, throwDuration);
        float timer = 0f;

        while (timer < duration)
        {
            if (!IsSceneObjectValid())
            {
                Debug.LogError("PokemonCapture: the Poké Ball or Pokémon was destroyed during the throw; capture stopped safely.", this);
                yield break;
            }

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y += 4f * throwHeight * t * (1f - t);
            ballTransform.position = pos;
            ballTransform.Rotate(Vector3.up, 500f * Time.deltaTime, Space.World);
            yield return null;
        }

        if (!IsSceneObjectValid()) yield break;
        ballTransform.position = end;
        pokemon.gameObject.SetActive(false);

        if (captureSuccessSound != null && captureAudioSource != null)
        {
            captureAudioSource.Stop();
            captureAudioSource.clip = captureSuccessSound;
            captureAudioSource.volume = captureSoundVolume;
            captureAudioSource.Play();
        }
        else if (captureSuccessSound == null)
        {
            Debug.LogWarning("PokemonCapture: 请在 Inspector 的 Capture Success Sound 栏指定音效素材。", this);
        }

        if (captureEffectRoot != null)
        {
            captureEffectRoot.gameObject.SetActive(true);
            captureEffectRoot.position = end;
            ParticleSystem effect = captureEffectRoot.GetComponent<ParticleSystem>();
            if (effect != null) effect.Play(true);
        }

        Debug.Log("收服成功！", this);
    }

    private bool IsSceneObjectValid()
    {
        return ballTransform != null && pokemonTransform != null && pokeball != null && pokemon != null;
    }

    private static GameObject FindSceneObject(params string[] names)
    {
        foreach (string name in names)
        {
            GameObject found = GameObject.Find(name);
            if (found != null) return found;
        }
        return null;
    }

    private static Transform FindSceneTransform(params string[] names)
    {
        GameObject found = FindSceneObject(names);
        return found != null ? found.transform : null;
    }
}
