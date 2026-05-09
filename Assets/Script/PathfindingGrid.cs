using System.Collections.Generic;
using UnityEngine;

public class PathfindingGrid : MonoBehaviour
{
    public static PathfindingGrid Instance { get; private set; }

    [Header("Configuración de la cuadrícula")]
    public Vector2  worldSize    = new Vector2(40f, 40f);
    public float    nodeRadius   = 0.5f;
    public LayerMask obstacleMask;

    private Node[,] _grid;
    private int     _sizeX, _sizeY;
    private float   _diameter;

    private void Awake()
    {
        Instance  = this;
        _diameter = nodeRadius * 2f;
        _sizeX    = Mathf.RoundToInt(worldSize.x / _diameter);
        _sizeY    = Mathf.RoundToInt(worldSize.y / _diameter);
        BakeGrid();
    }

    private void BakeGrid()
    {
        _grid = new Node[_sizeX, _sizeY];
        Vector2 origin = (Vector2)transform.position - worldSize * 0.5f;

        for (int x = 0; x < _sizeX; x++)
        for (int y = 0; y < _sizeY; y++)
        {
            Vector2 wp       = origin + new Vector2(x * _diameter + nodeRadius, y * _diameter + nodeRadius);
            bool    walkable = !Physics2D.OverlapCircle(wp, nodeRadius * 0.9f, obstacleMask);
            _grid[x, y]      = new Node(walkable, wp, x, y);
        }
    }

    // API pública

    // Devuelve nodos walkable dentro del rango de distancia dado — usado por el spawner.
    public List<Vector2> GetSpawnCandidates(Vector2 center, float minDist, float maxDist)
    {
        var result = new List<Vector2>();
        float minSq = minDist * minDist;
        float maxSq = maxDist * maxDist;
        foreach (Node n in _grid)
        {
            if (!n.Walkable) continue;
            float sq = (n.WorldPosition - center).sqrMagnitude;
            if (sq >= minSq && sq <= maxSq) result.Add(n.WorldPosition);
        }
        return result;
    }

    // Devuelve una lista de posiciones mundiales que forman la ruta A* (puede ser null si no hay ruta).
    public List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        Node s = WorldToNode(start);
        Node e = WorldToNode(end);
        if (s == null || e == null || !s.Walkable) return null;

        // Si el nodo destino no es walkable, busca el más cercano que sí lo sea
        if (!e.Walkable) e = NearestWalkable(e);
        if (e == null) return null;

        foreach (Node n in _grid) { n.GCost = int.MaxValue; n.Parent = null; }

        var open   = new List<Node> { s };
        var closed = new HashSet<Node>();
        s.GCost = 0;
        s.HCost = Distance(s, e);

        while (open.Count > 0)
        {
            Node cur = open[0];
            for (int i = 1; i < open.Count; i++)
                if (open[i].FCost < cur.FCost || (open[i].FCost == cur.FCost && open[i].HCost < cur.HCost))
                    cur = open[i];

            open.Remove(cur);
            closed.Add(cur);

            if (cur == e) return BuildPath(s, e);

            foreach (Node nb in Neighbours(cur))
            {
                if (!nb.Walkable || closed.Contains(nb)) continue;
                int g = cur.GCost + Distance(cur, nb);
                if (g < nb.GCost)
                {
                    nb.GCost  = g;
                    nb.HCost  = Distance(nb, e);
                    nb.Parent = cur;
                    if (!open.Contains(nb)) open.Add(nb);
                }
            }
        }
        return null;
    }

    // Helpers privados

    private Node WorldToNode(Vector2 wp)
    {
        Vector2 origin = (Vector2)transform.position - worldSize * 0.5f;
        int x = Mathf.Clamp(Mathf.FloorToInt((wp.x - origin.x) / _diameter), 0, _sizeX - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((wp.y - origin.y) / _diameter), 0, _sizeY - 1);
        return _grid[x, y];
    }

    private Node NearestWalkable(Node from)
    {
        Node best = null; int bestDist = int.MaxValue;
        foreach (Node n in _grid)
        {
            if (!n.Walkable) continue;
            int d = Distance(n, from);
            if (d < bestDist) { bestDist = d; best = n; }
        }
        return best;
    }

    private List<Vector2> BuildPath(Node start, Node end)
    {
        var path = new List<Node>();
        Node cur = end;
        while (cur != start) { path.Add(cur); cur = cur.Parent; }
        path.Reverse();
        var result = new List<Vector2>(path.Count);
        foreach (var n in path) result.Add(n.WorldPosition);
        return result;
    }

    private List<Node> Neighbours(Node n)
    {
        var list = new List<Node>(8);
        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            if (dx == 0 && dy == 0) continue;
            int nx = n.GridX + dx, ny = n.GridY + dy;
            if (nx >= 0 && nx < _sizeX && ny >= 0 && ny < _sizeY)
                list.Add(_grid[nx, ny]);
        }
        return list;
    }

    // Coste heurístico (distancia Chebyshev × 10/14)
    private static int Distance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.GridX - b.GridX), dy = Mathf.Abs(a.GridY - b.GridY);
        return dx > dy ? 14 * dy + 10 * (dx - dy) : 14 * dx + 10 * (dy - dx);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(worldSize.x, worldSize.y, 0.1f));
        if (_grid == null) return;
        float s = _diameter - 0.05f;
        foreach (Node n in _grid)
        {
            Gizmos.color = n.Walkable ? new Color(1, 1, 1, 0.08f) : new Color(1, 0, 0, 0.35f);
            Gizmos.DrawCube(n.WorldPosition, new Vector3(s, s, 0.1f));
        }
    }
#endif

    // Clase nodo
    public class Node
    {
        public readonly bool    Walkable;
        public readonly Vector2 WorldPosition;
        public readonly int     GridX, GridY;
        public int              GCost, HCost;
        public Node             Parent;
        public int              FCost => GCost + HCost;

        public Node(bool walkable, Vector2 pos, int x, int y)
        { Walkable = walkable; WorldPosition = pos; GridX = x; GridY = y; }
    }
}
