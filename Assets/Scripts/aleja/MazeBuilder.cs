using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Construye un laberinto en Tilemaps a partir de un archivo de texto.
///   X = pared      . = piso
///   Cualquier otra letra o número = piso con un marcador (inicio, gema, enemigos, sonidos...)
/// Se usa desde el Inspector con el botón "Construir laberinto" (no hace falta darle Play).
/// </summary>
public class MazeBuilder : MonoBehaviour
{
    [System.Serializable]
    public class MarcadorObjeto
    {
        [Tooltip("Letra del archivo, por ejemplo P, G, E")]
        public string simbolo = "P";
        [Tooltip("Objeto que YA está en la escena y se moverá a esa posición")]
        public Transform objeto;
    }

    [System.Serializable]
    public class MarcadorPrefab
    {
        public string simbolo = "b";
        [Tooltip("Se crea una copia en cada casilla con esta letra")]
        public GameObject prefab;
    }

    [Header("Diseño del laberinto")]
    [Tooltip("Archivo .txt dentro de Assets. Si está vacío se usa el texto de abajo.")]
    public TextAsset archivoLaberinto;
    [TextArea(8, 30)] public string laberintoTexto;
    public char simboloPared = 'X';

    [Header("Tilemaps (hijos de un Grid)")]
    public Tilemap tilemapPiso;
    [Tooltip("Este Tilemap lleva las colisiones")]
    public Tilemap tilemapParedes;

    [Header("Tiles (arrástralos desde la carpeta de tu Tile Palette)")]
    public TileBase tilePiso;
    [Tooltip("Parte oscura de arriba del muro")]
    public TileBase tileParedArriba;
    [Tooltip("Cara de ladrillo que se ve de frente")]
    public TileBase tileParedFrente;
    [Tooltip("Opcional: fila de ladrillo que toca el piso (con la viga o el zócalo)")]
    public TileBase tileParedFrenteInferior;
    [Range(0, 4)] public int altoFrentePared = 2;

    [Header("Escala y posición")]
    [Min(1)] public int tilesPorCelda = 1;
    [Tooltip("Celda del Tilemap donde queda la esquina superior izquierda del laberinto")]
    public Vector2Int origen = new Vector2Int(-14, 12);

    [Header("Colisiones")]
    public bool configurarColisiones = true;

    [Header("Objetos")]
    public List<MarcadorObjeto> objetosEnEscena = new List<MarcadorObjeto>();
    public List<MarcadorPrefab> prefabs = new List<MarcadorPrefab>();
    public Transform contenedorPrefabs;

    // Datos leídos del archivo (sirven en tiempo de ejecución, por ejemplo para el sistema de audio)
    string[] filas;
    int ancho, alto;
    readonly Dictionary<char, List<Vector3>> posiciones = new Dictionary<char, List<Vector3>>();

    void Awake() { LeerDiseno(); }

    // ---------------------------------------------------------------- API pública

    /// <summary>Posiciones en el mundo de todas las casillas con esa letra.</summary>
    public IReadOnlyList<Vector3> PosicionesDe(char simbolo)
    {
        if (filas == null) LeerDiseno();
        return posiciones.TryGetValue(simbolo, out var l) ? l : new List<Vector3>();
    }

    /// <summary>True si ese punto del mundo cae dentro de una pared del laberinto.</summary>
    public bool EsPared(Vector3 mundo)
    {
        if (filas == null) LeerDiseno();
        Vector3Int t = tilemapParedes.WorldToCell(mundo);
        int x = Mathf.FloorToInt((t.x - origen.x) / (float)tilesPorCelda);
        int y = Mathf.FloorToInt((origen.y - t.y) / (float)tilesPorCelda);
        return Celda(x, y) == simboloPared;
    }

    // ---------------------------------------------------------------- Construcción

    [ContextMenu("Construir laberinto")]
    public void Construir()
    {
        if (!LeerDiseno()) return;
        if (tilemapPiso == null || tilemapParedes == null)
        {
            Debug.LogError("[MazeBuilder] Asigna los dos Tilemaps (Piso y Paredes).", this);
            return;
        }

#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(new Object[] { tilemapPiso, tilemapParedes }, "Construir laberinto");
#endif
        tilemapPiso.ClearAllTiles();
        tilemapParedes.ClearAllTiles();

        int k = tilesPorCelda;
        int tw = ancho * k, th = alto * k;

        // Mapa a resolución de tile (true = pared)
        bool[,] pared = new bool[tw, th];
        for (int y = 0; y < th; y++)
            for (int x = 0; x < tw; x++)
                pared[x, y] = Celda(x / k, y / k) == simboloPared;

        for (int y = 0; y < th; y++)
        {
            for (int x = 0; x < tw; x++)
            {
                Vector3Int pos = new Vector3Int(origen.x + x, origen.y - y, 0);
                if (!pared[x, y])
                {
                    tilemapPiso.SetTile(pos, tilePiso);
                    continue;
                }

                // ¿Hay piso justo debajo, a 1..altoFrentePared tiles, sin otra cosa en medio?
                int distanciaAlPiso = 0;
                for (int d = 1; d <= altoFrentePared; d++)
                {
                    int yy = y + d;
                    if (yy >= th) break;
                    if (!pared[x, yy]) { distanciaAlPiso = d; break; }
                }

                TileBase t = tileParedArriba;
                if (distanciaAlPiso > 0 && tileParedFrente != null)
                    t = (distanciaAlPiso == 1 && tileParedFrenteInferior != null) ? tileParedFrenteInferior : tileParedFrente;

                tilemapParedes.SetTile(pos, t);
            }
        }

        tilemapPiso.CompressBounds();
        tilemapParedes.CompressBounds();

        if (configurarColisiones) ConfigurarColisiones();
        ColocarObjetos();
        RevisarCaminos();

#if UNITY_EDITOR
        if (!Application.isPlaying) EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log($"[MazeBuilder] Laberinto de {ancho}x{alto} celdas construido ({tw}x{th} tiles).", this);
    }

    [ContextMenu("Borrar laberinto")]
    public void Borrar()
    {
#if UNITY_EDITOR
        if (tilemapPiso && tilemapParedes)
            Undo.RegisterCompleteObjectUndo(new Object[] { tilemapPiso, tilemapParedes }, "Borrar laberinto");
#endif
        if (tilemapPiso) tilemapPiso.ClearAllTiles();
        if (tilemapParedes) tilemapParedes.ClearAllTiles();
        BorrarPrefabsCreados();
    }

    // ---------------------------------------------------------------- Lectura

    bool LeerDiseno()
    {
        string texto = archivoLaberinto != null ? archivoLaberinto.text : laberintoTexto;
        if (string.IsNullOrWhiteSpace(texto))
        {
            Debug.LogError("[MazeBuilder] No hay diseño: asigna el archivo .txt o pega el texto.", this);
            return false;
        }

        var lista = new List<string>();
        foreach (string linea in texto.Replace("\r", "").Split('\n'))
            if (linea.Trim().Length > 0) lista.Add(linea.TrimEnd());

        filas = lista.ToArray();
        alto = filas.Length;
        ancho = 0;
        foreach (string f in filas) ancho = Mathf.Max(ancho, f.Length);

        posiciones.Clear();
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                char c = Celda(x, y);
                if (c == simboloPared || c == '.' || c == ' ') continue;
                if (!posiciones.TryGetValue(c, out var l)) posiciones[c] = l = new List<Vector3>();
                l.Add(CentroCelda(x, y));
            }
        }
        return true;
    }

    char Celda(int x, int y)
    {
        if (y < 0 || y >= alto || x < 0) return simboloPared;
        string f = filas[y];
        return x < f.Length ? f[x] : simboloPared;
    }

    Vector3 CentroCelda(int x, int y)
    {
        Tilemap tm = tilemapPiso != null ? tilemapPiso : tilemapParedes;
        if (tm == null) return Vector3.zero;
        int k = tilesPorCelda;
        // Tile de abajo a la izquierda de la celda
        Vector3Int abajoIzq = new Vector3Int(origen.x + x * k, origen.y - (y * k + k - 1), 0);
        Vector3 cs = tm.layoutGrid.cellSize;
        return tm.CellToWorld(abajoIzq) + new Vector3(cs.x * k * 0.5f, cs.y * k * 0.5f, 0f);
    }

    // ---------------------------------------------------------------- Objetos

    void ColocarObjetos()
    {
        // Objetos de la escena: si hay dos "E" se reparten en orden (Enemy, Enemy (1)...)
        var usados = new Dictionary<char, int>();
        foreach (var m in objetosEnEscena)
        {
            if (m.objeto == null || string.IsNullOrEmpty(m.simbolo)) continue;
            char c = m.simbolo[0];
            usados.TryGetValue(c, out int i);
            var lista = PosicionesDe(c);
            if (i >= lista.Count)
            {
                Debug.LogWarning($"[MazeBuilder] No hay suficientes '{c}' en el diseño para {m.objeto.name}.", this);
                continue;
            }
#if UNITY_EDITOR
            Undo.RecordObject(m.objeto, "Mover objeto del laberinto");
#endif
            Vector3 p = lista[i];
            m.objeto.position = new Vector3(p.x, p.y, m.objeto.position.z);
            usados[c] = i + 1;
        }

        // Prefabs (decoración, fuentes de sonido, etc.)
        if (prefabs.Count == 0) return;
        if (contenedorPrefabs == null)
        {
            var go = new GameObject("Laberinto_Objetos");
            go.transform.SetParent(transform, false);
            contenedorPrefabs = go.transform;
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(go, "Crear contenedor");
#endif
        }
        BorrarPrefabsCreados();

        foreach (var m in prefabs)
        {
            if (m.prefab == null || string.IsNullOrEmpty(m.simbolo)) continue;
            foreach (Vector3 p in PosicionesDe(m.simbolo[0]))
            {
                GameObject nuevo;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    nuevo = (GameObject)PrefabUtility.InstantiatePrefab(m.prefab, contenedorPrefabs);
                    Undo.RegisterCreatedObjectUndo(nuevo, "Crear objeto del laberinto");
                }
                else
#endif
                    nuevo = Instantiate(m.prefab, contenedorPrefabs);
                nuevo.transform.position = new Vector3(p.x, p.y, m.prefab.transform.position.z);
            }
        }
    }

    void BorrarPrefabsCreados()
    {
        if (contenedorPrefabs == null) return;
        for (int i = contenedorPrefabs.childCount - 1; i >= 0; i--)
        {
            GameObject hijo = contenedorPrefabs.GetChild(i).gameObject;
#if UNITY_EDITOR
            if (!Application.isPlaying) { Undo.DestroyObjectImmediate(hijo); continue; }
#endif
            Destroy(hijo);
        }
    }

    // ---------------------------------------------------------------- Colisiones

    void ConfigurarColisiones()
    {
        GameObject go = tilemapParedes.gameObject;

        var col = go.GetComponent<TilemapCollider2D>();
        if (col == null) col = Undoable<TilemapCollider2D>(go);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = Undoable<Rigidbody2D>(go);
        rb.bodyType = RigidbodyType2D.Static;

        if (go.GetComponent<CompositeCollider2D>() == null) Undoable<CompositeCollider2D>(go);

#if UNITY_6000_0_OR_NEWER
        col.compositeOperation = Collider2D.CompositeOperation.Merge;
#else
        col.usedByComposite = true;
#endif
    }

    T Undoable<T>(GameObject go) where T : Component
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return Undo.AddComponent<T>(go);
#endif
        return go.AddComponent<T>();
    }

    // ---------------------------------------------------------------- Validación

    void RevisarCaminos()
    {
        int sx = -1, sy = -1;
        for (int y = 0; y < alto && sx < 0; y++)
            for (int x = 0; x < ancho; x++)
                if (Celda(x, y) == 'P') { sx = x; sy = y; break; }
        if (sx < 0) { Debug.LogWarning("[MazeBuilder] El diseño no tiene 'P' (inicio).", this); return; }

        var visto = new bool[ancho, alto];
        var cola = new Queue<Vector2Int>();
        cola.Enqueue(new Vector2Int(sx, sy));
        visto[sx, sy] = true;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (cola.Count > 0)
        {
            var c = cola.Dequeue();
            foreach (var d in dirs)
            {
                var n = c + d;
                if (n.x < 0 || n.y < 0 || n.x >= ancho || n.y >= alto) continue;
                if (visto[n.x, n.y] || Celda(n.x, n.y) == simboloPared) continue;
                visto[n.x, n.y] = true;
                cola.Enqueue(n);
            }
        }

        for (int y = 0; y < alto; y++)
            for (int x = 0; x < ancho; x++)
            {
                char c = Celda(x, y);
                if (c != simboloPared && c != '.' && !visto[x, y])
                    Debug.LogWarning($"[MazeBuilder] '{c}' en columna {x}, fila {y} no se puede alcanzar desde el inicio.", this);
            }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MazeBuilder))]
public class MazeBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space(10);
        var mb = (MazeBuilder)target;
        GUI.backgroundColor = new Color(0.55f, 0.85f, 0.55f);
        if (GUILayout.Button("Construir laberinto", GUILayout.Height(32))) mb.Construir();
        GUI.backgroundColor = new Color(0.9f, 0.55f, 0.55f);
        if (GUILayout.Button("Borrar laberinto")) mb.Borrar();
        GUI.backgroundColor = Color.white;
    }
}
#endif