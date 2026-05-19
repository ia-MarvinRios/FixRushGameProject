using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Clase estática que aporta métodos útiles a la funcionalidad de los NavMeshAgents para formar colas ordenadas.
/// </summary>
public static class AIExtension
{
    /// <summary>
    /// Interface a implementar para cada objeto que pertenece a una cola y crucial para el funcionamiento de 
    /// la lógica de la clase de extensión.
    /// </summary>
    public interface IQueueAgent
    {
        /// <summary>
        /// El índice del agente dentro de la cola. Define su posición dentro de la misma.
        /// </summary>
        public int QueueIndex { get; set; }
        /// <summary>
        /// El tamaño del collider del agente para calcular los espacios entre agentes
        /// dentro de la cola a lo largo de esta.
        /// </summary>
        public Vector3 Size { get; }
    }

    /// <summary>
    /// Recalcula la posición de los agentes en cola a partir del índice dado.
    /// </summary>
    /// <param name="startIndex">El índice desde el cual se recalcula la cola.</param>
    public static void RecalculateQueueFrom(int startIndex, float offset, Vector3 waitPointPos, NavMeshAgent leader, List<NavMeshAgent> activeAgents)
    {
        NavMeshPath path = new NavMeshPath();

        if (!leader.CalculatePath(waitPointPos, path))
            return;

        float pathLength = GetPathLength(path);

        for (int i = startIndex; i < activeAgents.Count; i++)
        {
            IQueueAgent queueAgent = activeAgents[i].GetComponent<IQueueAgent>();
            queueAgent.QueueIndex = i;

            float distance = GetQueueDistance(i, offset, activeAgents);
            distance = Mathf.Min(distance, pathLength - queueAgent.Size.z);

            Vector3 point =
                GetPointFromPathEnd(path, distance);

            activeAgents[i].SetDestination(point);
        }
    }
    /// <summary>
    /// Obtiene la distancia de separación para los agentes a través del la cola en base a
    /// su tamaño z del collider y su posición en la cola.
    /// </summary>
    /// <param name="size">El tamaño Z del collider.</param>
    /// <param name="index">El índice del objeto en la cola.</param>
    /// <returns>La distancia de separación a lo largo de toda la cola para el agente.</returns>
    public static float GetQueueDistance(int index, float offset, List<NavMeshAgent> agents)
    {
        float distance = 0f;

        for (int i = 0; i < index; i++)
        {
            IQueueAgent queueObject = agents[i].GetComponent<IQueueAgent>();
            distance += queueObject.Size.z + offset + 0.2f;
        }

        return distance;
    }
    /// <summary>
    /// Obtiene el tamaño total de un NavMeshPath sumando las distancias entre sus esquinas.
    /// </summary>
    /// <param name="path">El NavMeshPath para calcular el tamaño.</param>
    /// <returns>El tamaño total del recorrido.</returns>
    public static float GetPathLength(NavMeshPath path)
    {
        float length = 0f;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }

        return length;
    }
    /// <summary>
    /// Obtiene un punto dentro del recorrido que esté a cierta distancia del final del recorrido.
    /// </summary>
    /// <param name="path">El NavMeshPath en el que se basa el algoritmo para obtener el punto.</param>
    /// <param name="distanceFromEnd">La distancia desde el final del recorrido.</param>
    /// <returns>El punto dentro del recorrido que se encuentra a la distancia especificada desde el final del recorrido.</returns>
    public static Vector3 GetPointFromPathEnd(NavMeshPath path, float distanceFromEnd)
    {
        float remaining = distanceFromEnd;

        for (int i = path.corners.Length - 1; i > 0; i--)
        {
            float segmentLength =
                Vector3.Distance(path.corners[i], path.corners[i - 1]);

            if (remaining <= segmentLength)
            {
                Vector3 dir =
                    (path.corners[i - 1] - path.corners[i]).normalized;

                return path.corners[i] + dir * remaining;
            }

            remaining -= segmentLength;
        }

        return path.corners[0];
    }

    /// <summary>
    /// Hace varias comprobaciones lógicas para saber si el agente realmente llegó a su destino.
    /// </summary>
    /// <param name="agent">El agente a analizar.</param>
    /// <returns>Verdadero si llegó al destino. Falso si no ha llegado aún a su destino.</returns>
    public static bool HasReachedDestination(NavMeshAgent agent)
    {
        if (!agent || !agent.enabled || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending ||
            agent.remainingDistance > agent.stoppingDistance + 0.05f ||
            agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            return false;

        return true;
    }
}
