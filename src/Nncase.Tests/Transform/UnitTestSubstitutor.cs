using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Nncase.IR;
using Nncase.TIR;
using Nncase.Transform;
using Xunit;
using static Nncase.IR.F.Math;
using static Nncase.IR.F.Tensors;

namespace Nncase.Tests.TransformTest;

public sealed class UnitTestSubstitutor : TestFixture.UnitTestFixtrue
{
    /// <summary>
    /// the substitutor can't change the inner function var
    /// </summary>
    [Fact]
    public void TestSubstitutorFailed()
    {
        var passOptions = GetPassOptions();

        var loop_i = new Var("loop_i", TensorType.Scalar(DataTypes.Int32));
        var prim_func_1 = T.PrimFunc("prim_func_1", "k?", T.PhysicalBuffer(DataTypes.Float32, Schedule.MemoryLocation.Input, new[] { 1, 2, 3, 4 }, out var input_a), T.PhysicalBuffer(DataTypes.Float32, Schedule.MemoryLocation.Output, new[] { 1, 2, 3, 4 }, out var input_b)).Body(
          T.Load(T.Handle("hd", DataTypes.Float32), loop_i)
        ).Build();

        var prim_wrapper = new PrimFunctionWrapper(prim_func_1, 1);

        var input = new Var("input", new TensorType(DataTypes.Float32, new[] { 1, 2, 3, 4 }));
        var main_func = new Function("main", new Call(prim_wrapper, ImmutableArray.Create<Expr>(input)), ImmutableArray.Create<Var>(input));

        Assert.True(CompilerServices.InferenceType(main_func));

        Dictionary<Expr, Expr> vmap = new() { { loop_i, 1 } };
        var substitutor = Transform.Mutator.Substitute(e => vmap.TryGetValue(e, out var res) ? res : null)();

        var main_func_2 = substitutor.Visit(main_func);
        Assert.True(object.ReferenceEquals(main_func, main_func_2));
    }

    /// <summary>
    /// Substitute the prim func var
    /// </summary>
    [Fact]
    public void TestSubstitutorTrue()
    {
        var passOptions = GetPassOptions();

        var loop_i = new Var("loop_i", TensorType.Scalar(DataTypes.Int32));
        var prim_func_1 = T.PrimFunc("prim_func_1", "k?", T.PhysicalBuffer(DataTypes.Float32, Schedule.MemoryLocation.Input, new[] { 1, 2, 3, 4 }, out var input_a), T.PhysicalBuffer(DataTypes.Float32, Schedule.MemoryLocation.Output, new[] { 1, 2, 3, 4 }, out var input_b)).Body(
          T.Load(T.Handle("hd", DataTypes.Float32), loop_i)
        ).Build();

        Dictionary<Expr, Expr> vmap = new() { { loop_i, 1 } };
        var substitutor = Transform.Mutator.Substitute(e => vmap.TryGetValue(e, out var res) ? res : null)();

        var prim_func_2 = substitutor.Visit(prim_func_1);
        Assert.True(prim_func_2 is PrimFunction { Body: Sequential { Count: 1 } sequential } && sequential[0] is Call { Parameters: IRArray<Expr> parameters } && parameters[1] is TensorConst);
    }

    /// <summary>
    /// visit the stackvm var
    /// </summary>
    [Fact]
    public void TestSubstitutorTrue2()
    {
        var passOptions = GetPassOptions();

        var loop_i = new Var("loop_i", TensorType.Scalar(DataTypes.Int32));
        var prim_func_1 = T.PrimFunc("prim_func_1", "k?", T.PhysicalBuffer(DataTypes.Float32, Schedule.MemoryLocation.Input, new[] { 1, 2, 3, 4 }, out var input_a), T.PhysicalBuffer(DataTypes.Int32, Schedule.MemoryLocation.Output, new[] { 1, 2, 3, 4 }, out var input_b)).Body(
          T.Load(T.Handle("hd", DataTypes.Float32), loop_i)
        ).Build();

        var prim_wrapper = new PrimFunctionWrapper(prim_func_1, 1);

        var input = new Var("input", new TensorType(DataTypes.Float32, new[] { 1, 2, 3, 4 }));
        var main_func = new Function("main", (new Call(prim_wrapper, ImmutableArray.Create<Expr>(input))) + loop_i, ImmutableArray.Create<Var>(input));

        Assert.True(CompilerServices.InferenceType(main_func));

        Dictionary<Expr, Expr> vmap = new() { { loop_i, 1 } };
        var substitutor = Transform.Mutator.Substitute(e => vmap.TryGetValue(e, out var res) ? res : null)();

        CompilerServices.DumpIR(main_func, "pre", passOptions.DumpDir);
        var main_func_2 = substitutor.Visit(main_func);
        Assert.True(CompilerServices.InferenceType(main_func_2));

        CompilerServices.DumpIR(main_func_2, "post", passOptions.DumpDir);

        Assert.False(object.ReferenceEquals(main_func, main_func_2));

        Assert.True(main_func_2 is Function { Body: Call { Target: IR.Math.Binary, Parameters: IRArray<Expr> binary_param } } &&
                  binary_param[0] is Call { Target: PrimFunctionWrapper wrapper } &&
                  object.Equals(prim_wrapper, wrapper) &&
                   binary_param[1] is TensorConst);
    }

    /// <summary>
    /// try substitute the same function twice.
    /// </summary>
    [Fact]
    public void TestSubstitutorTrue3()
    {
        var passOptions = GetPassOptions();

        var input = new Var("input", new TensorType(DataTypes.Float32, new[] { 1, 2, 3, 4 }));
        var loop_i = new Var("loop_i", TensorType.Scalar(DataTypes.Int32));
        var main_func = new Function("main", 3 + loop_i, ImmutableArray.Create<Var>(input));

        Assert.True(CompilerServices.InferenceType(main_func));

        Dictionary<Expr, Expr> vmap = new() { { loop_i, 1 } };
        var substitutor = Transform.Mutator.Substitute(e => vmap.TryGetValue(e, out var res) ? res : null)();

        CompilerServices.DumpIR(main_func, "pre", passOptions.DumpDir);
        var main_func_2 = substitutor.Visit(main_func);
        Assert.True(CompilerServices.InferenceType(main_func_2));

        CompilerServices.DumpIR(main_func_2, "post", passOptions.DumpDir);

        Assert.False(object.ReferenceEquals(main_func, main_func_2));

        Assert.True(main_func_2 is Function { Body: Call { Target: IR.Math.Binary, Parameters: IRArray<Expr> binary_param } } &&
                   binary_param[1] is TensorConst tensor && tensor.Value.ToScalar<int>() == 1);
    }
}