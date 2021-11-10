// Copyright (c) Canaan Inc. All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nncase.IR.Tensors
{
    /// <summary>
    /// OneHot expression.
    /// </summary>
    public sealed record OneHot(OneHotMode OneHotMode) : Op
    {
        /// <summary>
        /// Gets input.
        /// </summary>
        public static readonly ParameterInfo Input = new(typeof(OneHot), 0, "input");

        /// <summary>
        /// Gets depth.
        /// </summary>
        public static readonly ParameterInfo Depth = new(typeof(OneHot), 1, "depth");

        /// <summary>
        /// Gets on_value.
        /// </summary>
        public static readonly ParameterInfo OnValue = new(typeof(OneHot), 2, "on_value");
        
        /// <summary>
        /// Gets off_value.
        /// </summary>
        public static readonly ParameterInfo OffValue = new(typeof(OneHot), 3, "off_value");

        /// <summary>
        /// Gets axis.
        /// </summary>
        public static readonly ParameterInfo Axis = new(typeof(OneHot), 4, "axis");
        
        /// <inheritdoc/>
        public override IRType InferInvokeResultType(ITypeInferenceContext context)
        {
            throw new NotImplementedException();
        }
    }
}