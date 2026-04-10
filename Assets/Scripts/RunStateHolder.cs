using UnityEngine;

public class RunStateHolder : MonoBehaviour
{
    [System.NonSerialized]
    public RunState CurrentRunState;

    public void InitializeRun(RunState runState)
    {
        CurrentRunState = runState;
    }
}
