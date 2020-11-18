using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefenseGame : MonoBehaviour
{
    Ray TouchRay=>Camera.main.ScreenPointToRay(Input.mousePosition);

    [SerializeField]
    Vector2Int boradSize=new Vector2Int(11,11);

    [SerializeField]
    GameBoard board=default;

    
	[SerializeField]
	GameTileContentFactory tileContentFactory=default;

    private void Awake() {
        board.Initialize(boradSize,tileContentFactory);
    }

    private void OnValidate() {
        if(boradSize.x<2){
            boradSize.x=2;
        }
        if(boradSize.y<2){
            boradSize.y=2;
        }
    }

    private void Update() {
        if(Input.GetMouseButtonDown(0)){
            HandleTouch();
        }
    }

    void HandleTouch(){
        DefenseGameTile tile=board.GetTile(TouchRay);
        if(tile!=null){
            board.ToggleDestination(tile);
        }
    }
}
