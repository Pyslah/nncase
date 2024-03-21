﻿// Copyright (c) Canaan Inc. All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DryIoc.ImTools;
using NetFabric.Hyperlinq;
using Nncase.IR;
using Nncase.IR.Math;
using Nncase.IR.Ncnn;
using Nncase.PatternMatch;

using static Nncase.IR.F.Ncnn;
using static Nncase.IR.F.Tensors;
using static Nncase.IR.TypePatternUtility;
using static Nncase.PatternMatch.F.Math;
using static Nncase.PatternMatch.Utility;

namespace Nncase.Passes.Rules.Ncnn;

[RuleGenerator]
public partial class LowerBinary : RewriteRule<Pattern>
{
    /// <inheritdoc/>
    public override Pattern Pattern { get; } = IsBinary(
      target_name: "binary",
      _ => true,
      IsWildcard("inputA") with { TypePattern = IsFloat() },
      IsWildcard("inputB") with { TypePattern = IsFloat() });

    private static BinaryOperationType? MapBinaryOp(BinaryOp binaryOp) =>
        binaryOp switch
        {
            BinaryOp.Add => BinaryOperationType.ADD,
            BinaryOp.Sub => BinaryOperationType.SUB,
            BinaryOp.Mul => BinaryOperationType.MUL,
            BinaryOp.Div => BinaryOperationType.DIV,
            BinaryOp.Max => BinaryOperationType.MAX,
            BinaryOp.Min => BinaryOperationType.MIN,
            BinaryOp.Pow => BinaryOperationType.POW,
            _ => null,

            // unsupported Binary ops
            // BinaryOp.Mod =>
            // BitwiseAnd
            // BitwiseOr
            // BitwiseXor
            // LogicalAnd
            // LogicalOr
            // LogicalXor
            // LeftShift
            // RightShift
            // => BinaryOperationType.RSUB,
            // => BinaryOperationType.RDIV,
            // => BinaryOperationType.RPOW,
            // => BinaryOperationType.ATAN2,
            // => BinaryOperationType.RATAN2,
        };

    private int[] FixShape(int[] shape, int r)
    {
        var newShape = shape.ToList();
        for (int i = r - shape.Length; i > 0; i--)
        {
            newShape.Insert(0, 1);
        }

        return newShape.ToArray();
    }

    private Expr? GetReplace(Binary binary, Expr inputA, Expr inputB)
    {
        if (MapBinaryOp(binary.BinaryOp) is BinaryOperationType op)
        {
            int r = Math.Max(inputA.CheckedShape.Rank, inputB.CheckedShape.Rank);
            if (r > 4)
            {
                return null;
            }

            Call b;

            if (inputA is Const)
            {
                // A
                var constA = ((TensorConst)inputA).Value;
                var aShape = FixShape(inputA.CheckedShape.ToValueArray(), r).ToList();

                // B
                var newB = Reshape(inputB, FixShape(inputB.CheckedShape.ToValueArray(), r));
                var newInputB = new Var(newB.CheckedType);

                // Constant can not support 4D unless 0-D is 1.
                if (r == 4)
                {
                    if (newB.CheckedShape[0].FixedValue != 1 || aShape[0] != 1)
                    {
                        return null;
                    }

                    newB = Squeeze(newB, new[] { 0 });
                    newInputB = new Var(newB.CheckedType);
                }

                while (aShape[0] == 1 && aShape.Count > 3 && aShape.Count > newB.CheckedShape.Count)
                {
                    aShape.RemoveAt(0);
                }

                b = new Call(new Fusion("ncnn", NcnnBinary(new Expr[] { newInputB }, op, 1, constA.ToArray<float>(), aShape.ToArray()), new[] { newInputB }), newB);
            }
            else if (inputB is Const)
            {
                // A
                var newA = Reshape(inputA, FixShape(inputA.CheckedShape.ToValueArray(), r));
                var newInputA = new Var(newA.CheckedType);

                // B
                var constB = ((TensorConst)inputB).Value;
                var bShape = FixShape(inputB.CheckedShape.ToValueArray(), r).ToList();

                if (r == 4)
                {
                    if (newA.CheckedShape[0].FixedValue != 1 || bShape[0] != 1)
                    {
                        return null;
                    }

                    newA = Squeeze(newA, new[] { 0 });
                    newInputA = new Var(newA.CheckedType);
                }

                while (bShape[0] == 1 && bShape.Count > 3 && bShape.Count > newA.CheckedShape.Count)
                {
                    bShape.RemoveAt(0);
                }

                b = new Call(new Fusion("ncnn", NcnnBinary(new Expr[] { newInputA }, op, 2, constB.ToArray<float>(), bShape.ToArray()), newInputA), newA);
            }
            else
            {
                if (r < 4)
                {
                    var newInputA = new Var(inputA.CheckedType);
                    var newInputB = new Var(inputB.CheckedType);
                    b = new Call(new Fusion("ncnn", NcnnBinary(new Expr[] { newInputA, newInputB }, op, 0, null, null), new[] { newInputA, newInputB }), inputA, inputB);
                }
                else
                {
                    var newA = inputA;
                    var newB = inputB;
                    var newInputA = new Var(newA.CheckedType);
                    var newInputB = new Var(newB.CheckedType);
                    if (inputA.CheckedShape[0].FixedValue != 1 && inputA.CheckedShape.Rank == r)
                    {
                        return null;
                    }
                    else
                    {
                        newA = Squeeze(inputA, new[] { 0 });
                        newInputA = new Var(newA.CheckedType);
                    }

                    if (inputB.CheckedShape[0].FixedValue != 1 && inputB.CheckedShape.Rank == r)
                    {
                        var aa = inputB.CheckedShape.AsEnumerable().Select(x => x.FixedValue).ToArray();
                        return null;
                    }
                    else
                    {
                        newB = Squeeze(inputB, new[] { 0 });
                        newInputB = new Var(newB.CheckedType);
                    }

                    b = new Call(new Fusion("ncnn", NcnnBinary(new Expr[] { newInputA, newInputB }, op, 0, null, null), new[] { newInputA, newInputB }), newA, newB);
                }
            }

            if (r == 4)
            {
                return Unsqueeze(b, new[] { 0 });
            }
            else
            {
                return b;
            }
        }

        return null;
    }
}
