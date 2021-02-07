/* Copyright 2019-2020 Canaan Inc.
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
#include <nncase/ir/ops/k210/fake_kpu_conv2d.h>
#include <nncase/runtime/k210/runtime_op_utility.h>

using namespace nncase;
using namespace nncase::ir;
using namespace nncase::ir::k210;
using namespace nncase::runtime::k210;

fake_kpu_conv2d::fake_kpu_conv2d(shape_t input_shape, bool is_depthwise, runtime::k210::kpu_filter_type_t filter_type, runtime::k210::kpu_pool_type_t pool_type, xt::xtensor<float, 4> weights,
    xt::xtensor<float, 1> bias, value_range<float> fused_activation)
    : weights_(std::move(weights)), bias_(std::move(bias)), is_depthwise_(is_depthwise), filter_type_(filter_type), pool_type_(pool_type), fused_activation_(fused_activation)
{
    add_input("input", dt_float32, input_shape);
    add_output("output", dt_float32,
        shape_t {
            input_shape[0],
            (size_t)output_channels(),
            (size_t)get_kpu_pool_output_size((int32_t)input_shape[2], pool_type_),
            (size_t)get_kpu_pool_output_size((int32_t)input_shape[3], pool_type_) });
}

bool fake_kpu_conv2d::properties_equal(node &other) const
{
    auto &r = static_cast<fake_kpu_conv2d &>(other);
    return is_depthwise() == r.is_depthwise() && filter_type() == r.filter_type() && pool_type() == r.pool_type()
        && weights() == r.weights() && bias() == r.bias() && fused_activation() == r.fused_activation();
}
