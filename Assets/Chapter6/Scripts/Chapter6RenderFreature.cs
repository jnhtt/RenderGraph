//Reference1 : Universal RP URP RenderGraph Samples
//How to import PacakgeManager > Universal RP > Samples > URP RenderGraph Samples
//How to access Assets > Universal RP > 17.0.3 > URP RenderGraph Samples > Compute
//Reference2 : https://github.com/robin-boucher/URPRendererFeatureExample6/tree/master/Assets/Samples/7_Blit_UnsafePass

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class Chapter6RendererFeature : ScriptableRendererFeature
{
    public class Chapter6RenderPass : ScriptableRenderPass
    {
        private const string PASS_NAME = "Chapter6RenderPass";

        private Material material;

        private class PassData
        {
            public TextureHandle tempTextureHandle;
            public TextureHandle cameraColorTextureHandle;
            public Material material;
        }

        public Chapter6RenderPass(RenderPassEvent passEvent, Material mat)
        {
            profilingSampler = new ProfilingSampler(nameof(Chapter6RenderPass));
            material = mat;
            renderPassEvent = passEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (this.material == null)
            {
                return;
            }

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            using (IUnsafeRenderGraphBuilder builder = renderGraph.AddUnsafePass(PASS_NAME, out PassData passData, this.profilingSampler))
            {
                TextureHandle cameraColorTextureHandle = resourceData.activeColorTexture;

                RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 0;

                TextureHandle tempTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_TempRT", true);

                builder.UseTexture(cameraColorTextureHandle, AccessFlags.ReadWrite);
                builder.UseTexture(tempTextureHandle, AccessFlags.ReadWrite);

                builder.SetGlobalTextureAfterPass(tempTextureHandle, Shader.PropertyToID("_Chapter6Tex"));

                passData.tempTextureHandle = tempTextureHandle;
                passData.cameraColorTextureHandle = cameraColorTextureHandle;
                passData.material = this.material;
                builder.SetRenderFunc((PassData passData, UnsafeGraphContext graphContext) => ExecutePass(passData, graphContext));
            }
        }

        private static void ExecutePass(PassData passData, UnsafeGraphContext graphContext)
        {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(graphContext.cmd);
            Blitter.BlitCameraTexture(cmd, passData.cameraColorTextureHandle, passData.tempTextureHandle);
            Blitter.BlitCameraTexture(cmd, passData.tempTextureHandle, passData.cameraColorTextureHandle, passData.material, 0);
        }

        public void Dispose()
        {
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    [SerializeField] private Material material = null;

    private Chapter6RenderPass renderPass;

    public override void Create()
    {
        renderPass = new Chapter6RenderPass(renderPassEvent, material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {

        renderer.EnqueuePass(this.renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        renderPass.Dispose();
    }
}
