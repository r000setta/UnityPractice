using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : PersistableObject
{
    [SerializeField]
    ShapeFactory shapeFactory;

    [SerializeField]
    PersistentStorage storage;

    public KeyCode createKey=KeyCode.C;
    public KeyCode newGameKey=KeyCode.N;
    public KeyCode saveKey=KeyCode.S;
    public KeyCode loadKey=KeyCode.L;

    public KeyCode destorykey=KeyCode.X;
    const int saveVersion=2;

    public float CreationSpeed{get;set;}
    public float DestructionSpeed{get;set;}

    float creationProgress,destructionProgress;

    int loadLevelBuildIdx;

    List<Shape> shapes;

    public int levelCount=2;

    [SerializeField]
    SpawnZone spawnZone;

    private void Update() {
        if(Input.GetKeyDown(createKey)){
            CreateObject();
        }
        else if(Input.GetKeyDown(newGameKey)){
            BeginNewGame();    
        }else if (Input.GetKeyDown(saveKey)) {
			storage.Save(this,saveVersion);
		}
		else if (Input.GetKeyDown(loadKey)) {
			BeginNewGame();
			storage.Load(this);
		}else if (Input.GetKeyDown(destorykey)) {
			DestoryShape();
		}else{
            for(int i=1;i<=levelCount;++i){
                if(Input.GetKeyDown(KeyCode.Alpha0+i)){
                    BeginNewGame();
                    StartCoroutine(LoadLevel(i));
                    return;
                }
            }
        }
        creationProgress+=Time.deltaTime*CreationSpeed;
        while(creationProgress>=1f){
            creationProgress-=1f;
            CreateObject();
        }
        destructionProgress += Time.deltaTime * DestructionSpeed;
		while (destructionProgress >= 1f) {
			destructionProgress -= 1f;
			DestoryShape();
		}
    }

    private void Start() {
        shapes=new List<Shape>();
        if(Application.isEditor){
            for(int i=0;i<SceneManager.sceneCount;++i){
                Scene loadedScene=SceneManager.GetSceneAt(i);
                if(loadedScene.name.Contains("level ")){
                    SceneManager.SetActiveScene(loadedScene);
                    loadLevelBuildIdx=loadedScene.buildIndex;
                    return;
                }
            }
        }
        StartCoroutine(LoadLevel(1));
    }

    void BeginNewGame(){
        for(int i=0;i<shapes.Count;++i){
            shapeFactory.Reclaim(shapes[i]);
        }
        shapes.Clear();
    }

    void CreateObject(){
        Shape o=shapeFactory.GetRandom();
        Transform t = o.transform;
        t.localPosition=spawnZone.SpawnPoint;
        t.localRotation=Random.rotation;
        t.localScale=Vector3.one*Random.Range(0.1f,1f);
        o.SetColor(Random.ColorHSV(
			hueMin: 0f, hueMax: 1f,
			saturationMin: 0.5f, saturationMax: 1f,
			valueMin: 0.25f, valueMax: 1f,
			alphaMin: 1f, alphaMax: 1f
		));
        shapes.Add(o);
    }

    public override void Save(GameDataWriter writer){
        writer.Write(shapes.Count);
        writer.Write(loadLevelBuildIdx);
        for(int i=0;i<shapes.Count;++i){
            writer.Write(shapes[i].ShapeId);
            writer.Write(shapes[i].MaterialId);
            shapes[i].Save(writer);
        }
    }

    public override void Load (GameDataReader reader) {
        int version=reader.Version;
        if (version > saveVersion) {
			Debug.LogError("Unsupported future save version " + version);
			return;
		}
		int count = version <= 0 ? -version : reader.ReadInt();
        StartCoroutine(LoadLevel(version<2?1:reader.ReadInt()));
		for (int i = 0; i < count; i++) {
            int shapeId=version>0?reader.ReadInt():0;
            int materialId=version>0?reader.ReadInt():0;
			Shape instance=shapeFactory.Get(shapeId,materialId);
            instance.Load(reader);
            shapes.Add(instance);
		}
	}

    void DestoryShape(){
        if(shapes.Count>0){
            int idx=Random.Range(0,shapes.Count);
            shapeFactory.Reclaim(shapes[idx]);
            int lastIdx=shapes.Count-1;
            shapes[idx]=shapes[lastIdx];
            shapes.RemoveAt(lastIdx);
        }
    }

    IEnumerator LoadLevel(int levelBuildIdx){
        enabled=false;
        if(loadLevelBuildIdx>0){
            yield return SceneManager.UnloadSceneAsync(loadLevelBuildIdx);
        }
        yield return SceneManager.LoadSceneAsync(
            levelBuildIdx,LoadSceneMode.Additive
        );
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(levelBuildIdx));
        loadLevelBuildIdx=levelBuildIdx;
        enabled=true;
    }
}
