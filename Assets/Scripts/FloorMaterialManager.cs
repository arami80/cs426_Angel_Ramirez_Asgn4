using UnityEngine;

public class FloorMaterialManager : MonoBehaviour
{
    public Renderer floorRenderer; // Reference to the floor renderer
    public Material[] floorMaterials; // Array of floor materials for each question

    public void UpdateFloorMaterial(int questionIndex)
    {
        if (floorRenderer != null && floorMaterials.Length > questionIndex)
        {
            floorRenderer.material = floorMaterials[questionIndex];
        }
    }
}

// This chunk of code goes in QuizGame

// private void UpdateFloorMaterial()
// {
//     if (floorMaterialManager != null)
//     {
//         floorMaterialManager.UpdateFloorMaterial(currentQuestionIndex);
//     }
// }
