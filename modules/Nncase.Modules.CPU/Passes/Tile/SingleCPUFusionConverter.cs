﻿// Copyright (c) Canaan Inc. All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Runtime.CompilerServices;
using NetFabric.Hyperlinq;
using Nncase.Diagnostics;
using Nncase.IR;
using Nncase.IR.Buffers;
using Nncase.IR.CPU;
using Nncase.IR.F;
using Nncase.IR.Math;
using Nncase.Passes.Mutators;
using Nncase.PatternMatch;
using Nncase.Schedule;
using Nncase.Targets;
using Nncase.TIR;
using Nncase.TIR.Builders;
using Buffer = Nncase.TIR.Buffer;
using MathF = Nncase.IR.F.Math;
using Range = Nncase.TIR.Range;
using Tuple = Nncase.IR.Tuple;

namespace Nncase.Passes.Tile;

/// <summary>
/// convert the fusion to prim func.
/// </summary>
internal sealed class SingleCPUFusionConverter
{
    public TIR.PrimFunction Visit(Fusion fusion)
    {
        var body = new List<Expr>();
        var visitor = new ConvertVisitor(body);
        visitor.Visit(fusion);
        return T.PrimFunc(fusion.Name, fusion.ModuleKind, visitor.InputBuffers.Concat(visitor.OutputBuffers).ToArray()).Body(body.ToArray()).Build();
    }

    private sealed class ConvertVisitor : ExprVisitor<Unit, Unit>
    {
        private readonly Dictionary<Expr, TIR.Buffer> _buffersMap = new(ReferenceEqualityComparer.Instance);
        private readonly List<Expr> _mainBody;

        public ConvertVisitor(List<Expr> mainBody)
        {
            _mainBody = mainBody;
        }

        public Fusion VisitRootFusion => (Fusion)VisitRoot!;

        public IEnumerable<TIR.Buffer> OutputBuffers => _buffersMap.Values.OfType<TIR.Buffer>().Where(b => b.MemSpan.Location.HasFlag(MemoryLocation.Output));

        public IEnumerable<TIR.Buffer> InputBuffers => _buffersMap.Values.OfType<TIR.Buffer>().Where(b => b.MemSpan.Location.HasFlag(MemoryLocation.Input));

        protected override Unit DefaultVisitLeaf(Expr expr)
        {
            return default;
        }

        protected override Unit VisitLeafCall(Call expr)
        {
            var arguments = expr.Arguments.AsValueEnumerable().Select(TryAllocateBuffer).ToArray();
            var ret = TryAllocateBuffer(expr);
            var op = ((CPUKernelOp)expr.Target).Target;

            switch (op)
            {
                case Unary unary:
                    GenerateUnary(unary, arguments, ret);
                    break;
                case Binary binary:
                    GenerateBinary(binary, arguments, ret, expr);
                    break;
                case MatMul matmul:
                    GenerateMatMul(arguments, ret, expr);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return default;
        }

        private void GenerateMatMul(ReadOnlySpan<Buffer> arguments, Buffer ret, Call expr)
        {
            var lhs = arguments[0];
            var rhs = arguments[1];

            var loops = Enumerable.Range(0, lhs.Rank - 2).Select(i => (T.ForLoop(out var loopVar, (0, lhs.Dimensions[i]), i == 0 ? LoopMode.Parallel : LoopMode.Serial, $"loop_{i}"), loopVar)).ToArray();
            var loopVars = loops.Select(f => f.loopVar).ToArray();
            var stmt = T.Serial(out var m, (0, lhs.Dimensions[^2])).Body(
                T.Serial(out var n, (0, rhs.Dimensions[^1])).Body(
                    T.BufferStore(ret, loopVars.Concat(new[] { m, n }).ToArray(), 0f),
                    T.Serial(out var k, (0, lhs.Dimensions[^1])).Body(
                        T.BufferStore(ret, loopVars.Concat(new[] { m, n }).ToArray(), T.BufferLoad(ret, loopVars.Concat(new[] { m, n }).ToArray()) + (T.BufferLoad(lhs, loopVars.Concat(new[] { m, k }).ToArray()) * T.BufferLoad(rhs, loopVars.Concat(new[] { k, n }).ToArray())))))).
                Build();
            var final = loops.Reverse().Aggregate(stmt, (acc, p) => p.Item1.Body(acc).Build());

            // [m,k] @ [k, n]
            var body = T.Block(nameof(MatMul)).Body(
                T.MatchBuffer(arguments[0]),
                T.MatchBuffer(arguments[1]),
                T.MatchBuffer(ret),
                final);
            _mainBody.Add(body.Build());
        }

        private void GenerateUnary(Unary unary, ReadOnlySpan<Buffer> arguments, Buffer ret)
        {
            var input = arguments[Unary.Input.Index];
            var loops = Enumerable.Range(0, input.Rank).Select(i => (T.ForLoop(out var loopVar, (0, input.Dimensions[i]), i == 0 ? LoopMode.Parallel : LoopMode.Serial, $"loop_{i}"), loopVar)).ToArray();
            var loopVars = loops.Select(f => f.loopVar).ToArray();
            Expr stmt = T.BufferStore(ret, loopVars, IR.F.Math.Unary(unary.UnaryOp, T.BufferLoad(input, loopVars)));
            var final = loops.Reverse().Aggregate(stmt, (acc, p) => p.Item1.Body(acc).Build());
            _mainBody.Add(T.Block(nameof(Unary)).Body(
                T.MatchBuffer(arguments[0]),
                T.MatchBuffer(ret),
                final).Build());
        }

        private void GenerateBinary(Binary binary, ReadOnlySpan<Buffer> arguments, Buffer ret, Call call)
        {
            var lhs = call.Arguments[Binary.Lhs.Index];
            var rhs = call.Arguments[Binary.Rhs.Index];
            var lhsBuffer = arguments[Binary.Lhs.Index];
            var rhsBuffer = arguments[Binary.Rhs.Index];

            var outShape = call.CheckedShape.ToValueArray();
            var lhsShape = Enumerable.Repeat(1, outShape.Length).ToArray();
            Array.Copy(lhs.CheckedShape.ToValueArray(), 0, lhsShape, lhsShape.Length - lhs.CheckedShape.Rank, lhs.CheckedShape.Rank);
            var rhsShape = Enumerable.Repeat(1, outShape.Length).ToArray();
            Array.Copy(rhs.CheckedShape.ToValueArray(), 0, rhsShape, rhsShape.Length - rhs.CheckedShape.Rank, rhs.CheckedShape.Rank);

            var lhsScale = outShape.Zip(lhsShape).Select(s => s.First / s.Second).ToArray();
            var rhsScale = outShape.Zip(rhsShape).Select(s => s.First / s.Second).ToArray();

            var loops = Enumerable.Range(0, outShape.Length).Select(i => (T.ForLoop(out var loopVar, (0, outShape[i]), LoopMode.Serial, $"loop_{i}"), loopVar)).ToArray();
            var loopVars = loops.Select(f => f.loopVar).ToArray();
            var lhsLoopVars = loopVars.Zip(lhsScale).Select(v => v.First / v.Second).ToArray();
            var rhsLoopVars = loopVars.Zip(rhsScale).Select(v => v.First / v.Second).ToArray();
            Expr stmt = T.BufferStore(ret, loopVars, IR.F.Math.Binary(binary.BinaryOp, T.BufferLoad(lhsBuffer, lhsLoopVars), T.BufferLoad(rhsBuffer, rhsLoopVars)));
            var final = loops.Reverse().Aggregate(stmt, (acc, p) => p.Item1.Body(acc).Build());
            var body = T.Block(nameof(Binary)).Body(
                T.MatchBuffer(arguments[0]),
                T.MatchBuffer(arguments[1]),
                T.MatchBuffer(ret),
                final);
            _mainBody.Add(body.Build());
        }

        private TIR.Buffer TryAllocateBuffer(Expr expr)
        {
            var name = $"buffer_{_buffersMap.Keys.Count}";
            if (!_buffersMap.TryGetValue(expr, out var buffer))
            {
                switch (expr)
                {
                    case Call c:
                        if (ReferenceEquals(c, VisitRootFusion.Body))
                        {
                            buffer = T.AttachBuffer((TensorType)c.CheckedType, MemoryLocation.Output, out _, out _, name);
                        }
                        else
                        {
                            buffer = T.CreateBuffer((TensorType)c.CheckedDataType, MemoryLocation.Data, out _, name);
                        }

                        break;
                    case Var v:
                        buffer = T.AttachBuffer((TensorType)v.CheckedType, MemoryLocation.Input, out _, out _, name);
                        break;
                    case TensorConst c:
                        buffer = T.AttachBuffer(c, out _, name);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                _buffersMap.Add(expr, buffer);
            }

            return buffer;
        }
    }
}
