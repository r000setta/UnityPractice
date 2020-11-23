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
    EnemyFactory enemyFactory=default;

    [SerializeField,Range(0.1f,10f)]
    float spawnSpeed=1f;

    
	[SerializeField]
	GameTileContentFactory tileContentFactory=default;

    EnemyCollections enemies=new EnemyCollections();

    float spawnProgress;

    private void Awake() {
        board.Initialize(boradSize,tileContentFactory);
        board.ShowGrid=true;
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
        }else if(Input.GetMouseButtonDown(1)){
            HandleAlternativeTouch();
        }
        if(Input.GetKeyDown(KeyCode.V)){
            board.ShowPaths=!board.ShowPaths;
        }
        if(Input.GetKeyDown(KeyCode.G)){
            board.ShowGrid=!board.ShowGrid;
        }
        spawnProgress+=spawnSpeed*Time.deltaTime;
        while(spawnProgress>=1f){
            spawnProgress-=1f;
            SpawnEnemy();
        }
        enemies.GameUpdate();
    }

    void SpawnEnemy(){
        DefenseGameTile spawnPoint=board.GetSpawnPoint(Random.Range(0,board.SpawnPointCount));
        Enemy enemy=enemyFactory.Get();
        enemy.SpawnOn(spawnPoint);
        enemies.Add(enemy);
    }

    void HandleAlternativeTouch(){
        DefenseGameTile tile=board.GetTile(TouchRay);
        if(tile!=null){
            if(Input.GetKey(KeyCode.LeftShift)){
                board.ToggleDestination(tile);
            }else{
                board.ToggleSpawnPoint(tile);
            }
        }
    }

    void HandleTouch(){
        DefenseGameTile tile=board.GetTile(TouchRay);
        if(tile!=null){
            board.ToggleWall(tile);
        }
    }
}
