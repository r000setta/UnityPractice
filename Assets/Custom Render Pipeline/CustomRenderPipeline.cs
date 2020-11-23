using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    bool useDynamicBatching,useGPUInstancing;
    public CustomRenderPipeline(
        bool useDynamicBatching,bool useGPUInstancing,bool useSPRBatcher
    ){
        this.useDynamicBatching=useDynamicBatching;
        this.useGPUInstancing=useGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching=useSPRBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    CameraRender renderer=new CameraRender();
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach(Camera camera in cameras){
            renderer.Render(context,camera,useDynamicBatching,useGPUInstancing);
        }
    }
}
