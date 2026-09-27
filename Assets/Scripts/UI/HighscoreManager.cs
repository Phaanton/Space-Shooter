using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HighscoreEntry
{
    public string playerName;
    public int score;

    public HighscoreEntry(string name, int score)
    {
        playerName = name;
        this.score = score;
    }
}

public class HighscoreManager : MonoBehaviour
{
    public static HighscoreManager Instance;

    [SerializeField] private int maxEntries = 10;
    private List<HighscoreEntry> highscoreList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHighscores();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Verifica se o score está entre os 10 melhores
    public bool IsNewHighscore(int score)
    {
        if (highscoreList.Count < maxEntries)
            return true;

        // Verifica se o score é maior que o último colocado
        return score > highscoreList[highscoreList.Count - 1].score;
    }

    // Adiciona novo highscore
    public void AddHighscore(string playerName, int score)
    {
        HighscoreEntry newEntry = new HighscoreEntry(playerName, score);
        highscoreList.Add(newEntry);

        // Ordena por score (maior para menor)
        highscoreList.Sort((a, b) => b.score.CompareTo(a.score));

        // Mantém apenas os top 10
        if (highscoreList.Count > maxEntries)
        {
            highscoreList.RemoveRange(maxEntries, highscoreList.Count - maxEntries);
        }

        SaveHighscores();
    }

    // Retorna a lista de highscores
    public List<HighscoreEntry> GetHighscores()
    {
        return highscoreList;
    }

    // Salva no PlayerPrefs
    private void SaveHighscores()
    {
        string json = JsonUtility.ToJson(new Wrapper<HighscoreEntry> { items = highscoreList });
        PlayerPrefs.SetString("Highscores", json);
        PlayerPrefs.Save();
    }

    // Carrega do PlayerPrefs
    private void LoadHighscores()
    {
        if (PlayerPrefs.HasKey("Highscores"))
        {
            string json = PlayerPrefs.GetString("Highscores");
            Wrapper<HighscoreEntry> wrapper = JsonUtility.FromJson<Wrapper<HighscoreEntry>>(json);
            highscoreList = wrapper.items ?? new List<HighscoreEntry>();
        }
        else
        {
            highscoreList = new List<HighscoreEntry>();
        }
    }

    // Classe auxiliar para serialização
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }
}
