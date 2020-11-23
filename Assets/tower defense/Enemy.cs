using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    EnemyFactory originFactory;

    DefenseGameTile tileFrom,tileTo;

    Vector3 positionFrom,positionTo;

    float progress,progressFactor;

    Direction direction;
    DirectionChange directionChange;
    float directionAngleFrom,directionAngleTo;

    [SerializeField]
    Transform model=default;

    public EnemyFactory OriginFactory{
        get=>originFactory;
        set{
            Debug.Assert(originFactory==null,"Redefined origin factory!");
            originFactory=value;
        }
    }

    public void SpawnOn(DefenseGameTile tile){
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom=tile;
        tileTo=tile.NextTileOnPath;
        progress=0f;
        PrepareIntro();
    }

    public bool GameUpdate(){
        progress+=Time.deltaTime*progressFactor;
        while(progress>=1f){
            tileFrom=tileTo;
            tileTo=tileTo.NextTileOnPath;
            if(tileTo==null){
                originFactory.Reclaim(this);
                return false;
            }
            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }
        transform.localPosition =
			Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        if (directionChange == DirectionChange.None) {
            transform.localPosition =
				Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        }else{
			float angle = Mathf.LerpUnclamped(
				directionAngleFrom, directionAngleTo, progress
			);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}
        return true;
    }

    void PrepareIntro () {
		positionFrom = tileFrom.transform.localPosition;
		positionTo = tileFrom.ExitPoint;
		direction = tileFrom.PathDirection;
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		transform.localRotation = direction.GetRotation();
        progressFactor = 2f;
	}

    void PrepareNextState () {
		positionFrom = positionTo;
		positionTo = tileFrom.ExitPoint;
		directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
		direction = tileFrom.PathDirection;
		directionAngleFrom = directionAngleTo;

        switch (directionChange) {
			case DirectionChange.None: PrepareForward(); break;
			case DirectionChange.TurnRight: PrepareTurnRight(); break;
			case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
			default: PrepareTurnAround(); break;
		}
	}

    void PrepareForward () {
		transform.localRotation = direction.GetRotation();
		directionAngleTo = direction.GetAngle();
        model.localPosition=Vector3.zero;
        progressFactor = 1f;
	}

    void PrepareTurnRight () {
		directionAngleTo = directionAngleFrom + 90f;
        model.localPosition=new Vector3(-0.5f,0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
	}

	void PrepareTurnLeft () {
		directionAngleTo = directionAngleFrom - 90f;
        model.localPosition=new Vector3(0.5f,0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        progressFactor = 1f / (Mathf.PI * 0.25f);
	}

	void PrepareTurnAround () {
		directionAngleTo = directionAngleFrom + 180f;
        model.localPosition=Vector3.zero;
        transform.localPosition = positionFrom;
        progressFactor = 1f / (Mathf.PI * 0.25f);
	}
}
