using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckManager : MonoBehaviour
{
    public List<ChargeHazard> m_Chucks = new List<ChargeHazard>();
    public float m_MinDelay = 1f;
    public float m_MaxDelay = 3f;
    public bool m_AutoStart = true;
    
    [Header("Rounds")]
    public int m_Rounds = 5;

    [Header("Groups")]
    public int m_ChucksPerWave = 1;

    Coroutine m_Routine;

    void Start()
    {
        if (m_AutoStart) StartGame();
    }

    public void StartGame()
    {
        StopGame();
        m_Routine = StartCoroutine(GameRoutine());
    }

    public void StopGame()
    {
        if (m_Routine != null)
        {
            StopCoroutine(m_Routine);
            m_Routine = null;
        }
    }

    IEnumerator GameRoutine()
    {
        for (int round = 1; round <= m_Rounds; round++)
        {
            yield return new WaitForSeconds(Random.Range(m_MinDelay, m_MaxDelay));

            foreach (var chuck in PickChucks(m_ChucksPerWave))
                chuck.Charge();
        }

        m_Routine = null;
        Debug.Log("ChuckManager: all rounds done");
    }

    List<ChargeHazard> PickChucks(int count)
    {
        // Lines that already have a Chuck running
        var busyLines = new HashSet<int>();
        foreach (var c in m_Chucks)
            if (c.IsCharging) busyLines.Add(c.m_LineId);

        // Idle Chucks on free lines, in random order
        var candidates = new List<ChargeHazard>();
        foreach (var c in m_Chucks)
            if (!c.IsCharging && !busyLines.Contains(c.m_LineId))
                candidates.Add(c);
        Shuffle(candidates);

        // Take Chucks until we have enough, never two from the same line
        var picked = new List<ChargeHazard>();
        var usedLines = new HashSet<int>();
        foreach (var c in candidates)
        {
            if (picked.Count >= count) break;
            if (!usedLines.Add(c.m_LineId)) continue;
            picked.Add(c);
        }
        return picked;
    }

    static void Shuffle(List<ChargeHazard> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}