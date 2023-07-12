/* This file is generated by tools/stackvm_gen/IsaGen at 2023/7/12 15:04:16 +08:00.
 *
 * Copyright 2019-2021 Canaan Inc.
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
#include <nncase/runtime/stackvm/op_reader.h>

using namespace nncase;
using namespace nncase::runtime;
using namespace nncase::runtime::stackvm;

result<void> tensor_op_visitor::visit(tensor_function_t tensor_funct, span_reader &reader) noexcept
{
     switch (tensor_funct)
     {
    case tensor_function_t::batch_normalization:
        return visit(tensor_op_reader<tensor_function_t::batch_normalization>()(reader));
    case tensor_function_t::batch_to_space:
        return visit(tensor_op_reader<tensor_function_t::batch_to_space>()(reader));
    case tensor_function_t::binary:
        return visit(tensor_op_reader<tensor_function_t::binary>()(reader));
    case tensor_function_t::bitcast:
        return visit(tensor_op_reader<tensor_function_t::bitcast>()(reader));
    case tensor_function_t::broadcast:
        return visit(tensor_op_reader<tensor_function_t::broadcast>()(reader));
    case tensor_function_t::bucket_pad:
        return visit(tensor_op_reader<tensor_function_t::bucket_pad>()(reader));
    case tensor_function_t::cast:
        return visit(tensor_op_reader<tensor_function_t::cast>()(reader));
    case tensor_function_t::celu:
        return visit(tensor_op_reader<tensor_function_t::celu>()(reader));
    case tensor_function_t::clamp:
        return visit(tensor_op_reader<tensor_function_t::clamp>()(reader));
    case tensor_function_t::compare:
        return visit(tensor_op_reader<tensor_function_t::compare>()(reader));
    case tensor_function_t::concat:
        return visit(tensor_op_reader<tensor_function_t::concat>()(reader));
    case tensor_function_t::condition:
        return visit(tensor_op_reader<tensor_function_t::condition>()(reader));
    case tensor_function_t::constant_of_shape:
        return visit(tensor_op_reader<tensor_function_t::constant_of_shape>()(reader));
    case tensor_function_t::conv2d:
        return visit(tensor_op_reader<tensor_function_t::conv2d>()(reader));
    case tensor_function_t::conv2d_transpose:
        return visit(tensor_op_reader<tensor_function_t::conv2d_transpose>()(reader));
    case tensor_function_t::cum_sum:
        return visit(tensor_op_reader<tensor_function_t::cum_sum>()(reader));
    case tensor_function_t::dequantize:
        return visit(tensor_op_reader<tensor_function_t::dequantize>()(reader));
    case tensor_function_t::elu:
        return visit(tensor_op_reader<tensor_function_t::elu>()(reader));
    case tensor_function_t::erf:
        return visit(tensor_op_reader<tensor_function_t::erf>()(reader));
    case tensor_function_t::expand:
        return visit(tensor_op_reader<tensor_function_t::expand>()(reader));
    case tensor_function_t::fake_dequantize:
        return visit(tensor_op_reader<tensor_function_t::fake_dequantize>()(reader));
    case tensor_function_t::fake_quantize:
        return visit(tensor_op_reader<tensor_function_t::fake_quantize>()(reader));
    case tensor_function_t::fix_shape:
        return visit(tensor_op_reader<tensor_function_t::fix_shape>()(reader));
    case tensor_function_t::flatten:
        return visit(tensor_op_reader<tensor_function_t::flatten>()(reader));
    case tensor_function_t::gather:
        return visit(tensor_op_reader<tensor_function_t::gather>()(reader));
    case tensor_function_t::gather_elements:
        return visit(tensor_op_reader<tensor_function_t::gather_elements>()(reader));
    case tensor_function_t::gather_nd:
        return visit(tensor_op_reader<tensor_function_t::gather_nd>()(reader));
    case tensor_function_t::gelu:
        return visit(tensor_op_reader<tensor_function_t::gelu>()(reader));
    case tensor_function_t::get_item:
        return visit(tensor_op_reader<tensor_function_t::get_item>()(reader));
    case tensor_function_t::hard_sigmoid:
        return visit(tensor_op_reader<tensor_function_t::hard_sigmoid>()(reader));
    case tensor_function_t::hard_swish:
        return visit(tensor_op_reader<tensor_function_t::hard_swish>()(reader));
    case tensor_function_t::hardmax:
        return visit(tensor_op_reader<tensor_function_t::hardmax>()(reader));
    case tensor_function_t::index_of:
        return visit(tensor_op_reader<tensor_function_t::index_of>()(reader));
    case tensor_function_t::instance_normalization:
        return visit(tensor_op_reader<tensor_function_t::instance_normalization>()(reader));
    case tensor_function_t::l2_normalization:
        return visit(tensor_op_reader<tensor_function_t::l2_normalization>()(reader));
    case tensor_function_t::layer_norm:
        return visit(tensor_op_reader<tensor_function_t::layer_norm>()(reader));
    case tensor_function_t::leaky_relu:
        return visit(tensor_op_reader<tensor_function_t::leaky_relu>()(reader));
    case tensor_function_t::log_softmax:
        return visit(tensor_op_reader<tensor_function_t::log_softmax>()(reader));
    case tensor_function_t::lp_normalization:
        return visit(tensor_op_reader<tensor_function_t::lp_normalization>()(reader));
    case tensor_function_t::lrn:
        return visit(tensor_op_reader<tensor_function_t::lrn>()(reader));
    case tensor_function_t::lstm:
        return visit(tensor_op_reader<tensor_function_t::lstm>()(reader));
    case tensor_function_t::mat_mul:
        return visit(tensor_op_reader<tensor_function_t::mat_mul>()(reader));
    case tensor_function_t::normal:
        return visit(tensor_op_reader<tensor_function_t::normal>()(reader));
    case tensor_function_t::normal_like:
        return visit(tensor_op_reader<tensor_function_t::normal_like>()(reader));
    case tensor_function_t::one_hot:
        return visit(tensor_op_reader<tensor_function_t::one_hot>()(reader));
    case tensor_function_t::pad:
        return visit(tensor_op_reader<tensor_function_t::pad>()(reader));
    case tensor_function_t::prelu:
        return visit(tensor_op_reader<tensor_function_t::prelu>()(reader));
    case tensor_function_t::prod:
        return visit(tensor_op_reader<tensor_function_t::prod>()(reader));
    case tensor_function_t::quant_param_of:
        return visit(tensor_op_reader<tensor_function_t::quant_param_of>()(reader));
    case tensor_function_t::quantize:
        return visit(tensor_op_reader<tensor_function_t::quantize>()(reader));
    case tensor_function_t::range:
        return visit(tensor_op_reader<tensor_function_t::range>()(reader));
    case tensor_function_t::range_of:
        return visit(tensor_op_reader<tensor_function_t::range_of>()(reader));
    case tensor_function_t::rank:
        return visit(tensor_op_reader<tensor_function_t::rank>()(reader));
    case tensor_function_t::reduce:
        return visit(tensor_op_reader<tensor_function_t::reduce>()(reader));
    case tensor_function_t::reduce_arg:
        return visit(tensor_op_reader<tensor_function_t::reduce_arg>()(reader));
    case tensor_function_t::reduce_window2d:
        return visit(tensor_op_reader<tensor_function_t::reduce_window2d>()(reader));
    case tensor_function_t::relu:
        return visit(tensor_op_reader<tensor_function_t::relu>()(reader));
    case tensor_function_t::relu6:
        return visit(tensor_op_reader<tensor_function_t::relu6>()(reader));
    case tensor_function_t::require:
        return visit(tensor_op_reader<tensor_function_t::require>()(reader));
    case tensor_function_t::reshape:
        return visit(tensor_op_reader<tensor_function_t::reshape>()(reader));
    case tensor_function_t::resize_image:
        return visit(tensor_op_reader<tensor_function_t::resize_image>()(reader));
    case tensor_function_t::reverse_sequence:
        return visit(tensor_op_reader<tensor_function_t::reverse_sequence>()(reader));
    case tensor_function_t::scatter_nd:
        return visit(tensor_op_reader<tensor_function_t::scatter_nd>()(reader));
    case tensor_function_t::select:
        return visit(tensor_op_reader<tensor_function_t::select>()(reader));
    case tensor_function_t::selu:
        return visit(tensor_op_reader<tensor_function_t::selu>()(reader));
    case tensor_function_t::shape_of:
        return visit(tensor_op_reader<tensor_function_t::shape_of>()(reader));
    case tensor_function_t::sigmoid:
        return visit(tensor_op_reader<tensor_function_t::sigmoid>()(reader));
    case tensor_function_t::size_of:
        return visit(tensor_op_reader<tensor_function_t::size_of>()(reader));
    case tensor_function_t::slice:
        return visit(tensor_op_reader<tensor_function_t::slice>()(reader));
    case tensor_function_t::softmax:
        return visit(tensor_op_reader<tensor_function_t::softmax>()(reader));
    case tensor_function_t::softplus:
        return visit(tensor_op_reader<tensor_function_t::softplus>()(reader));
    case tensor_function_t::softsign:
        return visit(tensor_op_reader<tensor_function_t::softsign>()(reader));
    case tensor_function_t::space_to_batch:
        return visit(tensor_op_reader<tensor_function_t::space_to_batch>()(reader));
    case tensor_function_t::split:
        return visit(tensor_op_reader<tensor_function_t::split>()(reader));
    case tensor_function_t::squeeze:
        return visit(tensor_op_reader<tensor_function_t::squeeze>()(reader));
    case tensor_function_t::stack:
        return visit(tensor_op_reader<tensor_function_t::stack>()(reader));
    case tensor_function_t::swish:
        return visit(tensor_op_reader<tensor_function_t::swish>()(reader));
    case tensor_function_t::tile:
        return visit(tensor_op_reader<tensor_function_t::tile>()(reader));
    case tensor_function_t::top_k:
        return visit(tensor_op_reader<tensor_function_t::top_k>()(reader));
    case tensor_function_t::transpose:
        return visit(tensor_op_reader<tensor_function_t::transpose>()(reader));
    case tensor_function_t::trilu:
        return visit(tensor_op_reader<tensor_function_t::trilu>()(reader));
    case tensor_function_t::unary:
        return visit(tensor_op_reader<tensor_function_t::unary>()(reader));
    case tensor_function_t::uniform:
        return visit(tensor_op_reader<tensor_function_t::uniform>()(reader));
    case tensor_function_t::uniform_like:
        return visit(tensor_op_reader<tensor_function_t::uniform_like>()(reader));
    case tensor_function_t::unsqueeze:
        return visit(tensor_op_reader<tensor_function_t::unsqueeze>()(reader));
    case tensor_function_t::where:
        return visit(tensor_op_reader<tensor_function_t::where>()(reader));
    default:
        break;
    }

    return err(nncase_errc::stackvm_illegal_instruction);
}
