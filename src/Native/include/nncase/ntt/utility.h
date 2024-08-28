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
#include "shape.h"
#include <cstddef>
#include <cstring>
#include <span>
#include <utility>

namespace nncase::ntt {
namespace utility_detail {
template <size_t OutRank, size_t InRank, size_t... Ints>
inline constexpr ranked_shape<OutRank>
slice_index(const ranked_shape<InRank> &index, const size_t offset,
            std::index_sequence<Ints...>) noexcept {
    return ranked_shape<OutRank>{index[offset + Ints]...};
}

template <size_t OutRank, size_t OffSet = 0, template <size_t...> class A,
          size_t... Dims, size_t... Ints>
inline constexpr auto slice(A<Dims...>, std::index_sequence<Ints...>) noexcept {
    return A<A<Dims...>::at(Ints + OffSet)...>{};
}

template <template <size_t...> class T, size_t... ADims, size_t... BDims,
          size_t... I>
inline constexpr bool is_same_seq(const T<ADims...> &a, const T<BDims...> &b,
                                  std::index_sequence<I...>) {
    return ((a[I] == b[I]) && ...);
}

template <class TTensor, class TOutShape>
static constexpr size_t get_safe_stride(const TTensor &tensor, size_t axis,
                                        const TOutShape &out_shape) noexcept {
    auto dim_ext = out_shape.rank() - tensor.rank();
    if (axis < dim_ext) {
        return 0;
    }

    auto actual_axis = axis - dim_ext;
    return tensor.shape()[actual_axis] == 1 ? 0 // broadcast
                                            : tensor.strides()[actual_axis];
}

template <template <size_t...> class A, size_t... Ints>
inline constexpr auto
make_index_sequence(std::index_sequence<Ints...>) noexcept {
    return A<Ints...>{};
}

template <int32_t Offset, template <size_t...> class A, size_t... Dims>
inline constexpr auto shift_fixed_dims(A<Dims...>) {
    return A<(Dims - Offset)...>{};
}
} // namespace utility_detail

template <class U, class T, size_t Extent>
auto span_cast(std::span<T, Extent> span) noexcept {
    using return_type =
        std::conditional_t<Extent == std::dynamic_extent, std::span<U>,
                           std::span<U, Extent * sizeof(T) / sizeof(U)>>;
    return return_type(reinterpret_cast<U *>(span.data()),
                       span.size_bytes() / sizeof(U));
}

template <size_t OutRank, size_t InRank>
inline constexpr ranked_shape<OutRank>
slice_index(const ranked_shape<InRank> &index,
            const size_t offset = 0) noexcept {
    static_assert(OutRank <= InRank, "the out rank must less then inRank");
    return utility_detail::slice_index<OutRank>(
        index, offset, std::make_index_sequence<OutRank>{});
}

template <template <size_t...> class T, size_t... PreDims, size_t... PostDims>
inline constexpr auto concat_fixed_dims(T<PreDims...>,
                                        T<PostDims...>) noexcept {
    return T<PreDims..., PostDims...>{};
}

template <size_t OutRank, size_t OffSet = 0, template <size_t...> class A,
          size_t... Dims>
inline constexpr auto slice_fixed_dims(A<Dims...> a) noexcept {
    return utility_detail::slice<OutRank, OffSet>(
        a, std::make_index_sequence<OutRank>{});
}

template <template <size_t...> class T, size_t... ADims, size_t... BDims>
inline constexpr bool is_same_seq(const T<ADims...> &a, const T<BDims...> &b) {
    return sizeof...(ADims) == sizeof...(BDims) &&
           utility_detail::is_same_seq(
               a, b, std::make_index_sequence<sizeof...(ADims)>{});
}

template <template <size_t...> class A, size_t... Dims>
inline constexpr auto make_index_sequence(A<Dims...>) {
    return utility_detail::make_index_sequence<A>(
        std::make_index_sequence<sizeof...(Dims)>{});
}

template <int32_t Offset, template <size_t...> class A, size_t... Dims>
inline constexpr auto shift_fixed_dims(A<Dims...> a) {
    return utility_detail::shift_fixed_dims<Offset>(a);
}
} // namespace nncase::ntt