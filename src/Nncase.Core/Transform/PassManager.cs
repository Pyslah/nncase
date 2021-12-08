﻿// Copyright (c) Canaan Inc. All rights reserved.
// Licensed under the Apache license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nncase.IR;

namespace Nncase.Transform
{
    /// <summary>
    /// Pass manager.
    /// </summary>
    public class PassManager
    {
        private readonly Module _module;
        private readonly RunPassOptions _options;
        private readonly List<FunctionPass> _passes = new List<FunctionPass>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PassManager"/> class.
        /// </summary>
        /// <param name="module">Module.</param>
        /// <param name="options">Options.</param>
        public PassManager(Module module, RunPassOptions options)
        {
            _module = module;
            _options = options;
        }

        /// <summary>
        /// Add function pass.
        /// </summary>
        /// <param name="pass">Pass.</param>
        public void Add(FunctionPass pass)
        {
            _passes.Add(pass);
        }

        /// <summary>
        /// Run passes.
        /// </summary>
        public void Run()
        {
            foreach (var pass in _passes)
            {
                _module.Update(pass.Run(_module.Entry!, _options));
            }
        }
    }
}