﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseGameTile : MonoBehaviour
{
	DefenseGameTile north,east,south,west,nextOnPath;

    public DefenseGameTile GrowPathNorth()=>GrowPathTo(north);
    public DefenseGameTile GrowPathEast()=>GrowPathTo(east);
    public DefenseGameTile GrowPathSouth()=>GrowPathTo(south);
    public DefenseGameTile GrowPathWest()=>GrowPathTo(west);

    public bool IsAlternative{get;set;}

    GameTileContent content;

    int distance;

    static Quaternion
        northRotation=Quaternion.Euler(90f,0f,0f),
        eastRotation=Quaternion.Euler(90f,90f,0f),
        southRotation = Quaternion.Euler(90f, 180f, 0f),
		westRotation = Quaternion.Euler(90f, 270f, 0f);

    [SerializeField]
    Transform arrow=default;

    public bool HasPath=>distance!=int.MaxValue;

    public static void MakeEastWestNeightbors(DefenseGameTile east,DefenseGameTile west){
        Debug.Assert(
            west.east==null&&east.west==null,"Redefined neightbors!"
        );
        west.east=east;
        east.west=west;
    }

    public static void MakeNorthSouthNeighbors (DefenseGameTile north, DefenseGameTile south) {
		Debug.Assert(
			south.north == null && north.south == null, "Redefined neighbors!"
		);
		south.north = north;
		north.south = south;
	}

    public void ClearPath(){
        distance=int.MaxValue;
        nextOnPath=null;
    }

    public void BecomeDestination(){
        distance=0;
        nextOnPath=null;
    }

    DefenseGameTile GrowPathTo(DefenseGameTile neighbor){
        Debug.Assert(HasPath,"No Path!");
        if(neighbor==null||neighbor.HasPath){
            return null;
        }
        neighbor.distance=distance+1;
        neighbor.nextOnPath=this;
        return neighbor;
    }

    public void ShowPath(){
        if(distance==0){
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
        arrow.localRotation =
			nextOnPath == north ? northRotation :
			nextOnPath == east ? eastRotation :
			nextOnPath == south ? southRotation :
			westRotation;
    }

    public GameTileContent Content{
        get=>content;
        set{
            Debug.Assert(value!=null,"Null assigned to content!");
            if(content!=null){
                content.Recycle();
            }
            content=value;
            content.transform.localPosition=transform.localPosition;
        }
    }
}
