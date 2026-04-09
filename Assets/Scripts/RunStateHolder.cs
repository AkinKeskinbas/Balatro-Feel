using UnityEngine;

public class RunStateHolder : MonoBehaviour
{
    public RunState CurrentRunState;

    public void InitializeRun(RunState runState)
    {
        CurrentRunState = runState;
    }
}