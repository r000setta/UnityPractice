using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    Transform ground=default;

    Queue<DefenseGameTile> searchFrontier=new Queue<DefenseGameTile>();
	
	DefenseGameTile[] tiles;

    [SerializeField]
    DefenseGameTile tilePrefab=default;

    Vector2Int size;

	GameTileContentFactory contentFactory;

    public void Initialize(Vector2Int size,GameTileContentFactory contentFactory){
        this.size=size;
		this.contentFactory=contentFactory;
        ground.localScale=new Vector3(size.x,size.y,1f);
        
        Vector2 offset = new Vector2(
			(size.x - 1) * 0.5f, (size.y - 1) * 0.5f
		);
		tiles=new DefenseGameTile[size.x*size.y];
		for (int i=0,y=0; y < size.y; y++) {
			for (int x = 0; x < size.x; x++,i++) {
				DefenseGameTile tile=Instantiate(tilePrefab);
				tiles[i]=tile;
				tile.transform.SetParent(transform, false);
				tile.transform.localPosition = new Vector3(
					x - offset.x, 0f, y - offset.y
				);
				if(x>0){
					DefenseGameTile.MakeEastWestNeightbors(tile,tiles[i-1]);
				}
				if(y>0){
					DefenseGameTile.MakeNorthSouthNeighbors(tile,tiles[i-size.x]);
				}
				tile.IsAlternative=(x&1)==0;
				if((y&1)==0){
					tile.IsAlternative=!tile.IsAlternative;
				}
				tile.Content=contentFactory.Get(GameTileContentType.Empty);
			}
		}
		ToggleDestination(tiles[tiles.Length/2]);
    }

	bool FindPaths(){
		foreach(DefenseGameTile t in tiles){
			if(t.Content.Type==GameTileContentType.Destination){
				t.BecomeDestination();
				searchFrontier.Enqueue(t);
			}else{
				t.ClearPath();
			}
		}
		if(searchFrontier.Count==0){
			return false;
		}
		while (searchFrontier.Count>0) {
			DefenseGameTile tile=searchFrontier.Dequeue();
			if(tile!=null){
				if(tile.IsAlternative){
					searchFrontier.Enqueue(tile.GrowPathNorth());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathWest());
				}
				else{
					searchFrontier.Enqueue(tile.GrowPathWest());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathNorth());
				}
			}
		}
		foreach(DefenseGameTile t in tiles){
			t.ShowPath();
		}
		return true;
	}

	public DefenseGameTile GetTile(Ray ray){
		if(Physics.Raycast(ray,out RaycastHit hit)){
			int x = (int)(hit.point.x + size.x * 0.5f);
			int y = (int)(hit.point.z + size.y * 0.5f);
			if (x >= 0 && x < size.x && y >= 0 && y < size.y) {
				return tiles[x + y * size.x];
			}
		}
		return null;
	}

	public void ToggleDestination(DefenseGameTile tile){
		if(tile.Content.Type==GameTileContentType.Destination){
			tile.Content=contentFactory.Get(GameTileContentType.Empty);
			if(!FindPaths()){
				tile.Content=contentFactory.Get(GameTileContentType.Destination);
				FindPaths();
			}
		}
		else{
			tile.Content=contentFactory.Get(GameTileContentType.Destination);
			FindPaths();
		}
	}
}
