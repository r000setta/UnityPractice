using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : PersistableObject
{
    static MaterialPropertyBlock shaderPropertyBlock;

    static int colorPropertyId = Shader.PropertyToID("_Color");
    MeshRenderer meshRenderer;
    int shapeId=int.MinValue;
    public int ShapeId{
        get{
            return shapeId;
        }
        set{
            if(shapeId==int.MinValue&&value!=int.MinValue){
                shapeId=value;
            }else{
                Debug.LogError("Not allowed!");
            }
        }
    }

    public int MaterialId{get;private set;}

    Color color;

    private void Awake() {
        meshRenderer=GetComponent<MeshRenderer>();
    }

    public void SetMaterial(Material material,int materialId){
        meshRenderer.material=material;
        MaterialId=materialId;
    }

    public void SetColor(Color color){
        this.color=color;
        if(shaderPropertyBlock==null){
            shaderPropertyBlock=new MaterialPropertyBlock();
        }
        shaderPropertyBlock.SetColor(colorPropertyId,color);
        meshRenderer.SetPropertyBlock(shaderPropertyBlock);
    }

    public override void Save(GameDataWriter writer)
    {
        base.Save(writer);
        writer.Write(color);
    }

    public override void Load(GameDataReader reader)
    {
        base.Load(reader);
        SetColor(reader.Version>0?reader.ReadColor():Color.white);
    }
}
