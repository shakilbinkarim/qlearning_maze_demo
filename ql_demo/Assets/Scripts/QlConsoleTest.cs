using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QualityLearningState
{
    Training,
    Trained
}

public class QlConsoleTest : MonoBehaviour
{
    [SerializeField] private int maxEpochs = 500;
    [SerializeField] private int goalState = 0;
    [SerializeField] private int startState = 0;
    [SerializeField] private float discountRate = 0.1f;
    [SerializeField] private float learningRate = 0.1f;
    [SerializeField] private GameObject environment;
    
    private static readonly System.Random Random = new System.Random(1);

    private QualityLearningState _learningState;
    private EnvironmentMaze _environmentMaze;
    private int _numberOfStates;
    private int[][] _environmentMatrix;
    private float[] _stateRewards;
    private double[][] _qMatrix;

    private void Start()
    {
        Debug.Log("Begin Q-Learning Maze Demo");
        SetUpEnvironmentAndRewardsForTraining();
        StartCoroutine(TrainAsync());
        StartCoroutine(WalkAsync());
        Debug.Log("End demo");
    }

    private void SetUpEnvironmentAndRewardsForTraining()
    {
        Debug.Log("Setting up maze and rewards");
        _numberOfStates = 12;
        _environmentMatrix = CreateEnvironmentMaze(_numberOfStates);
        _stateRewards = CreateRewardList(_numberOfStates);
        _qMatrix = CreateQualityMatrix(_numberOfStates);
    }

    private IEnumerator WalkAsync()
    {
        yield return Walk();
    }

    private float[] CreateRewardList(int numberOfStates)
    {
        float [] rewardList = new float[numberOfStates];
        for (int i = 0; i < rewardList.Length; i++) rewardList[i] = -0.1f;
        rewardList[goalState] = 10.0f;
        rewardList[11] = -100.0f;
        return rewardList;
    }

    private static int MaxAction(IReadOnlyList<double> possibleActions) {
        double maxVal = possibleActions[0];  
        int action = 0;
        for (int i = 0; i < possibleActions.Count; ++i) {
            if (!(possibleActions[i] > maxVal)) continue;
            maxVal = possibleActions[i];  
            action = i;
        }
        return action;
    }
    
    private string Walk()
    {
        while (_learningState != QualityLearningState.Trained)
        {
        }
        _environmentMaze.SetAllStateToUncharted();
        int currentState = startState;
        _environmentMaze.GoToState(currentState, _learningState);
        string printString = currentState + "->";
        while (currentState != goalState) {
            int next = MaxAction(_qMatrix[currentState]);
            printString += next + "->";
            currentState = next;
            _environmentMaze.GoToState(currentState, _learningState);
        }
        printString += "done";
        Debug.Log(printString);
        return printString;
    }

    private static void Print(IReadOnlyList<double[]> qMatrix)
    {
        int numberOfStates = qMatrix.Count;
        string printString = "[0] [1] . . [11]";
        for (int i = 0; i < numberOfStates; ++i) {
            for (int j = 0; j < numberOfStates; ++j) printString += qMatrix[i][j].ToString("F2") + " ";
            Debug.Log(printString);
        }
    }
    
    private IEnumerator TrainAsync()
    {
        yield return Train();
    }

    private QualityLearningState Train()
    {
        _learningState = QualityLearningState.Training;
        for (int epoch = 0; epoch < maxEpochs; ++epoch)
        {
            _environmentMaze.SetAllStateToUncharted();
            int currentState = Random.Next(0, _stateRewards.Length);
            _environmentMaze.GoToState(currentState, _learningState);
            do
            {
                int nextState = GetRandNextState(currentState, _environmentMatrix);
                List<int> possibleActions = GetPossibleActions(nextState, _environmentMatrix);
                double maxQ = MaxQ(_qMatrix, possibleActions, nextState);
                _qMatrix[currentState][nextState] =
                    ((1 - learningRate) * _qMatrix[currentState][nextState]) +
                    (learningRate * (_stateRewards[nextState] + (discountRate * maxQ)));
                currentState = nextState;
                _environmentMaze.GoToState(currentState, _learningState);
                if (_stateRewards[currentState] < 50.0f) break;
            } while (currentState != goalState);
        } // for

        _learningState = QualityLearningState.Trained;
        return _learningState;
    }

    private void Train(IReadOnlyList<int[]> environmentMatrix, IReadOnlyList<float> stateRewards, IReadOnlyList<double[]> qualityMatrix)
    {
        for (int epoch = 0; epoch < maxEpochs; ++epoch) {
            _environmentMaze.SetAllStateToUncharted();
            int currentState = Random.Next(0, stateRewards.Count);
            _environmentMaze.GoToState(currentState, _learningState);
            while (true) {
                int nextState = GetRandNextState(currentState, environmentMatrix);
                List<int> possibleActions = GetPossibleActions(nextState, environmentMatrix);
                double maxQ = MaxQ(qualityMatrix, possibleActions, nextState);
                qualityMatrix[currentState][nextState] =
                    ((1 - learningRate) * qualityMatrix[currentState][nextState]) +
                    (learningRate * (stateRewards[nextState] + (discountRate * maxQ)));
                currentState = nextState;
                _environmentMaze.GoToState(currentState, _learningState);
                if (currentState == goalState) break;
            } // while
        } // for
    }
    
    private static double MaxQ(IReadOnlyList<double[]> qualityMatrix, IEnumerable<int> possibleActions, int nextState)
    {
        double maxQ = double.MinValue;
        foreach (int action in possibleActions) maxQ = MaxDouble(qualityMatrix[nextState][action], maxQ);
        return maxQ;
    }

    private static double MaxDouble(double a, double b) => a > b ? a : b;

    private double[][] CreateRewardMatrix(int numberOfStates)
    {
        double[][] rewardMatrix = new double[numberOfStates][];
        for (int i = 0; i < numberOfStates; ++i) rewardMatrix[i] = new double[numberOfStates];
        rewardMatrix[0][1] = rewardMatrix[0][4] = rewardMatrix[1][0] = rewardMatrix[1][5] = rewardMatrix[2][3] = -0.1;
        rewardMatrix[2][6] = rewardMatrix[3][2] = rewardMatrix[3][7] = rewardMatrix[4][0] = rewardMatrix[4][8] = -0.1;
        rewardMatrix[5][1] = rewardMatrix[5][6] = rewardMatrix[5][9] = rewardMatrix[6][2] = rewardMatrix[6][5] = -0.1;
        rewardMatrix[6][7] = rewardMatrix[7][3] = rewardMatrix[7][6] = rewardMatrix[7][11] = rewardMatrix[8][4] = -0.1;
        rewardMatrix[8][9] = rewardMatrix[9][5] = rewardMatrix[9][8] = rewardMatrix[9][10] = rewardMatrix[10][9] = -0.1;
        rewardMatrix[7][goalState] = 10.0;  // TODO: FIx this
        return rewardMatrix;
    }

    private static double[][] CreateQualityMatrix(int numberOfStates)
    {
        double[][] qualityMatrix = new double[numberOfStates][];
        for (int i = 0; i < numberOfStates; ++i) qualityMatrix[i] = new double[numberOfStates];
        return qualityMatrix;
    }

    private int[][] CreateEnvironmentMaze(int numberOfStates)
    {
        int[][] environmentMatrix = new int[numberOfStates][];
        for (int i = 0; i < numberOfStates; ++i) environmentMatrix[i] = new int[numberOfStates];
        environmentMatrix[0][1] = environmentMatrix[0][4] = environmentMatrix[1][0] = environmentMatrix[1][5] = environmentMatrix[2][3] = 1;
        environmentMatrix[2][6] = environmentMatrix[3][2] = environmentMatrix[3][7] = environmentMatrix[4][0] = environmentMatrix[4][8] = 1;
        environmentMatrix[5][1] = environmentMatrix[5][6] = environmentMatrix[5][9] = environmentMatrix[6][2] = environmentMatrix[6][5] = 1;
        environmentMatrix[6][7] = environmentMatrix[7][3] = environmentMatrix[7][6] = environmentMatrix[7][11] = environmentMatrix[8][4] = 1;
        environmentMatrix[8][9] = environmentMatrix[9][5] = environmentMatrix[9][8] = environmentMatrix[9][10] = environmentMatrix[10][9] = 1;
        environmentMatrix[11][11] = 1;  // Goal
        _environmentMaze = environment.GetComponent<EnvironmentMaze>();
        if (!_environmentMaze) Debug.LogError("Environment Maze not found");
        _environmentMaze.SetAllStateToUncharted();
        return environmentMatrix;
    }

    private static List<int> GetPossibleActions(int state, IReadOnlyList<int[]> environmentMaze) {
        List<int> result = new List<int>();
        for (int j = 0; j < environmentMaze.Count; ++j)
            if (environmentMaze[state][j] == 1) result.Add(j);
        return result;
    }

    private static int GetRandNextState(int s, IReadOnlyList<int[]> environmentMaze) {
        List<int> possibleNextStates = GetPossibleActions(s, environmentMaze);
        int numberOfPossibleNextStates = possibleNextStates.Count;
        int nextState = Random.Next(0, numberOfPossibleNextStates);
        return possibleNextStates[nextState];
    }
    
}
