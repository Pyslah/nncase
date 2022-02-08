using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
using Nncase.IR;
using Nncase.Transform;
using Nncase.Pattern;
using static Nncase.IR.F.Math;
using static Nncase.IR.F.Tensors;
using static Nncase.IR.F.NN;
using Rule = Nncase.Transform.Rule;
using static Nncase.Pattern.F.Math;
using static Nncase.Pattern.F.NN;
using static Nncase.Pattern.F.Tensors;
using static Nncase.Pattern.Utility;
using System.IO;
using Nncase.Evaluator;


namespace Nncase.Tests.ReWriteTest
{
    public class EGraphRewriteTestFactory : RewriteTest
    {
        public EGraphRewriteTestFactory() : base()
        {
            passOptions.SetDir(Path.Combine(passOptions.FullDumpDir, "EGraphRewriteTestFactory"));
        }

        private static IEnumerable<object[]> Data =>
          new List<object[]>
          {
             new object[] { new FoldNopClampCase() },
             new object[] { new FoldNopReshapeCase() },
             new object[] { new FoldReshapeCase() },
             new object[] { new TransposeDemoCase() },
             new object[] { new ClassicDemo() },
             new object[] { new FoldNopTransposeCase3() },
             new object[] { new FoldNopTransposeCase2() },
             new object[] { new FoldNopTransposeCase1() },
             new object[] { new FoldTransposeCase() },
             new object[] { new TransposeConstBinaryCase() },
          };

        [Theory]
        [MemberData(nameof(DataOne))]
        public void RunOne(IRewriteCase Case) => RunCore(Case);

        protected void RunCore(IRewriteCase Case)
        {
            passOptions.SetName($"{Case.Name}");
            Expr pre = Case.PreExpr;
            var infered = pre.InferenceType();
            pre.DumpExprAsIL("pre", passOptions.FullDumpDir);
            Assert.True(infered);
            var eGraph = new EGraph();
            eGraph.Add(pre, out var root);
            EGraphPrinter.DumpEgraphAsDot(eGraph, Path.Combine(passOptions.FullDumpDir, $"pre"));

            EGraphReWriter.ReWrite(eGraph, Case.Rules, passOptions);
            var post = eGraph.Extract(root, passOptions);
            Assert.True(post.InferenceType());
            post.DumpExprAsIL("post", passOptions.FullDumpDir);
            Assert.Equal((pre.Eval()), (post.Eval()));
        }

        [Theory]
        [MemberData(nameof(DataAll))]
        public void RunAll(IRewriteCase Case) => RunCore(Case);


        public static IEnumerable<object[]> DataOne => Data.Take(1);
        public static IEnumerable<object[]> DataAll => Data.Skip(1);
    }

    public class EGraphRewriteTest : RewriteTest
    {

        public EGraphRewriteTest() : base()
        {
            passOptions.SetDir(Path.Combine(passOptions.FullDumpDir, "EGraphRewriteTest"));
        }

        [Fact]
        public void RewriteNoSenceAdd()
        {
            var Name = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Var x = "a";
            var lhs = (x + (100 / 120.0f) - 100);
            var y = lhs + 0;
            var egraph = new EGraph(y);
            EGraphPrinter.DumpEgraphAsDot(egraph, $"{Name}_ADD");

            WildCardPattern wcx = "a";
            var pattern = wcx + IsConst(0);

            // rule  (? + 0) => (?)
            Func<Expr, Expr> nawPass = x => x;

            var EResults = EGraphMatcher.Match(egraph, pattern);
            EGraphPrinter.DumpEgraphAsDot(egraph, EResults, $"{Name}_Ematch");
            Assert.Single(EResults);
            var wcxv = EResults[0][wcx];
            Assert.Equal(wcxv, lhs);
            egraph.Add(nawPass(wcxv), out var to_eid);

            egraph.Merge(to_eid, egraph.HashCons[((EMatchResult)EResults[0]).Root]);
            EGraphPrinter.DumpEgraphAsDot(egraph, $"{Name}_Merge");
            egraph.ReBuild();
            EGraphPrinter.DumpEgraphAsDot(egraph, $"{Name}_ReBuild");
        }

        [Fact]
        public void TestReassociate()
        {
            Expr pre = ((Const)10 * 11) * 12;
            var eGraph = new EGraph(pre);
            var rule = new Rule.ReassociateMul();
            EGraphReWriter.ReWrite(eGraph, rule, passOptions.SetName("Reassociate"));
            // Assert.Equal(newExpr, 10 * ((Const)11 * 12));
        }


        [Fact]
        public void TestClassicDemo()
        {
            passOptions.SetName("EGraphTest/TestClassicDemo");
            var g = new EGraph();
            Var x = "x";
            g.Add(x * 2, out var e1);
            g.Add((x * 2) / 2, out var root);
            EGraphPrinter.DumpEgraphAsDot(g, Path.Combine(passOptions.FullDumpDir, "befroe"));
            g.Add(x << 1, out var e2);
            EGraphPrinter.DumpEgraphAsDot(g, Path.Combine(passOptions.FullDumpDir, "added"));
            g.Merge(e2, e1);
            EGraphPrinter.DumpEgraphAsDot(g, Path.Combine(passOptions.FullDumpDir, "merge"));
            g.ReBuild();
            EGraphPrinter.DumpEgraphAsDot(g, Path.Combine(passOptions.FullDumpDir, "rebuild"));
        }


        [Fact]
        public void TestTransposeBinaryMotion()
        {
            passOptions.SetName("TransposeBinaryMotion");
            Call c0 = (Call)NHWCToNCHW(Const.FromShape<int>(new[] { 2, 2, 3, 4 }, 1));
            Call c1 = (Call)NHWCToNCHW(Const.FromShape<int>(new[] { 2, 2, 1, 1 }, 1));
            Assert.Equal(c0.Parameters[1].GetHashCode(), c1.Parameters[1].GetHashCode());

            Expr pre = c0 + c1;

            Assert.True(pre.InferenceType());
            var eGraph = new EGraph();
            eGraph.Add(pre, out var root);
            pre.DumpExprAsIL("pre", passOptions.FullDumpDir);

            EGraphReWriter.ReWrite(eGraph, new Rule.TransposeBinaryMotion(), passOptions);

            var post = eGraph.Extract(root, passOptions);
            Assert.True(post.InferenceType());
            Assert.Equal((pre.Eval()), (post.Eval()));
            post.DumpExprAsIL("post", passOptions.FullDumpDir);
        }
    }

}
