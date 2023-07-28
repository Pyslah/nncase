﻿// Copyright (c) Canaan Inc. All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFabric.Hyperlinq;
using Nncase.IR;
using Nncase.Passes.Analysis;
using Nncase.Passes.Rules;
using Nncase.PatternMatch;
using Nncase.TIR;
using static Nncase.PatternMatch.Utility;

namespace Nncase.Passes;

/// <summary>
/// merge call/ assgin ddr buffer start/layout.
/// </summary>
public sealed class DDrBufferSchdeulePass : ModulePass
{
    private readonly Dictionary<string, Dictionary<MemoryLocation, int>> _moduleUsage = new();

    private readonly Dictionary<string, Dictionary<Const, System.Range>> _moduleRdataMaps = new();

    private readonly bool _enbaleMergeCall;

    public DDrBufferSchdeulePass(bool enableMergeCall = false)
    {
        _enbaleMergeCall = enableMergeCall;
    }

    private IAnalyzerManager AnalyzerManager => CompileSession.GetRequiredService<IAnalyzerManager>();

    /// <inheritdoc/>
    protected override async Task<IRModule> RunCoreAsync(IRModule module, RunPassContext options)
    {
        // 1. merge the all call prim func
#if false
        if (_enbaleMergeCall)
        {
            HashSet<BaseFunction> mergedFuncs = new(ReferenceEqualityComparer.Instance);
            HashSet<BaseFunction> stackvmFuncs = new(ReferenceEqualityComparer.Instance);
            for (int i = 0; i < module.Functions.Count; i++)
            {
                if (module.Functions[i] is Function { ModuleKind: "stackvm" } func)
                {
                    var analysis = new Dictionary<Type, IAnalysisResult>
                    {
                        [typeof(IExprUserAnalysisResult)] = AnalyzerManager.GetAnaylsis<IExprUserAnalysisResult>(func),
                    };
                    _ = new HashSet<BaseFunction>(ReferenceEqualityComparer.Instance);
                    var mergePass = new DataflowPass();
                    mergePass.Add<Rules.Neutral.PrimFuncMergeRule>(mergedFuncs);
                    var post = await mergePass.RunAsync(func, new() { AnalysisResults = analysis, RewriteOnce = true });
                    module.Replace(i, post);
                    stackvmFuncs.Add(post);
                }
            }

            // 2. add the ext func into module.
            foreach (var func in stackvmFuncs)
            {
                var collector = new ExternalFuncCollector();
                collector.Visit(func);
                foreach (var ext_func in collector.GetExternalFuncs())
                {
                    module.Add(ext_func);
                }
            }

            // 3. remove the all merged funcs
            foreach (var item in mergedFuncs)
            {
                module.Remove(item);
            }
        }
#endif

        // 4. schedule the prim funcs.
        for (int i = 0; i < module.Functions.Count; i++)
        {
            if (module.Functions[i] is TIR.PrimFunction prim_func)
            {
                if (!prim_func.SchedResult.IsScheduled)
                {
                    var rewriter = new DDrBufferRewriter(_moduleUsage, _moduleRdataMaps);
                    var post = (TIR.PrimFunction)rewriter.Rewrite(prim_func); // changed ddr buffer.
                    if (rewriter.IsMutated)
                    {
                        post.SchedResult.DataUsage = rewriter.DataUsage;
                        post.SchedResult.IsScheduled = true;
                    }

                    module.Replace(i, prim_func);
                }
            }
        }

        _moduleRdataMaps.Clear();
        _moduleUsage.Clear();

        return await Task.FromResult(module);
    }
}

internal sealed class DDrBufferRewriter : ExprRewriter
{
    private readonly Dictionary<MemoryLocation, int> _functionUsage;
    private readonly Dictionary<Const, System.Range> _functionRdatas;

    public DDrBufferRewriter(Dictionary<string, Dictionary<MemoryLocation, int>> moduleUsage, Dictionary<string, Dictionary<Const, System.Range>> moduleRdataMaps)
    {
        ModuleUsage = moduleUsage;
        ModuleRdataMaps = moduleRdataMaps;
        _functionUsage = new();
        _functionRdatas = new();
        Changed = false;
    }

    public Dictionary<string, Dictionary<MemoryLocation, int>> ModuleUsage { get; }

    public Dictionary<string, Dictionary<Const, System.Range>> ModuleRdataMaps { get; }

    public bool Changed { get; private set; }

    public int DataUsage => _functionUsage.GetValueOrDefault(MemoryLocation.Data, 0);

    public PrimFunction Entry => (PrimFunction)VisitRoot!;

    protected override TIR.MemSpan RewriteLeafMemSpan(TIR.MemSpan memSpan)
    {
        if (memSpan is { Location: MemoryLocation.Rdata, Start: Call { Target: IR.Buffers.DDrOf, Arguments: var arg } } && arg[0] is Const @const)
        {
            if (!ModuleRdataMaps.TryGetValue(Entry.ModuleKind, out var moduleRdataMap))
            {
                moduleRdataMap = new();
                ModuleRdataMaps.Add(Entry.ModuleKind, moduleRdataMap);
            }

            if (!ModuleUsage.TryGetValue(Entry.ModuleKind, out var moduleUsage))
            {
                moduleUsage = new();
                ModuleUsage.Add(Entry.ModuleKind, moduleUsage);
            }

            if (!moduleRdataMap.TryGetValue(@const, out var memRange))
            {
                if (!moduleUsage.TryGetValue(memSpan.Location, out var start))
                {
                    start = 0;
                }

                _ = ComputeSize(@const);
                moduleUsage[memSpan.Location] = start + ComputeSize(@const);
                memRange = start..(start + ComputeSize(@const));
                moduleRdataMap.Add(@const, memRange);
                Entry.SchedResult.Rdatas.Add(@const, memRange);
                Changed = true;
            }

            return memSpan.With(memRange.Start.Value, memRange.End.Value - memRange.Start.Value);
        }

        // else if (memSpan.Location is MemoryLocation.Data)
        // {
        //     data write into the FunctionUsage
        //     if (!_functionRdatas.Contains(physical))
        //     {
        //         if (!_functionUsage.TryGetValue(physical.Location, out var start))
        //         {
        //             start = 0;
        //         }

        // physical.Start = start;
        //         _functionUsage[physical.Location] = start + physical.Size;
        //         _functionRdatas.Add(physical);
        //         Changed = true;
        //     }
        // }
        // else if (memSpan.Location is MemoryLocation.SharedData)
        // {
        //     throw new NotSupportedException("Current Not Support!");
        // }
        return memSpan;
    }

    private int ComputeSize(IValue v) => v.AsTensors().Select(t => t.BytesBuffer.Length).Sum();

    private int ComputeSize(Const @const) => @const switch
    {
        TensorConst { Value: Tensor tc } => tc.BytesBuffer.Length,
        TupleConst tc => ComputeSize(tc.Value),
        _ => throw new NotSupportedException(),
    };
}

internal sealed class ExternalFuncCollector : ExprWalker
{
    public HashSet<BaseFunction> GetExternalFuncs()
    {
        var set = new HashSet<BaseFunction>(ReferenceEqualityComparer.Instance);
        set.UnionWith(ExprMemo.Keys.OfType<PrimFunctionWrapper>());
        set.UnionWith(set.OfType<PrimFunctionWrapper>().Select(w => w.Target).ToArray());
        return set;
    }
}
