// using System.Collections.Generic;
// using UnityEngine;

// /* Save state class that records the state of the map before a choice is made so it can be reverted to later.
// */
// public class SaveState
// {
//     Cell[,] map;
//     PriorityQueue pq;
//     Vector2Int position;
//     Tile selection;

//     public SaveState(Cell[,] state, PriorityQueue queue, Vector2Int location, Tile choice)
//     {
//         map = state;
//         pq = queue;
//         position = location;
//         selection = choice;
//     }
//     /*Getters for each save state's data
//     */
//     public Cell[,] Map
//     {
//         get { return map; }
//     }
//     public PriorityQueue PQ
//     {
//         get { return pq; }
//     }
//     public Vector2Int Position
//     {
//         get { return position; }
//     }
//     public Tile Selection
//     {
//         get { return selection; }
//     }
// }