using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance { get; private set; }

    public Vector2 MoveInput { get; private set; }
    public bool ShootInput { get; private set; }
    public bool ShieldInput { get; private set; }
    public bool PauseInput { get; private set; }
    public bool UnpauseInput { get; private set; }

    //private PlayerInput playerInput;
    public static PlayerInput PlayerInput;

    private InputAction moveAction;
    private InputAction shootAction;
    private InputAction shieldAction;
    private InputAction pauseAction;
    private InputAction unpauseAction;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        PlayerInput = GetComponent<PlayerInput>();

        // Acessa as actions através do currentActionMap
        moveAction = PlayerInput.actions["Move"];
        shootAction = PlayerInput.actions["Shoot"];
        shieldAction = PlayerInput.actions["Shield"];
        pauseAction = PlayerInput.actions["PauseGame"];
        unpauseAction = PlayerInput.actions["UnpauseGame"];
    }

    private void Update()
    {
        MoveInput = moveAction.ReadValue<Vector2>();
        ShootInput = shootAction.IsPressed();
        ShieldInput = shieldAction.IsPressed();
        PauseInput = pauseAction.WasPressedThisFrame();
        UnpauseInput = unpauseAction.WasPressedThisFrame();
    }
}