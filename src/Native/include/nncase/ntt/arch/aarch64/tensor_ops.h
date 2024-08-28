/* Copyright 2019-2021 Canaan Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#pragma once
#include "../../tensor_ops.h"
#include "arch_types.h"
#include "arm_math.h"

namespace nncase::ntt::tensor_ops {
template <> struct tload_scalar<ntt::vector<float, 4>> {
    ntt::vector<float, 4> operator()(const float &v) const noexcept {
        return vdupq_n_f32(v);
    }
};

template <> struct tload_scalar<ntt::vector<float, 8>> {
    ntt::vector<float, 8> operator()(const float &v) const noexcept {
        return float32x4x2_t{vdupq_n_f32(v), vdupq_n_f32(v)};
    }
};
} // namespace nncase::ntt::tensor_ops