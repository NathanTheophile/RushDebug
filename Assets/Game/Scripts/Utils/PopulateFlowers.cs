using System.Collections.Generic;
using Rush.Game;
using UnityEngine;
using System.Linq;

public class PopulateFlowers : MonoBehaviour
{
    [SerializeField] private int cubesToSpawn;
    [SerializeField] private Transform floatingContainer;
    [SerializeField] private Transform flowerPrefab;
    private List<Transform> flowers = new();
    private List<Transform> shuffledFlowers = new();
    [SerializeField] private List<Target> targets;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    void Start()
    {
        foreach (Transform t in floatingContainer)
        {
            Transform lFlower = Instantiate(flowerPrefab, t);
            lFlower.gameObject.SetActive(false);
            flowers.Add(lFlower);  
        }

        shuffledFlowers = flowers.OrderBy( x => Random.value ).ToList( );

        for (int i = 0; i < targets.Count; i++)
        {
            Debug.Log("Starts spawner " + i);
            targets[i].amountOfCubes = cubesToSpawn;
            Target lTarget = targets[i];
            for (int j = i; j < shuffledFlowers.Count -1; j += targets.Count)
            {
                targets[i].ElementsToActivate.Add(shuffledFlowers[j]);
            }
            targets[i].PopulateElementsToActivate();
        }
    }
}
