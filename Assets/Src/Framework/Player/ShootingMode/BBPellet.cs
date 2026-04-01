using UnityEngine;

public class BBPellet : MonoBehaviour
{
    private Vector3 targetPos;
    private float speed;
    private ParticleSystem hitVFX;
    private bool hasHit;
    private bool arrived;

    public void Init(Vector3 target, float speed, ParticleSystem hitVFX, bool hasHit)
    {
        targetPos = target;
        this.speed = speed;
        this.hitVFX = hitVFX;
        this.hasHit = hasHit;
    }

    void Update()
    {
        if (arrived) return;

        transform.position = Vector3.MoveTowards(
            transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            arrived = true;

            if (hasHit && hitVFX != null)
            {
                hitVFX.transform.position = targetPos;
                hitVFX.Play();
            }

            // Hide sphere, let trail fade out naturally
            var renderer = GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;

            var trail = GetComponent<TrailRenderer>();
            float delay = (trail != null) ? trail.time + 0.1f : 0f;
            Destroy(gameObject, delay);
        }
    }
}
