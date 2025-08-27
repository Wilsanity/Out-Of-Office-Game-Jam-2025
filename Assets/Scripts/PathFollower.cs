using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [SerializeField] PathPoint nextPoint;
    [SerializeField] float speed;

    private void Update() {
        FollowPath(speed * Time.deltaTime);
    }

    private void FollowPath(float distance) {
        float nextSqDist = (nextPoint.transform.position - transform.position).sqrMagnitude;
        if (nextSqDist > distance * distance) { // straight line
            transform.position = Vector3.MoveTowards(transform.position, nextPoint.transform.position, speed * Time.deltaTime);
        } else { // turn the corner and keep going
            transform.position = nextPoint.transform.position;
            nextPoint = nextPoint.nextPoint;
            transform.forward = nextPoint.transform.position - transform.position;
            FollowPath(distance - Mathf.Sqrt(nextSqDist));
        }
    }
}
