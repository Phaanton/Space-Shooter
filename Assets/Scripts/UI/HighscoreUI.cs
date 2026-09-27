using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class HighscoreUI : MonoBehaviour
{
    [SerializeField] private Transform highscoreContainer;
    [SerializeField] private GameObject highscoreEntryPrefab; // Prefab com TextMeshProUGUI para nome e score

    [System.Serializable]
    public class HighscoreSlot
    {
        public TextMeshProUGUI positionText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI scoreText;
    }

    [SerializeField] private HighscoreSlot[] highscoreSlots; // Se você tem slots fixos na UI

    private void OnEnable()
    {
        UpdateHighscoreTable();
    }

    public void UpdateHighscoreTable()
    {
        List<HighscoreEntry> highscores = HighscoreManager.Instance.GetHighscores();

        // Se estiver usando slots fixos
        for (int i = 0; i < highscoreSlots.Length; i++)
        {
            if (i < highscores.Count)
            {
                highscoreSlots[i].positionText.text = (i + 1).ToString();
                highscoreSlots[i].nameText.text = highscores[i].playerName;
                highscoreSlots[i].scoreText.text = highscores[i].score.ToString();
            }
            else
            {
                highscoreSlots[i].positionText.text = (i + 1).ToString();
                highscoreSlots[i].nameText.text = "---";
                highscoreSlots[i].scoreText.text = "0";
            }
        }
    }
}
