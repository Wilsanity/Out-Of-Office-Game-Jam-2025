using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class Escort : MonoBehaviour, Interactable
{
    [SerializeField] float healthRefill = 10f;
    [SerializeField] Transform[] spawnPoints;
    bool isPickedUp = false;
    bool isDropped = false;

    Animator animator;
    NavMeshAgent navMeshAgent;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        
        TaskSpawnPoint pickup = TaskSpawnPoint.FindRandomSpawnPoint(tsp => tsp.allowEscort && tsp.isFree);
        pickup.isFree = false;

        TaskSpawnPoint drop = TaskSpawnPoint.FindRandomSpawnPoint(tsp => tsp.allowEscort && tsp.isFree);
        drop.isFree = false;
    }

    public void SetHighlight(bool highlighted)
    {
        //TODO: implement highlight
    }

    public void Interact(Player source)
    {
        if (isPickedUp)
        {
            transform.SetParent(source.transform);
            animator.SetBool("IsSitting", true);
            
            
        }

        if (!isPickedUp && isDropped)
        {
            transform.SetParent(null);
            animator.SetBool("IsSitting", false);
        }
    }

    private void Update()
    {
        //Animation
        if (!isPickedUp && navMeshAgent.velocity.magnitude > 0) animator.SetBool("IsWalking", true);
        else animator.SetBool("IsWalking", false);
        
        
    }
}