﻿using UnityEngine;
using System.Collections;
using System;

public class PlantScript : MonoBehaviour
{
    //Basic attributes
    public SeasonScript.Seasons[] season;
    public bool functionalPlant;
    Vector3 PlantPosition;
    bool m_active;
    bool m_destroy;
    int m_seed_nr;
    int m_plantTry;

    //Plant sorts and strengths
    public enum PlantSorts
    {
        PLANT1,
        PLANT2,
        PLANT3,
        PLANT4
    };

    public PlantSorts PlantSort;
    public PlantSorts[] PlantSortsStronger;

    Vector3 m_weakPlantPos;

    //Timers
    float m_plantTimerCurr = 2f;//Sec
    float m_plantTimerPrev;

    float m_plantDestroy = 1f;//Sec

    // Use this for initialization
    void Start()
    {
        m_plantTimerPrev = m_plantTimerCurr;

        if (gameObject.tag != "Pickup")
        {
            m_seed_nr = this.transform.parent.gameObject.GetComponent<PlantedSeedScript>().SeedNr;
        }

        m_active = false;
        m_destroy = false;
        m_plantTry = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_active && !m_destroy)
        {
            switch (gameObject.tag)
            {
                case "Pickup":
                    break;
                case "Flower":
                    if (m_plantTimerCurr > 0)
                    {
                        m_plantTimerCurr -= Time.deltaTime;
                    }
                    if (m_plantTimerCurr <= 0)
                    {
                        m_plantTimerCurr = m_plantTimerPrev * 1.2f;
                        m_plantTimerPrev = m_plantTimerCurr;
                        plantSeed();
                    }
                    break;
            }
        }

        if (m_destroy)
        {
            if (m_plantDestroy > 0)
            {
                m_plantDestroy -= Time.deltaTime;
                this.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
            }
            if (m_plantDestroy <= 0)
            {
                destroyPlant(gameObject);
                m_destroy = false;
            }
        }

        switch (gameObject.tag)
        {
            case "Pickup":
                break;
            case "Flower":
                setPlantState();
                break;
        }
    }

    // Destroy or set Inactive in wrong season
    public void setPlantState()
    {
        int plantSeason = System.Array.IndexOf(season, GameObject.FindGameObjectWithTag("SeasonManager").GetComponent<SeasonScript>().CurrentSeason);

        if (plantSeason < 0)
        {
            transform.parent.GetComponent<PlantedSeedScript>().CreatedFlower.SetActive(false);
            transform.parent.GetComponent<PlantedSeedScript>().CreatedSeed.SetActive(true);
        }
        else
        {
            //destroyPlant(this.gameObject);
        }
    }

    //Plant the same plant again
    void plantSeed()
    {
        Vector2 PlantPos = RandomOnUnitCircle2(1f);
        PlantPosition = gameObject.transform.parent.position;
        PlantPosition.x += PlantPos.x;
        PlantPosition.z += PlantPos.y;

        //Check if you can plant the seed 
        switch(checkForPlantTakeOVer())
        {
            case true:
                GameObject.FindGameObjectWithTag("FlowerManager").GetComponent<FlowerManagerScript>().PlantSeed(m_seed_nr, m_weakPlantPos);
                break;

            case false:
                if (!CheckPlantLocation(PlantPosition))
                {
                    GameObject.FindGameObjectWithTag("FlowerManager").GetComponent<FlowerManagerScript>().PlantSeed(m_seed_nr, PlantPosition);
                }
                else
                {
                    m_plantTry++;
                    switch(m_plantTry)
                    {
                        case 5:
                            m_active = false;
                            break;
                    }
                }
                break;
        }
    }

    //Check if the plant can plant there
    bool CheckPlantLocation(Vector3 plantLocation)
    {
        Collider[] plantingLocation = Physics.OverlapSphere(plantLocation, .5f);
        GameObject groundMesh = null;

        if (plantingLocation.Length == 1 && plantingLocation[0].tag == "Ground")
        {
            groundMesh = plantingLocation[0].gameObject;
            Vector3 yPos = CastToPlantVertex(plantLocation, groundMesh);
            PlantPosition.y = yPos.y;
            return false;
        }
        else
        {
            int ground = 0;
            int other = 0;

            foreach (Collider thing in plantingLocation)
            {
                switch (thing.tag)
                {
                    case "Water":
                        other++;
                        break;
                    case "Flower":
                        other++;
                        break;
                    case "Rock":
                        other++;
                        break;
                    case "Seed":
                        other++;
                        break;
                    case "Ground":
                        ground++;
                        groundMesh = thing.gameObject;
                        break;
                }
            }

            if(ground == 1 && other == 0)
            {
                Vector3 yPos = CastToPlantVertex(plantLocation, groundMesh);
                PlantPosition.y = yPos.y;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    //Check if there is a plant to take over
    bool checkForPlantTakeOVer()
    {
        Collider[] otherPlant = Physics.OverlapSphere(gameObject.transform.parent.position, 1f);

        foreach (Collider plant in otherPlant)
        {
            if(plant.tag == "Flower")
            {
                int plantTakeOver = System.Array.IndexOf(PlantSortsStronger, plant.GetComponent<PlantScript>().PlantSort);

                //print("plantTakeOver "+plantTakeOver);

                if (plantTakeOver > -1)
                {
                    m_weakPlantPos = plant.transform.parent.position;
                    plant.GetComponent<PlantScript>().setDestroy();
                    return true;
                }
            }
        }

        return false;
    }

    // Remove the plant
    public void destroyPlant(GameObject plant)
    {
        GameObject.FindGameObjectWithTag("FlowerManager").GetComponent<FlowerManagerScript>().RemovePlant(plant.gameObject);

        if (this.gameObject.transform.parent.gameObject.tag.ToString() == "RichSoil")
        {
            //print("parent name : " + plant.gameObject.transform.parent.gameObject.tag.ToString());
        }
        else
        {
            GameObject.DestroyObject(plant.gameObject.transform.parent.gameObject);
        }

        Destroy(transform.parent.GetComponent<PlantedSeedScript>().CreatedSeed);

        GameObject.DestroyObject(plant.gameObject);
    }

    //Set the pant to active so that it can create more plants
    public void setActive()
    {
        m_active = true;
    }

    //Pick a random point in radius circle
    public static Vector2 RandomOnUnitCircle2(float radius)
    {
        Vector2 randomPointOnCircle = UnityEngine.Random.insideUnitCircle;
        randomPointOnCircle.Normalize();
        randomPointOnCircle *= radius;
        return randomPointOnCircle;
    }

    //Place plant on ground
    public Vector3 CastToPlantVertex(Vector3 plant, GameObject meshObject)
    {
        Vector3 newPos = new Vector3(0, 0, 0);

        RaycastHit plantRayHit;

        Ray plantRay = new Ray();
        plant.y += 0.5f;
        plantRay.origin = plant;
        plantRay.direction = new Vector3(0, -1, 0);


        if (Physics.Raycast(plantRay, out plantRayHit))
        {
            Mesh mesh = meshObject.GetComponent<MeshFilter>().mesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Extract local space vertices that were hit
            Vector3 p0 = vertices[triangles[plantRayHit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[plantRayHit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[plantRayHit.triangleIndex * 3 + 2]];

            // Transform local space vertices to world space
            Transform hitTransform = plantRayHit.collider.transform;
            p0 = hitTransform.TransformPoint(p0);
            p1 = hitTransform.TransformPoint(p1);
            p2 = hitTransform.TransformPoint(p2);

            newPos = (p0 + p1 + p2) / 3;
            //print("p0 : " + p0 + "p1 : " + p1 + "p2 : " + p2);
        }
        return newPos;
    }

    //Set destroy mode
    public void setDestroy()
    {
        m_destroy = true;
        m_active = false;
    }
}