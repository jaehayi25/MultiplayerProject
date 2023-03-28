using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Tilemaps;
using System;

public class MazeGenerator : NetworkBehaviour
{
    [SerializeField] int m_MazeWidth;
    [SerializeField] int m_MazeHeight;
    [SerializeField] int gridScale;

    char[,] m_OldMaze;
    char[,] m_Maze;

    Transform m_MazeContainerTransform; 
    [SerializeField] private Transform m_MazeContainerPrefab; 
    [SerializeField] private Transform m_WallPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Only the server should update maze data
        if (IsServer)
        {
            KruskalMazeGenerator k = new KruskalMazeGenerator(m_MazeWidth / 2, m_MazeHeight / 2);
            k.GenerateMaze();
            m_Maze = k.GetMaze();

            m_MazeContainerTransform = Instantiate(m_MazeContainerPrefab);
            m_MazeContainerTransform.GetComponent<NetworkObject>().Spawn(true);
        }
    }

    private void Update()
    {
        if (IsServer)
        {
            if (m_Maze != null)
            {
                for (int x = 0; x < m_Maze.GetLength(0); x++)
                {
                    for (int y = 0; y < m_Maze.GetLength(1); y++)
                    {
                        if (m_OldMaze == null || m_Maze[x, y] != m_OldMaze[x, y])
                        {
                            Vector3Int wallPos = new Vector3Int((x - m_MazeWidth/2) * gridScale, (y - m_MazeHeight/2) * gridScale, 0);
                            if (m_Maze[x, y] == ' ') //maze space changed to empty
                            {
                                //
                            }
                            else if (m_Maze[x, y] == '#') //maze space changed to wall
                            {
                                Transform spawnedObjectTransform = Instantiate(m_WallPrefab, wallPos, Quaternion.identity);
                                spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
                                spawnedObjectTransform.GetComponent<NetworkObject>().TrySetParent(m_MazeContainerTransform);
                            }
                        }
                    }
                }
            }
        }
        m_OldMaze = m_Maze;
    }

    void DisplayMaze()
    {
        string display = ""; 
        for (int i = 0; i < m_Maze.GetLength(0); i++)
        {
            string row = ""; 
            for (int j = 0; j < m_Maze.GetLength(1); j++)
            {
                row += m_Maze[i, j]; 
            }
            display += row + "\n"; 
        }
        Debug.Log(display);
    }
}

class KruskalMazeGenerator
{
    private char[,] maze;
    private int width;
    private int height;

    public KruskalMazeGenerator(int width, int height)
    {
        this.width = width;
        this.height = height;
        maze = new char[width * 2 + 1, height * 2 + 1];
    }

    public void GenerateMaze()
    {
        // Initialize maze with all walls and vertices intact
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                if (x % 2 == 1 && y % 2 == 1)
                {
                    maze[x, y] = ' ';
                }
                else
                {
                    maze[x, y] = '#';
                }
            }
        }

        // Create list of all walls
        List<Tuple<int, int, int>> walls = new List<Tuple<int, int, int>>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x > 0)
                {
                    walls.Add(new Tuple<int, int, int>(x, y, 1));
                }
                if (y > 0)
                {
                    walls.Add(new Tuple<int, int, int>(x, y, 2));
                }
            }
        }

        // Shuffle wall list
        System.Random rng = new System.Random();
        int n = walls.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Tuple<int, int, int> wall = walls[k];
            walls[k] = walls[n];
            walls[n] = wall;
        }

        // Iterate over walls, adding to maze if they connect two disjoint sets
        int[] set = new int[width * height];
        for (int i = 0; i < set.Length; i++)
        {
            set[i] = i;
        }
        foreach (Tuple<int, int, int> wall in walls)
        {
            int x = wall.Item1;
            int y = wall.Item2;
            int direction = wall.Item3;
            int a = x + y * width;
            int b = a + (direction == 1 ? -1 : -width);
            if (FindSet(a, set) != FindSet(b, set))
            {
                maze[x * 2 + (direction == 1 ? -1 : 0) + 1, y * 2 + (direction == 2 ? -1 : 0) + 1] = ' ';
                Union(a, b, set);
            }
        }
    }

    public char[,] GetMaze()
    {
        return maze;
    }

    private int FindSet(int i, int[] set)
    {
        if (set[i] == i)
        {
            return i;
        }
        set[i] = FindSet(set[i], set);
        return set[i];
    }

    private void Union(int i, int j, int[] set)
    {
        set[FindSet(i, set)] = FindSet(j, set);
    }
}