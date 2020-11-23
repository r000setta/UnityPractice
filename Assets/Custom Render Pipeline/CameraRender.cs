using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRender : MonoBehaviour
{

    static ShaderTagId unlitShaderTagId=new ShaderTagId("SRPDefaultUnlit"),
    litShaderTagId=new ShaderTagId("CustomLit");

    const string bufferName="Render Camera";

    CommandBuffer buffer=new CommandBuffer{
        name=bufferName
    };
    ScriptableRenderContext context;

    Camera camera;

    CullingResults cullingResults;

    Lighting lighting=new Lighting();

    public void Render(ScriptableRenderContext context,Camera camera,
                        bool useDynamicBatching,bool useGPUInstancing){
        this.context=context;
        this.camera=camera;
        PrepareBuffer();
        PrepareForSceneWindow();
        if(!Cull()){
            return;
        }
        
        SetUp();
        lighting.Setup(context,cullingResults);
        DrawVisibleGeometry(useDynamicBatching,useGPUInstancing);
        DrawUnsupportedShaders();
        DrawGizmos();
        Submit();
    }

    void DrawVisibleGeometry(bool useDynamicBatching,bool useGPUInstancing){
        var sortingSettings=new SortingSettings(camera){
            criteria=SortingCriteria.CommonOpaque
        };

        var drawingSettings=new DrawingSettings(unlitShaderTagId,sortingSettings){
            enableDynamicBatching=useDynamicBatching,
            enableInstancing=useGPUInstancing
        };
        drawingSettings.SetShaderPassName(1,litShaderTagId);
        var filteringSetting=new FilteringSettings(RenderQueueRange.opaque);
        context.DrawRenderers(
            cullingResults,ref drawingSettings,ref filteringSetting
        );
        context.DrawSkybox(camera);

        sortingSettings.criteria=SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings=sortingSettings;
        filteringSetting.renderQueueRange=RenderQueueRange.transparent;

        context.DrawRenderers(
            cullingResults,ref drawingSettings,ref filteringSetting
        );
    }

    void Submit(){
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void SetUp(){
        context.SetupCameraProperties(camera);
        CameraClearFlags flags=camera.clearFlags;
        buffer.ClearRenderTarget(
            flags<=CameraClearFlags.Depth,
            flags==CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
				camera.backgroundColor.linear : Color.clear
        );
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    void ExecuteBuffer(){
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    bool Cull(){
        if(camera.TryGetCullingParameters(out ScriptableCullingParameters parameters)){
            cullingResults=context.Cull(ref parameters);
            return true;
        }
        return false;
    }
}
