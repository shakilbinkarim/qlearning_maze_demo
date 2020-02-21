using System.Collections.Generic;
using UnityEngine;

public class EnvironmentMaze : MonoBehaviour
{
    [SerializeField] private List<GameObject> environmentMazeStates;
    [SerializeField] private Color trainingColor = Color.green;
    [SerializeField] private Color trainedColor = Color.red;
    [SerializeField] private Color unchartedColor = Color.white;
    
    public void GoToState(int state, QualityLearningState learningState)
    {
        SpriteRenderer spriteRenderer = environmentMazeStates[state].GetComponent<SpriteRenderer>();
        if (!spriteRenderer)
        {
            Debug.LogError($"SpriteRenderer for state: {state} is null!");
            return;
        }
        spriteRenderer.color = learningState == QualityLearningState.Training ? trainingColor : trainedColor;
    }

    public void SetAllStateToUncharted()
    {
        foreach (GameObject state in environmentMazeStates)
        {
            SpriteRenderer spriteRenderer = state.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                Debug.LogError($"SpriteRenderer not found for state: {state.name}");
                return;
            }
            spriteRenderer.color = unchartedColor;
        }
    }
    
}
