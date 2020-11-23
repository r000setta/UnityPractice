using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyCollections 
{
    List<Enemy> enemies=new List<Enemy>();

    public void Add(Enemy enemy){
        enemies.Add(enemy);
    }

    public void GameUpdate(){
        for(int i=0;i<enemies.Count;++i){
            if(!enemies[i].GameUpdate()){
                int lastidx=enemies.Count-1;
                enemies[i]=enemies[lastidx];
                enemies.RemoveAt(lastidx);
                i-=1;
            }
        }
    }
}
