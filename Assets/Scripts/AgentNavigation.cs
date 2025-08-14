using UnityEngine;
using UnityEngine.AI;

public class AgentNavigation : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private Transform player;
    private Vector3 destination;

    [SerializeField] private float triggerRadius = 5f;
    [SerializeField] private float maxSeparation = 10f;
    [SerializeField] private float resumeDist = 9f;
    [SerializeField] private float stopDist = 2f;

    private bool hasStarted = false;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDist;
        agent.isStopped = true;
    }

    public void SetDestination(Vector3 desiredDestination)
    {
        destination = desiredDestination;
    }
    private void Update()
    {
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // condition for when the agent should start moving the very first time
        if (!hasStarted && distToPlayer <= triggerRadius)
        {
            hasStarted = true;
            agent.isStopped = false;
            agent.destination = destination;
        }

        if (hasStarted)
        {
            //agent should stop when too far
            if (!isWaiting && distToPlayer > maxSeparation)
            {
                isWaiting = true;
                agent.isStopped = true;
            }
            else if (isWaiting && distToPlayer <= resumeDist)
            {
                isWaiting = false;
                agent.isStopped = false;
                agent.destination = destination;
            }
        }
    }
}