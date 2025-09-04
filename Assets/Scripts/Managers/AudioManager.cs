using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 音效管理器，控制背景音乐和音效播放
/// </summary>
public class AudioManager : Singleton<AudioManager>
{
    [Header("音频源")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("背景音乐")]
    [SerializeField] private AudioClip[] backgroundMusic;
    [SerializeField] private bool playMusicOnStart = true;
    
    [Header("音效")]
    [SerializeField] private AudioClip cardPlaceSound;
    [SerializeField] private AudioClip cardDrawSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip victorySound;
    [SerializeField] private AudioClip timerWarningSound;
    
    [Header("音量设置")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    
    // 音效字典，便于通过名称播放
    private Dictionary<string, AudioClip> soundEffects;
    
    // 属性
    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }
    
    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }
    
    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            UpdateVolumes();
        }
    }
    
    private void Start()
    {
        InitializeAudioManager();
    }
    
    /// <summary>
    /// 初始化音频管理器
    /// </summary>
    private void InitializeAudioManager()
    {
        // 创建音频源如果不存在
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music Source");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        // 初始化音效字典
        InitializeSoundEffects();
        
        // 设置音量
        UpdateVolumes();
        
        // 播放背景音乐
        if (playMusicOnStart && backgroundMusic.Length > 0)
        {
            PlayBackgroundMusic(0);
        }
    }
    
    /// <summary>
    /// 初始化音效字典
    /// </summary>
    private void InitializeSoundEffects()
    {
        soundEffects = new Dictionary<string, AudioClip>();
        
        if (cardPlaceSound != null) soundEffects["CardPlace"] = cardPlaceSound;
        if (cardDrawSound != null) soundEffects["CardDraw"] = cardDrawSound;
        if (buttonClickSound != null) soundEffects["ButtonClick"] = buttonClickSound;
        if (gameOverSound != null) soundEffects["GameOver"] = gameOverSound;
        if (victorySound != null) soundEffects["Victory"] = victorySound;
        if (timerWarningSound != null) soundEffects["TimerWarning"] = timerWarningSound;
    }
    
    /// <summary>
    /// 更新音量设置
    /// </summary>
    private void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = masterVolume * musicVolume;
        
        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;
    }
    
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="index">音乐索引</param>
    public void PlayBackgroundMusic(int index)
    {
        if (backgroundMusic != null && index >= 0 && index < backgroundMusic.Length)
        {
            if (musicSource.isPlaying)
                musicSource.Stop();
            
            musicSource.clip = backgroundMusic[index];
            musicSource.Play();
        }
    }
    
    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
    
    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBackgroundMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }
    
    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBackgroundMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }
    
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="clip">音效片段</param>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// 通过名称播放音效
    /// </summary>
    /// <param name="soundName">音效名称</param>
    public void PlaySFX(string soundName)
    {
        if (soundEffects.ContainsKey(soundName))
        {
            PlaySFX(soundEffects[soundName]);
        }
        else
        {
            Debug.LogWarning($"音效 '{soundName}' 未找到！");
        }
    }
    
    /// <summary>
    /// 播放卡牌放置音效
    /// </summary>
    public void PlayCardPlaceSound()
    {
        PlaySFX("CardPlace");
    }
    
    /// <summary>
    /// 播放卡牌抽取音效
    /// </summary>
    public void PlayCardDrawSound()
    {
        PlaySFX("CardDraw");
    }
    
    /// <summary>
    /// 播放按钮点击音效
    /// </summary>
    public void PlayButtonClickSound()
    {
        PlaySFX("ButtonClick");
    }
    
    /// <summary>
    /// 播放游戏结束音效
    /// </summary>
    public void PlayGameOverSound()
    {
        PlaySFX("GameOver");
    }
    
    /// <summary>
    /// 播放胜利音效
    /// </summary>
    public void PlayVictorySound()
    {
        PlaySFX("Victory");
    }
    
    /// <summary>
    /// 播放计时器警告音效
    /// </summary>
    public void PlayTimerWarningSound()
    {
        PlaySFX("TimerWarning");
    }
    
    /// <summary>
    /// 淡入背景音乐
    /// </summary>
    /// <param name="index">音乐索引</param>
    /// <param name="duration">淡入时间</param>
    public void FadeInMusic(int index, float duration = 1f)
    {
        if (backgroundMusic != null && index >= 0 && index < backgroundMusic.Length)
        {
            StartCoroutine(FadeInMusicCoroutine(index, duration));
        }
    }
    
    /// <summary>
    /// 淡出背景音乐
    /// </summary>
    /// <param name="duration">淡出时间</param>
    public void FadeOutMusic(float duration = 1f)
    {
        StartCoroutine(FadeOutMusicCoroutine(duration));
    }
    
    private IEnumerator FadeInMusicCoroutine(int index, float duration)
    {
        float targetVolume = masterVolume * musicVolume;
        
        if (musicSource.isPlaying)
            musicSource.Stop();
        
        musicSource.clip = backgroundMusic[index];
        musicSource.volume = 0f;
        musicSource.Play();
        
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
            yield return null;
        }
        
        musicSource.volume = targetVolume;
    }
    
    private IEnumerator FadeOutMusicCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null;
        }
        
        musicSource.volume = 0f;
        musicSource.Stop();
        musicSource.volume = masterVolume * musicVolume; // 重置音量
    }
}