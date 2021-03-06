﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides methods to spawn units in the scene.
/// </summary>
public class UnitSpawner : MonoBehaviour {
    /// <summary>
    /// Spawns a given unit type at given location.
    /// </summary>
    /// <param name="unitType">Type of unit to spawn.</param>
    /// <param name="spawnLocation">Location to spawn the unit.</param>
    /// <returns>Reference to the spawned unit.</returns>
    public static GameObject SpawnUnit(GameObject unitType, Vector3 spawnLocation)
    {
        if (unitType == null)
            throw new MissingReferenceException("Given unitType was null, can't spawn.");
        if (unitType.GetComponent<UnitInfo>() == null)
            throw new MissingComponentException("Given unitType doesn't have a UnitInfo component. Is it a unit?");

        Quaternion lookRotation = Quaternion.identity;
        if (unitType.GetComponent<UnitInfo>().IsPlayerShip)
        {
            lookRotation = RotationCalculator.RotationTowardZero(spawnLocation);
            if(UnitTracker.PlayerShip != null)
                throw new System.Exception("A player ship already exists in this scene and you are trying to instantiate another player ship.");
        }
        else
            lookRotation = RotationCalculator.RotationTowardPlayerShip(spawnLocation);
            
        GameObject unit = Instantiate(unitType, spawnLocation, lookRotation) as GameObject;
        UnitTracker.AddUnit(unit);
        return unit;
    }

    public static GameObject SpawnUnit(GameObject unitType, Vector3 spawnLocation, Quaternion spawnRotation)
    {
        if (unitType == null)
            throw new MissingReferenceException("Given unitType was null, can't spawn.");
        if (unitType.GetComponent<UnitInfo>() == null)
            throw new MissingComponentException("Given unitType doesn't have a UnitInfo component. Is it a unit?");
        if (unitType.GetComponent<UnitInfo>().IsPlayerShip && UnitTracker.PlayerShip != null)
            throw new System.Exception("A player ship already exists in this scene and you are trying to instantiate another player ship.");

        GameObject unit = Instantiate(unitType, spawnLocation, spawnRotation) as GameObject;
        UnitTracker.AddUnit(unit);
        return unit;
    }
    
    public static List<GameObject> SpawnUnitsInArea(GameObject unitType, int spawnCount, GameObject spawnBox)
    {
        if (spawnBox.GetComponent<BoxCollider>() == null)
            throw new MissingComponentException(spawnBox.name + " is missing a BoxCollider component.");

        BoxCollider spawnCollider = spawnBox.GetComponent<BoxCollider>();
        List<GameObject> spawnedUnits = new List<GameObject>();
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnLocation = Vector3.zero;
            int attemptsToFindPosition = 0;
            while (!spawnCollider.bounds.Contains(spawnLocation) || PositionInsideAnyUnitCollider(spawnLocation))
            {
                attemptsToFindPosition++;
                spawnLocation = new Vector3(spawnBox.transform.position.x + Random.Range(-spawnBox.transform.localScale.x / 2, spawnBox.transform.localScale.x / 2), spawnBox.transform.position.y + Random.Range(-spawnBox.transform.localScale.y / 2, spawnBox.transform.localScale.y / 2), spawnBox.transform.position.z + Random.Range(-spawnBox.transform.localScale.z / 2, spawnBox.transform.localScale.z / 2));

                if (attemptsToFindPosition > 100)
                {
                    Debug.LogError("The spawnBox must be too small, can not spawn every unit without colliding with another unit.");
                    return spawnedUnits;
                }
            }
            spawnedUnits.Add(SpawnUnit(unitType, spawnLocation));
        }

        return spawnedUnits;
    }

    static bool PositionInsideAnyUnitCollider(Vector3 position)
    {
        List<GameObject> units = UnitTracker.GetAllActiveUnits();

        foreach (GameObject go in units)
        {
            MeshCollider mc = go.GetComponent<MeshCollider>();
            if (mc == null)
                continue;

            if (mc.bounds.Contains(position))
            {
                return true;
            }
        }
        return false;
    }
}
