using UnityEngine;
using TMPro;

public class MiniConsola : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;  // Donde escribe el jugador
    [SerializeField] private TMP_Text outputText;        // Donde aparece la consola

    void Start()
    {
        // Limpiamos el input y texto de salida al iniciar
        inputField.text = "";
        outputText.text = "Bienvenido a la consola!\nEscribe 'hola' o 'ayuda'\n";

        // Detecta cuando presionen Enter en el InputField
        inputField.onSubmit.AddListener(ProcessInput);
    }

    // Función que procesa lo que escribe el jugador
    void ProcessInput(string input)
    {
        inputField.text = "";  // Limpiamos el input
        outputText.text += "> " + input + "\n";  // Mostramos el comando escrito

        // Respuesta según el comando
        string response = GetResponse(input);
        outputText.text += response + "\n";

        // Fuerza a que el Scroll se actualice si usas Scroll Rect
        Canvas.ForceUpdateCanvases();
        inputField.ActivateInputField();  // Mantener el input activo
    }

    // Función simple que devuelve respuesta según el comando
    string GetResponse(string input)
    {
        input = input.ToLower();  // Convertimos a minúsculas para comparar

        if (input == "hola")
            return "¡Hola, jugador!";
        else if (input == "ayuda")
            return "Comandos disponibles: hola, ayuda, estado";
        else if (input == "estado")
            return "Estado: Vida=100, Puntos=0";
        else
            return "Comando no reconocido.";
    }
}

