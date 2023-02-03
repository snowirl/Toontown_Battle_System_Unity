using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class CogMove : NetworkBehaviour
{
    public NavMeshAgent agent;
    public Transform[] patrolPoints;
    public int patrolIndex;
    
    [SyncVar]
    public bool isBusy;

    public override void OnStartServer()
    {
        if(patrolPoints.Length > 0)
        {
            agent.destination = patrolPoints[0].position;
        }
        
    }

    void Update()
    {
        if(!isServer) {return;}

        if(!isBusy)
        {
            Move();
        }
    }

    void Move()
    {
        if(!isServer) {return;}

        if(patrolPoints.Length < 1)
        {
            return;
        }

        print(patrolPoints.Length);

        float distance = Vector3.Distance(transform.position, patrolPoints[patrolIndex].position);

        GetComponent<CogAnimate>().ChangeAnimationState("Walk");

        if(distance < 1f)
        {
            GoToNextPoint();
        }
    }

    void GoToNextPoint()
    {
        patrolIndex++;

        if(patrolIndex > patrolPoints.Length - 1)
        {
            patrolIndex = 0;
        }

        agent.destination = patrolPoints[patrolIndex].position;
    }
}
