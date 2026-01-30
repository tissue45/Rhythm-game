using UnityEngine;
using System.Collections.Generic;

public class GalaxiasNoteSpawner : MonoBehaviour
{
    public AudioSource music; // Galaxias! 음악
    public GameObject notePrefab; // 복제할 노트 디자인
    
    // 채보 데이터: {시간, 라인 번호}
    private float[] spawnTimes = { 1.2f, 2.4f, 3.6f, 4.2f }; 
    private int[] laneIndices = { 0, 3, 1, 2 }; // 0=D, 1=F, 2=J, 3=K
    
    private int currentIdx = 0;

    void Update()
    {
        // 음악 시간에 맞춰 노트를 하나씩 꺼냅니다.
        if (currentIdx < spawnTimes.Length && music.time >= spawnTimes[currentIdx])
        {
            Spawn(laneIndices[currentIdx]);
            currentIdx++;
        }
    }

    void Spawn(int lane)
    {
        // 여기서 실제로 노트 오브젝트를 생성하고 아래로 떨어뜨립니다.
        Debug.Log(lane + "번 라인에 노트 생성!");
    }
}
