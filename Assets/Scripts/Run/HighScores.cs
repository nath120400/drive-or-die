using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// The leaderboard: the best runs across sessions, kept in a small JSON file
// under the persistent data path, capped to the five best scores
public static class HighScores
{
    private const int MaxEntries = 5;
    private const string FileName = "highscores.json";

    [Serializable]
    public class Entry
    {
        public float Score;
        public string Date;
    }

    [Serializable]
    private class Data
    {
        public List<Entry> Entries = new List<Entry>();
    }

    public static List<Entry> Load()
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, FileName);
        if (!System.IO.File.Exists(path))
        {
            return new List<Entry>();
        }

        return JsonUtility.FromJson<Data>(System.IO.File.ReadAllText(path)).Entries;
    }

    // The best score so far: zero when the board is empty
    public static float BestScore()
    {
        List<Entry> entries = Load();
        return entries.Count > 0 ? entries[0].Score : 0f;
    }

    // The board as display text: "1. 1240 — 14/09/2026"
    public static string Format()
    {
        List<Entry> entries = Load();

        StringBuilder board = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            board.AppendLine($"{entries[i].Date} - {entries[i].Score:F0}");
        }

        return board.ToString();
    }

    public static void Submit(float score)
    {
        string path = System.IO.Path.Combine(Application.persistentDataPath, FileName);
        List<Entry> entries = Load();

        entries.Add(new Entry { Score = score, Date = DateTime.Now.ToString("dd/MM/yyyy") });
        entries.Sort((a, b) => b.Score.CompareTo(a.Score));

        // Only the five best survive
        if (entries.Count > MaxEntries)
        {
            entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
        }

        System.IO.File.WriteAllText(path, JsonUtility.ToJson(new Data { Entries = entries }, prettyPrint: true));
    }
}
