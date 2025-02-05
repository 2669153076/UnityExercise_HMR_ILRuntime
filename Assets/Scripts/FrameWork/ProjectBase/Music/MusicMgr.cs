using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Custom.BaseFramework
{
    public class MusicMgr : BaseManager<MusicMgr>
    {
        private AudioSource bkMusic = null;
        private float bkMusicVolume = 1;

        private GameObject soundObj = null;
        private List<AudioSource> soundList = new List<AudioSource>();
        private float soundVolume = 1;

        public MusicMgr()
        {
            MonoMgr.GetInstance().AddUpdateListener(Update);
        }
        private void Update()
        {
            for (int i = soundList.Count - 1; i >= 0; i--)
            {
                if (soundList[i].isPlaying)
                {
                    GameObject.Destroy(soundList[i]);
                    soundList.RemoveAt(i);

                }
            }
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        /// <param name="name"></param>
        public void PlayBkMusic(string name)
        {
            if (bkMusic == null)
            {
                GameObject obj = new GameObject("bkMusic");
                bkMusic = obj.AddComponent<AudioSource>();
            }
            ResMgr.GetInstance().LoadAsync<AudioClip>("Music/Bk/" + name, (clip) =>
            {
                bkMusic.clip = clip;
                bkMusic.loop = true;
                bkMusic.Play();
            });
        }
        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void PauseMusic()
        {
            if (bkMusic == null)
            {
                return;
            }
            bkMusic.Pause();
        }
        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void StopBkMusic()
        {
            if (bkMusic == null)
            {
                return;
            }
            bkMusic.Stop();
        }
        /// <summary>
        /// 改变背景音乐音量大小
        /// </summary>
        /// <param name="value"></param>
        public void ChangeBkVolume(float value)
        {
            bkMusicVolume = value;
            if (bkMusic == null)
            {
                return;
            }
            bkMusic.volume = bkMusicVolume;
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        /// <param name="name"></param>
        public void PlaySound(string name, bool isLoop = false, UnityAction<AudioSource> callback = null)
        {
            if (soundObj == null)
            {
                soundObj = new GameObject("Sound");

            }
            ResMgr.GetInstance().LoadAsync<AudioClip>("Music/Sound/" + name, (clip) =>
            {

                AudioSource source = soundObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.loop = isLoop;
                source.volume = bkMusicVolume;
                source.Play();
                soundList.Add(source);
                callback?.Invoke(source);
            });
        }
        //停止音效
        public void StopSound(AudioSource source)
        {
            if (soundList.Contains(source))
            {
                soundList.Remove(source);
                source.Stop();
                GameObject.Destroy(source);
            }
        }
        public void ChangeSoundVolume(float value)
        {
            soundVolume = value;
            foreach (var item in soundList)
            {
                item.volume = soundVolume;
            }
        }
    }
}