using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public GameObject background;
    public GameObject[] screens;
    public GameObject pauseMenu;
    public GameObject startScreen;
    public GameObject gameplayScreen;

    [SerializeField] private NameInputScreen nameInputScreen;
    [SerializeField] private int currentScore = 0;
    public TMP_Text scoreText;

    [SerializeField] private AudioSource mainMenuSoundtrack;
    [SerializeField] private AudioSource gameplaySoundtrack;

    [SerializeField] public Image[] shieldIcons; // Array de imagens para os ícones de escudo
    [SerializeField] public Image[] engineIcons; // Array de imagens para os ícones de vida
    [SerializeField] public GameObject shieldBox;
    [SerializeField] public GameObject engineBox;

    [SerializeField] private GameObject ui;

    private PlayerController playerController;

    [SerializeField] private GameObject pauseBackground;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        shieldIcons = shieldBox.GetComponentsInChildren<Image>();
        engineIcons = engineBox.GetComponentsInChildren<Image>();
    }

    void Start()
    {
        // Ativa a tela de configurações para garantir que os sliders sejam inicializados
        screens[1].SetActive(true);
        
        // Encontra o SliderController
        SliderController sliderController = FindObjectOfType<SliderController>(true);

        if (sliderController != null)
        {
            // Força a aplicação dos volumes
            sliderController.SetMasterVolume();
            sliderController.SetMusicVolume();
            sliderController.SetSFXVolume();
        }

        // Agora toca a música com os volumes já configurados
        mainMenuSoundtrack.Play();
        gameplaySoundtrack.Stop();

        screens[1].SetActive(false);
        pauseBackground.SetActive(false);
    }

    private void Update()
    {
        if (InputManager.instance.PauseInput)
        {
            if (!PauseManager.instance.IsPaused)
            {
                Pause();
            }
        }
        else if (InputManager.instance.UnpauseInput)
        {
            if (PauseManager.instance.IsPaused)
            {
                Unpause();
            }
        }
    }

    public void Pause()
    {
        PauseManager.instance.PauseGame();
        SwitchToScreen(5);
        pauseBackground.SetActive(true);
    }

    public void Unpause()
    {
        PauseManager.instance.UnpauseGame();
        SwitchToScreen(4);
        pauseBackground.SetActive(false);
    }

    public void OnNewGamePress()
    {
        SwitchToScreen(4);
        background.SetActive(false);
        gameplayScreen.SetActive(true);
        SceneManager.LoadScene("Gameplay");
        mainMenuSoundtrack.Stop();
        gameplaySoundtrack.Play();
        currentScore = 0;

        StartCoroutine(FindPlayerAfterSceneLoad());
    }

    private void HideAllScreens()
    {
        for (int i = 0; i < screens.Length; i++)
        {
            screens[i].SetActive(false);
        }
    }

    public void OnRestartPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnSettingsPress()
    {
        // L�gica para abrir as configura��es
        SwitchToScreen(1);
        pauseBackground.SetActive(PauseManager.instance.IsPaused);
    }

    public void OnHighscorePress()
    {
        // L�gica para abrir a tabela de highscores
        SwitchToScreen(2);
    }

    public void OnCreditsPress()
    {
        // L�gica para abrir a tabela de highscores
        SwitchToScreen(3);
    }

    public void OnGameResumePress()
    {
        pauseMenu.SetActive(false);
    }

    public void OnGameExitPress()
    {
        Application.Quit();
    }

    public void OnBackToMenuPress()
    {
        if (!PauseManager.instance.IsPaused)
        {
            SwitchToScreen(0);
        }
        else
        {
            SwitchToScreen(5);
        }
    }

    public void OnEnterPause()
    {
        pauseMenu.SetActive(true);

    }

    public void UpdateScore(int score)
    {
        currentScore += score;
        scoreText.text = currentScore.ToString("D7");
    }

    public void OnGameOver()
    {
        // currentScore � sua pontua��o atual
        if (HighscoreManager.Instance.IsNewHighscore(currentScore))
        {
            if (!PauseManager.instance.IsPaused)
            {
                PauseManager.instance.PauseGame();
            }

            // Mostra tela de inser��o de nome
            nameInputScreen.ShowNameInput(currentScore);
        }
        else
        {
            // Vai direto para a tabela ou menu
            gameplaySoundtrack.Stop();
            LoadMainMenuScene();
        }
    }

    private void SwitchToScreen(int screenIndex)
    {
        // Encontra a tela ativa atual
        GameObject activeScreen = null;
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i].activeSelf)
            {
                activeScreen = screens[i];
                break;
            }
        }

        // Reseta a tela ativa se existir
        if (activeScreen != null)
        {
            ResetScreenUI(activeScreen);
        }

        // Troca as telas
        HideAllScreens();
        screens[screenIndex].SetActive(true);
    }

    private void ResetScreenUI(GameObject screen)
    {
        if (screen == null) return;

        // 1. Reseta todos os bot�es
        Button[] buttons = screen.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            // For�a o bot�o a sair de qualquer estado de sele��o
            if (EventSystem.current != null)
            {
                // Se este bot�o est� selecionado, deseleciona
                if (EventSystem.current.currentSelectedGameObject == button.gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }

            // For�a a execu��o dos eventos de sa�da
            button.OnPointerExit(new PointerEventData(EventSystem.current));
            button.OnDeselect(null);

            // Reseta o Animator se existir
            Animator animator = button.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                // Reseta para o estado Normal
                animator.Play("Normal", 0, 0f);
                animator.Update(0f);
            }
        }

        // 2. Reseta todos os elementos com animator
        Animator[] animators = screen.GetComponentsInChildren<Animator>(true);
        foreach (Animator anim in animators)
        {
            if (anim.runtimeAnimatorController != null)
            {
                // Tenta encontrar o estado Normal ou o primeiro estado
                if (anim.HasState(0, Animator.StringToHash("Normal")))
                {
                    anim.Play("Normal", 0, 0f);
                }
                else
                {
                    // Se n�o tem estado Normal, para a anima��o e reseta
                    anim.Play(0, 0, 0f);
                }
                anim.Update(0f);
            }
        }

        // 3. Reseta CanvasGroup se estiver usando para transpar�ncia
        CanvasGroup[] canvasGroups = screen.GetComponentsInChildren<CanvasGroup>(true);
        foreach (CanvasGroup cg in canvasGroups)
        {
            // Preserva a configura��o original se necess�rio
            // Isso depende da sua configura��o inicial
            if (cg.gameObject.GetComponent<Button>() == null)
            {
                // Se n�o for um bot�o, pode ser que precise resetar
                // Ajuste conforme sua necessidade
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    private IEnumerator FindPlayerAfterSceneLoad()
    {
        // Aguarda um frame para a cena começar a carregar
        yield return null;

        // Tenta encontrar o PlayerController na nova cena
        playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            // Reconecta os eventos
            playerController.OnDropScore += UpdateScore;
            playerController.OnShieldActivation += UpdateShiledsUI;
            playerController.OnLoseHelath += UpdateEnginesUI;

            shieldIcons[0].enabled = true;
            shieldIcons[1].enabled = true;
            shieldIcons[2].enabled = true;

            engineIcons[0].enabled = true;
            engineIcons[1].enabled = true;
            engineIcons[2].enabled = true;
        }
        else
        {
            Debug.LogError("PlayerController não encontrado na cena de gameplay!");
        }
    }

    private void UpdateShiledsUI(int obj)
    {
        shieldIcons[0].enabled = obj >= 1; // Ativa o primeiro ícone se tiver pelo menos 1 escudo
        shieldIcons[1].enabled = obj >= 2; // Ativa o segundo ícone se tiver pelo menos 2 escudos
        shieldIcons[2].enabled = obj >= 3; // Ativa o terceiro ícone se tiver pelo menos 3 escudos
    }

    private void UpdateEnginesUI(int obj)
    {
        engineIcons[2].enabled = obj >= 1; // Ativa o primeiro ícone se tiver pelo menos 1 vida
        engineIcons[1].enabled = obj >= 2; // Ativa o segundo ícone se tiver pelo menos 2 vidas
        engineIcons[0].enabled = obj >= 3; // Ativa o terceiro ícone se tiver pelo menos 3 vidas
    }

    public void StopAllSountrack()
    {
        mainMenuSoundtrack.Stop();
        gameplaySoundtrack.Stop();
    }

    public void LoadMainMenuScene()
    {
        Destroy(ui);
        Destroy(gameObject);
        SceneManager.LoadScene("MainMenu");

        if (PauseManager.instance.IsPaused)
        {
            Unpause();
        }
    }
}