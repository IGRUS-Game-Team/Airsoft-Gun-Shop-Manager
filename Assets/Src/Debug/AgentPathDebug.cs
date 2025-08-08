using UnityEngine;
using UnityEngine.AI;

public class AgentPathDebug : MonoBehaviour
{
    public NavMeshAgent agent;
    public Color lineColor = Color.cyan;
    public Color pointColor = Color.magenta;
    public float sphereSize = 0.07f;

    void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent == null) return;

        // 아직 경로 계산 중이면 패스
        if (agent.pathPending) return;

        // 현재 경로 상태 로그
        if (agent.hasPath)
        {
            Debug.DrawLine(transform.position, agent.steeringTarget, Color.yellow, 0f, false);

            var cs = agent.path.corners;
            for (int i = 0; i < cs.Length - 1; i++)
            {
                Debug.DrawLine(cs[i], cs[i + 1], lineColor, 0f, false);
            }

            // 한 번만 보고 싶으면 주석 풀기
            // Debug.Log($"[PATH] corners={cs.Length}, status={agent.pathStatus}");
        }
    }

    void OnDrawGizmos()
    {
        if (agent == null || agent.path == null) return;
        var cs = agent.path.corners;
        Gizmos.color = pointColor;
        for (int i = 0; i < cs.Length; i++)
        {
            Gizmos.DrawSphere(cs[i], sphereSize);
        }
    }
}