using UnityEngine;
using TMPro;
using UnityEngine.UI; 

public class MiniConsola : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public TMP_Text outputText;
    public ScrollRect scrollRect; 

    // Estados de la consola (Cisco Modes)
    private enum ConsoleState
    {
        UserMode,       // Switch>
        PrivilegedMode, // Switch#
        GlobalConfig,   // Switch(config)#
        InterfaceConfig // Switch(config-if)#
    }

    private ConsoleState currentState;

    private void Start()
    {
        currentState = ConsoleState.UserMode;
        outputText.text = "";
        
        // Mensaje de bienvenida inicial
        AddLine("Cisco IOS Software, C2960 Software (C2960-LANBASEK9-M)");
        AddLine("System Init... OK.");
        AddLine("<color=yellow>NODE: ¡Alex! Usa el comando 'help' si no sabes qué hacer.</color>"); // Pista inicial
        AddLine("");
        AddLine(GetPrompt()); 

        inputField.onSubmit.AddListener(ProcessCommand);
        inputField.ActivateInputField();
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

    void ProcessCommand(string cmd)
    {
        string cleanCmd = cmd.ToLower().Trim();

        // --- 1. COMANDO GLOBAL: CLEAR (Funciona siempre) ---
        if (cleanCmd == "clear" || cleanCmd == "cls")
        {
            outputText.text = "";
            AddLine(GetPrompt()); // Vuelve a poner el prompt limpio
            ResetInput();
            return;
        }

        // Mostramos lo que escribió el usuario (si no fue clear)
        AddLine(GetPrompt() + " " + cmd);

        // --- 2. COMANDO GLOBAL: HELP (Ayuda inteligente) ---
        if (cleanCmd == "help" || cleanCmd == "?")
        {
            ProvideHint(); // Llamamos a la función de pistas
            ResetInput();
            return;
        }

        // --- 3. MÁQUINA DE ESTADOS (Lógica del juego) ---
        switch (currentState)
        {
            // NIVEL 1: >
            case ConsoleState.UserMode:
                if (cleanCmd == "enable" || cleanCmd == "en")
                {
                    currentState = ConsoleState.PrivilegedMode;
                }
                else
                {
                    ShowError();
                }
                break;

            // NIVEL 2: #
            case ConsoleState.PrivilegedMode:
                if (cleanCmd == "conf t" || cleanCmd == "configure terminal")
                {
                    AddLine("Enter configuration commands, one per line. End with CNTL/Z.");
                    currentState = ConsoleState.GlobalConfig;
                }
                else if (cleanCmd == "exit")
                {
                    currentState = ConsoleState.UserMode;
                }
                else
                {
                    ShowError();
                }
                break;

            // NIVEL 3: (config)#
            case ConsoleState.GlobalConfig:
                if (cleanCmd == "int fa0/1" || cleanCmd == "interface fastethernet 0/1")
                {
                    currentState = ConsoleState.InterfaceConfig;
                }
                else if (cleanCmd == "exit")
                {
                    currentState = ConsoleState.PrivilegedMode;
                }
                else
                {
                    ShowError();
                }
                break;

            // NIVEL 4: (config-if)#
            case ConsoleState.InterfaceConfig:
                if (cleanCmd == "switchport access vlan 10")
                {
                    // ¡VICTORIA!
                    AddLine("Changes applied.");
                    AddLine("% LINK-3-UPDOWN: Interface FastEthernet0/1, changed state to up");
                    AddLine("<color=green>NODE: ¡Excelente! El servidor ya está visible en la VLAN 10.</color>");
                    WinLevel();
                }
                else if (cleanCmd == "exit")
                {
                    currentState = ConsoleState.GlobalConfig;
                }
                else
                {
                    ShowError();
                }
                break;
        }

        ResetInput();
    }

    // Función que da pistas según dónde estés atascado
    void ProvideHint()
    {
        string hint = "";
        
        switch (currentState)
        {
            case ConsoleState.UserMode:
                hint = "NODE: Necesitamos permisos de administrador. Escribe: <color=yellow>enable</color>";
                break;
            case ConsoleState.PrivilegedMode:
                hint = "NODE: Entra al modo de configuración global. Escribe: <color=yellow>conf t</color>";
                break;
            case ConsoleState.GlobalConfig:
                hint = "NODE: Selecciona el puerto donde está el servidor. Escribe: <color=yellow>int fa0/1</color>";
                break;
            case ConsoleState.InterfaceConfig:
                hint = "NODE: Mueve este puerto a la VLAN de administración. Escribe: <color=yellow>switchport access vlan 10</color>";
                break;
        }
        
        AddLine(hint);
    }

    void ShowError()
    {
        AddLine("% Invalid input detected at '^' marker.");
        AddLine("(Tip: Escribe 'help' si necesitas ayuda)");
    }

    void AddLine(string line)
    {
        outputText.text += line + "\n";
        
        // Auto-scroll
        if(scrollRect != null) 
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    void ResetInput()
    {
        inputField.text = "";
        inputField.ActivateInputField();
    }

    void WinLevel()
    {
        Debug.Log("¡NIVEL COMPLETADO!");
        // Aquí activas tu UI de victoria o cargas la siguiente escena
    }
}