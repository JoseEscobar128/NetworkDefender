using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniConsola : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public ScrollRect scrollRect;

    [Header("Configuración Juego")]
    public string nombreEscenaMapa = "Mapa";

    private enum ConsoleState
    {
        UserMode,       // Switch>
        PrivilegedMode, // Switch#
        GlobalConfig,   // Switch(config)#
        InterfaceConfig // Switch(config-if)#
    }

    private ConsoleState currentState;
    private bool nivelCompletado = false;

    private void Start()
    {
        currentState = ConsoleState.UserMode;
        outputText.text = "";

        // Mensaje inicial
        AddLine("Cisco IOS Software, C2960 Software (C2960-LANBASEK9-M)");
        AddLine("System Init... OK.");
        AddLine("");
        AddNodeMessage("¡Alex! El servidor está aislado. Usa el comando 'help' si no sabes qué hacer.");
        AddLine("");

        inputField.onSubmit.AddListener(ProcessCommand);
        inputField.ActivateInputField();

        ScrollToBottom();
    }

    string GetPrompt()
    {
        switch (currentState)
        {
            case ConsoleState.UserMode: return "Switch>";
            case ConsoleState.PrivilegedMode: return "Switch#";
            case ConsoleState.GlobalConfig: return "Switch(config)#";
            case ConsoleState.InterfaceConfig: return "Switch(config-if)#";
            default: return "Switch>";
        }
    }

    // --- AUTOSCROLL ---
    public void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            StartCoroutine(ForceScrollDown());
        }
    }

    IEnumerator ForceScrollDown()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
    }

    void ProcessCommand(string cmd)
    {
        if (nivelCompletado) return;

        string cleanCmd = cmd.ToLower().Trim();

        AddLine(GetPrompt() + " " + cmd);

        if (string.IsNullOrEmpty(cleanCmd))
        {
            ResetInput();
            return;
        }

        // COMANDO CLEAR
        if (cleanCmd == "clear" || cleanCmd == "cls")
        {
            outputText.text = "";
            ResetInput();
            return;
        }

        // COMANDO HELP
        if (cleanCmd == "help" || cleanCmd == "?")
        {
            ProvideHint();
            AddLine(GetPrompt()); // nuevo prompt después del help
            ResetInput();
            return;
        }

        bool comandoValido = false;

        // ------------------------
        //      LÓGICA DE ESTADOS
        // ------------------------
        switch (currentState)
        {
            case ConsoleState.UserMode:
                if (cleanCmd == "enable" || cleanCmd == "en")
                {
                    currentState = ConsoleState.PrivilegedMode;
                    comandoValido = true;
                }
                break;

            case ConsoleState.PrivilegedMode:
                if (cleanCmd == "conf t" || cleanCmd == "configure terminal")
                {
                    AddLine("Enter configuration commands, one per line. End with CNTL/Z.");
                    currentState = ConsoleState.GlobalConfig;
                    comandoValido = true;
                }
                else if (cleanCmd == "exit")
                {
                    currentState = ConsoleState.UserMode;
                    comandoValido = true;
                }
                break;

            case ConsoleState.GlobalConfig:
                if (cleanCmd == "int fa0/1" || cleanCmd == "interface fastethernet 0/1")
                {
                    currentState = ConsoleState.InterfaceConfig;
                    comandoValido = true;
                }
                else if (cleanCmd == "exit")
                {
                    currentState = ConsoleState.PrivilegedMode;
                    comandoValido = true;
                }
                break;

            case ConsoleState.InterfaceConfig:
                if (cleanCmd == "switchport access vlan 10")
                {
                    comandoValido = true;
                    nivelCompletado = true;
                    AddLine("Changes applied.");
                    AddLine("% LINK-3-UPDOWN: Interface FastEthernet0/1, changed state to up");
                    AddNodeMessage("¡Excelente trabajo! Conexión restaurada.");

                    PlayerPrefs.SetInt("current_level", 1);
                    PlayerPrefs.Save();


                    AddLine("<color=orange>Redirigiendo al mapa...</color>");
                    StartCoroutine(CargarMapaSequence());
                }
                else if (cleanCmd == "exit")
                {
                    currentState = ConsoleState.GlobalConfig;
                    comandoValido = true;
                }
                break;
        }

        // VALIDACIÓN
        if (!comandoValido)
        {
            ShowError();
        }
        else
        {
            // Imprime el nuevo prompt automáticamente (si no ganó)
            if (!nivelCompletado)
                AddLine(GetPrompt());
        }

        ResetInput();
    }

    IEnumerator CargarMapaSequence()
    {
        inputField.DeactivateInputField();
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene(nombreEscenaMapa);
    }

    void AddNodeMessage(string message)
    {
        AddLine($"<color=#00FFFF>NODE: {message}</color>");
    }

    // --- HELP AVANZADO (NODE) ---
    void ProvideHint()
    {
        switch (currentState)
        {
            case ConsoleState.UserMode:
                AddNodeMessage(
                    "Ahora estás en modo usuario: solo puedes ver información básica.\n" +
                    "Debes escribir <color=yellow>enable</color> para entrar al modo privilegiado, donde podrás comenzar configuraciones."
                );
                break;

            case ConsoleState.PrivilegedMode:
                AddNodeMessage(
                    "Estás en modo privilegiado: ya puedes ver y controlar más cosas del switch.\n" +
                    "Escribe <color=yellow>conf t</color> para entrar al modo de configuración global."
                );
                break;

            case ConsoleState.GlobalConfig:
                AddNodeMessage(
                    "Modo configuración global: aquí defines qué parte del switch quieres configurar.\n" +
                    "Escribe <color=yellow>int fa0/1</color> para editar la interfaz FastEthernet 0/1."
                );
                break;

            case ConsoleState.InterfaceConfig:
                AddNodeMessage(
                    "Estás configurando la interfaz FastEthernet 0/1.\n" +
                    "Para asignar la VLAN correcta, escribe <color=yellow>switchport access vlan 10</color>."
                );
                break;
        }
    }

    void ShowError()
    {
        AddLine("% Invalid input detected at '^' marker.");
    }

    void AddLine(string line)
    {
        outputText.text += line + "\n";
        ScrollToBottom();
    }

    void ResetInput()
    {
        inputField.text = "";
        inputField.ActivateInputField();
        ScrollToBottom();
    }
}
