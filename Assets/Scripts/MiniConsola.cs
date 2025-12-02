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
        
        // NO imprimimos el prompt aquí abajo todavía, 
        // para que la primera línea la ponga el usuario al escribir.
        
        inputField.onSubmit.AddListener(ProcessCommand);
        inputField.ActivateInputField();
        
        // Forzamos el scroll al inicio por si acaso
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

    // --- CORRECCIÓN DEL AUTOSCROLL ---
    public void ScrollToBottom()
    {
        // Forzamos a Unity a actualizar los lienzos (Canvas) YA MISMO
        // Esto recalcula el tamaño del texto antes de hacer el scroll.
        if (scrollRect != null)
        {
            StartCoroutine(ForceScrollDown());
        }
    }

    IEnumerator ForceScrollDown()
    {
        // Esperamos al final del frame para asegurar que el texto se pintó
        yield return new WaitForEndOfFrame();
        
        // Forzamos actualización de layout
        Canvas.ForceUpdateCanvases();
        
        // Mandamos el scroll al fondo (0 es abajo, 1 es arriba)
        scrollRect.verticalNormalizedPosition = 0f;
        
        // A veces se necesita un segundo empujón frame siguiente
        Canvas.ForceUpdateCanvases();
    }

    void ProcessCommand(string cmd)
    {
        if (nivelCompletado) return;

        // Limpiamos el comando (quitar espacios, minúsculas)
        string cleanCmd = cmd.ToLower().Trim();

        // 1. IMPRIMIR LO QUE EL USUARIO ESCRIBIÓ (Efecto Consola)
        // Esto pone "Switch> enable" en la pantalla
        AddLine(GetPrompt() + " " + cmd);

        // --- CORRECCIÓN ENTER VACÍO ---
        // Si el usuario solo dio Enter sin escribir nada...
        if (string.IsNullOrEmpty(cleanCmd))
        {
            // No hacemos nada, solo limpiamos el input.
            // Al haber hecho AddLine arriba, ya se imprimió el "Switch>" nuevo.
            ResetInput();
            return;
        }

        // --- COMANDOS GLOBALES ---
        if (cleanCmd == "clear" || cleanCmd == "cls")
        {
            outputText.text = "";
            // No imprimimos prompt extra, el input lo hará al escribir
            ResetInput();
            return;
        }

        if (cleanCmd == "help" || cleanCmd == "?")
        {
            ProvideHint();
            ResetInput();
            return;
        }

        // --- LÓGICA DE ESTADOS ---
        bool comandoValido = false; // Para saber si mostramos error o no

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
                    // VICTORIA
                    comandoValido = true;
                    nivelCompletado = true;
                    AddLine("Changes applied.");
                    AddLine("% LINK-3-UPDOWN: Interface FastEthernet0/1, changed state to up");
                    AddNodeMessage("¡Excelente trabajo! Conexión restaurada.");
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

        // Si escribió algo pero no era válido para el estado actual
        if (!comandoValido)
        {
            ShowError();
        }

        // Limpiamos y bajamos el scroll
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

    void ProvideHint()
    {
        switch (currentState)
        {
            case ConsoleState.UserMode:
                AddNodeMessage("Escribe: <color=yellow>enable</color>");
                break;
            case ConsoleState.PrivilegedMode:
                AddNodeMessage("Escribe: <color=yellow>conf t</color>");
                break;
            case ConsoleState.GlobalConfig:
                AddNodeMessage("Escribe: <color=yellow>int fa0/1</color>");
                break;
            case ConsoleState.InterfaceConfig:
                AddNodeMessage("Escribe: <color=yellow>switchport access vlan 10</color>");
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
        ScrollToBottom(); // Llamamos al scroll mejorado
    }

    void ResetInput()
    {
        inputField.text = "";
        inputField.ActivateInputField();
        ScrollToBottom(); // Aseguramos scroll también al resetear
    }
}