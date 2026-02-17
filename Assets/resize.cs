//mogelijkheid om de grootte van de persoon in te geven en dan wordt de grootte
// aangepast in de scene 




using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResizeOnly : MonoBehaviour
{
    public TMP_InputField heightInput;
    public Button applyButton;
    public Transform targetObject; 

    public Renderer targetRenderer;

    void Start()
    {
        applyButton.onClick.AddListener(ApplyChanges);
    }

    void ApplyChanges()
    {
        // Probeer de invoer direct om te zetten naar een float
        if (float.TryParse(heightInput.text, out float newHeight))
        {
            // Huidige hoogte
            float currentHeight = targetRenderer.bounds.size.y;

            // Bereken schaalfactor
            float scaleFactor = newHeight / currentHeight;

            // Uniform schalen
            targetObject.localScale *= scaleFactor;
        

            Debug.Log($"Oude hoogte: {currentHeight}, Nieuwe hoogte: {newHeight}, Schaalfactor: {scaleFactor}");
        }
        else
        {
            Debug.LogWarning("Ongeldige invoer! Voer een numerieke waarde in.");
        }
    }
}