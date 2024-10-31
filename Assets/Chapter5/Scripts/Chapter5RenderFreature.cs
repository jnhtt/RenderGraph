//Renference : Universal RP URP RenderGraph Samples
//How to import PacakgeManager > Universal RP > Samples > URP RenderGraph Samples
//How to access Assets > Universal RP > 17.0.3 > URP RenderGraph Samples > Compute

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

public class Chapter5RenderFreature : ScriptableRendererFeature
{
    public class Chapter5RenderPass : ScriptableRenderPass
    {
        private const string PASS_NAME = "Chapter5RenderPass";
        //リソースハンドルと参照を保持するためのクラス
        //必須:RenderGraphBuilder.SetRenderFuncで使う
        private class PassData
        {
            public ComputeShader computeShader;
            public BufferHandle input;
            public BufferHandle output;
        }

        private ComputeShader computeShader;
        private GraphicsBuffer inputBuffer;
        private GraphicsBuffer outputBuffer;

        int[] outputData = new int[20];

        //コンストラクタ内でRenderPass内で固定できるものを解決する
        public Chapter5RenderPass(ComputeShader cs)
        {
            computeShader = cs;
            inputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 20, sizeof(int));
            var list = new List<int>();
            for (int i = 0; i < 20; i++)
            {
                list.Add(i);
            }
            inputBuffer.SetData(list);
            outputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 20, sizeof(int));
            outputBuffer.SetData(list);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            outputBuffer.GetData(outputData);
            Debug.Log($"Output from compute shader: {string.Join(", ", outputData)}");

            BufferHandle inputHandle = renderGraph.ImportBuffer(inputBuffer);
            BufferHandle outputHandle = renderGraph.ImportBuffer(outputBuffer);

            using (IComputeRenderGraphBuilder builder = renderGraph.AddComputePass(PASS_NAME, out PassData passData))
            {
                // Set the pass data so the data can be transfered from the recording to the execution.
                passData.computeShader = computeShader;
                passData.input = inputHandle;
                passData.output = outputHandle;

                builder.UseBuffer(passData.input);
                builder.UseBuffer(passData.output, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, ComputeGraphContext computeGraphContext) => ExecutePass(data, computeGraphContext));
            }
        }

        private static void ExecutePass(PassData passData, ComputeGraphContext graphContext)
        {
            graphContext.cmd.SetComputeBufferParam(passData.computeShader, passData.computeShader.FindKernel("CSMain"), "inputData", passData.input);
            graphContext.cmd.SetComputeBufferParam(passData.computeShader, passData.computeShader.FindKernel("CSMain"), "outputData", passData.output);
            graphContext.cmd.DispatchCompute(passData.computeShader, passData.computeShader.FindKernel("CSMain"), 1, 1, 1);
        }
    }

    [SerializeField] private ComputeShader computeShader;

    private Chapter5RenderPass renderPass;

    public override void Create()
    {
        renderPass = new Chapter5RenderPass(computeShader);
        renderPass.renderPassEvent = RenderPassEvent.BeforeRendering;//描画前に実行
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!SystemInfo.supportsComputeShaders)
        {
            Debug.LogWarning("Device does not support compute shaders. The pass will be skipped.");
            return;
        }
        if (computeShader == null)
        {
            Debug.LogWarning("The compute shader is null. The pass will be skipped.");
            return;
        }
        renderer.EnqueuePass(renderPass);
    }
}
