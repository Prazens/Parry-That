using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource effectSound;

    [SerializeField] private Queue<int> playQueue;
    
    void Start()
    {
        InitializeSound();
    }
    
    public void InitializeSound()
    {
        playQueue = new Queue<int>();
    }

    public void Update()
    {
        if (playQueue.Count != 0)
        {
            while (playQueue.Count != 0)
            {
                playQueue.Dequeue();
                effectSound.Play();
            }
        }
    }

    public void AddMusic(int type)
    {
        playQueue.Enqueue(type);
    }
}
