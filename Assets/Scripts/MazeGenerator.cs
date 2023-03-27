using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

public class MazeGenerator : NetworkBehaviour
{
    [SerializeField] int m_MazeHeight;
    [SerializeField] int m_MazeWidth;

    char[,] m_Maze;
    int m_totalRooms;
    List<Tuple<int, int>> m_walls = new List<Tuple<int, int>>();
    Tuple<int, int>[,] m_group; 

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        KruskalMazeGenerator k = new KruskalMazeGenerator(10, 15);
        k.GenerateMaze();
        m_Maze = k.GetMaze();
        DisplayMaze(); 

        /*

        m_Maze = new char[2 * m_MazeHeight + 1, 2 * m_MazeWidth + 1];
        m_group = new Tuple<int, int>[2 * m_MazeHeight + 1, 2 * m_MazeWidth + 1];

        //Initialize walls of maze
        for (int i = 0; i < m_Maze.GetLength(0); i++)
        {
            for (int j = 0; j < m_Maze.GetLength(1); j++)
            {
                m_Maze[i, j] = '#'; 
            }
        }

        for (int i = 0; i < m_Maze.GetLength(0) - 1; i++)
        {
            for (int j = (i+1)%2; j < m_Maze.GetLength(1) - 1; j += 2)
            {
                m_Maze[i, j] = '?';
                m_walls.Add(Tuple.Create(i, j));
            }
        }
        m_walls.Shuffle();

        //Initialize non-walls
        m_totalRooms = 0; 
        for (int i = 1; i < m_Maze.GetLength(0); i+=2)
        {
            for (int j = 1; j < m_Maze.GetLength(1); j+=2)
            {
                m_totalRooms++; 
                m_Maze[i, j] = ' ';
                m_group[i, j] = Tuple.Create(i, j); 
            }
        }

        CreateMaze();

        DisplayMaze();
        */
    }

    public void Union(Tuple<int, int> room1, Tuple<int, int> room2)
    {

    }

    public Tuple<int, int> Find(Tuple<int, int> room)
    {
        Tuple<int, int> parent = m_group[room.Item1, room.Item2];
        if (room == parent)
        {
            return room; 
        }
        return Find(parent); 
    }

    List<Tuple<int, int>> GetRoomsNextToWall(Tuple<int, int> wall)
    {
        List<Tuple<int, int>> rooms = new List<Tuple<int, int>>();

        int row = wall.Item1;
        int col = wall.Item2; 
        if (row == 0) //rooms should be opposite side vertically
        {
            rooms.Add(Tuple.Create(0, col));
            rooms.Add(Tuple.Create(m_MazeHeight - 2, col));
        } else if (col == 0) //rooms should be opposite side horizontally
        {
            rooms.Add(Tuple.Create(row, 0));
            rooms.Add(Tuple.Create(row, m_MazeWidth - 2));
        } else if (row % 2 == 0) //wall connects rooms vertically
        {
            rooms.Add(Tuple.Create(row + 1, col));
            rooms.Add(Tuple.Create(row - 1, col));
        } else //wall connects room horizontally
        {
            rooms.Add(Tuple.Create(row, col - 1));
            rooms.Add(Tuple.Create(row, col + 1));
        }

        return rooms; 
    }

    void CreateMaze()
    {
        for (int i = 0; i < m_walls.Count; i++)
        {
            List<Tuple<int, int>> adjRooms = GetRoomsNextToWall(m_walls[i]);

            if (i % 100 == 0)
            {
                DisplayMaze(); 
            }
        }
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

static class MyExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
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

/*
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
        // Initialize maze with all walls intact
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                maze[x, y] = '#';
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
*/