//Reference1 : Universal RP URP RenderGraph Samples
//How to import PacakgeManager > Universal RP > Samples > URP RenderGraph Samples
//How to access Assets > Universal RP > 17.0.3 > URP RenderGraph Samples > Compute
//Reference2 : https://github.com/robin-boucher/URPRendererFeatureExample6/tree/master/Assets/Samples/

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

public class Chapter2RenderFreature : ScriptableRendererFeature
{
    public class Chapter2RenderPass : ScriptableRenderPass
    {
        private const string BLIT_CAMERA_COLOR_RT_TO_TEMP_RT = "Blit2TempRT";
        private const string BLIT_TEMP_RT_TO_CAMERA_COLOR_RT = "Blit2CameraColorRT";

        //リソースハンドルと参照を保持するためのクラス
        //必須:RenderGraphBuilder.SetRenderFuncで使う
        private class PassData
        {
            public TextureHandle sourceTextureHandle;
            public Material material;
        }

        private Material material;

        //コンストラクタ内でRenderPass内で固定できるものを解決する
        public Chapter2RenderPass(RenderPassEvent passEvent, Material mat)
        {
            profilingSampler = new ProfilingSampler(nameof(Chapter2RenderPass));
            renderPassEvent = passEvent;
            material = mat;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
            {
                return;
            }

            // 1:カメラのカラーバッファの内容にフルスクリーンエフェクトを適用して一時的なレンダーテクスチャーに書き込み
            // 2:一時的なレンダーテクスチャーの内容を一時的なレンダーテクスチャーにコピー
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            //カメラカラーバッファのテクスチャハンドル
            TextureHandle cameraColorTextureHandle = resourceData.activeColorTexture;
            RenderTextureDescriptor descriptor = cameraData.cameraTargetDescriptor;
            descriptor.msaaSamples = 1;
            descriptor.depthBufferBits = 0;

            //1:一時的なレンダーテクスチャ
            TextureHandle tempTextureHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_TmepRT", true);

            //カメラのカラーバッファをマテリアルを適用しながら一時的なレンダーテクスチャーに書き込む
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(BLIT_CAMERA_COLOR_RT_TO_TEMP_RT, out PassData passData, profilingSampler))
            {
                builder.UseTexture(cameraColorTextureHandle, AccessFlags.Read);//src
                builder.SetRenderAttachment(tempTextureHandle, 0, AccessFlags.Write);//dest

                passData.sourceTextureHandle = cameraColorTextureHandle;
                passData.material = material;

                builder.SetRenderFunc((PassData passData, RasterGraphContext graphContext) => ExecutePass(passData, graphContext));
            }

            //2:一時的なレンダーテクスチャーの内容を一時的なレンダーテクスチャーにコピー
            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass(BLIT_TEMP_RT_TO_CAMERA_COLOR_RT, out PassData passData, profilingSampler))
            {
                builder.UseTexture(tempTextureHandle, AccessFlags.Read);//src
                builder.SetRenderAttachment(cameraColorTextureHandle, 0, AccessFlags.Write);//dest

                passData.sourceTextureHandle = tempTextureHandle;
                passData.material = null;
                builder.SetRenderFunc((PassData passData, RasterGraphContext graphContext) => ExecutePass(passData, graphContext));

            }
        }

        private static void ExecutePass(PassData passData, RasterGraphContext graphContext)
        {
            RasterCommandBuffer cmd = graphContext.cmd;
            if (passData.material == null)
            {
                //コピー
                Blitter.BlitTexture(cmd, passData.sourceTextureHandle, new Vector4(1, 1, 0, 0), 0, false);
            }
            else
            {
                //フルスクリーンエフェクトをかけて書き込む
                Blitter.BlitTexture(cmd, passData.sourceTextureHandle, new Vector4(1, 1, 0, 0), passData.material, 0);
            }
        }

        public void Dispose()
        {
            //RenderPassのリソースを開放する処理
        }
    }

    [SerializeField] private RenderPassEvent renderPassEvent;
    [SerializeField] private Material material;

    private Chapter2RenderPass renderPass;

    public override void Create()
    {
        renderPass = new Chapter2RenderPass(renderPassEvent, material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        renderPass.Dispose();
    }
}
