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
#include <type_traits>

#if defined(_MSC_VER)
#ifdef NNCASE_DLL
#define NNCASE_API __declspec(dllexport)
#elif defined(NNCASE_SHARED_LIBS)
#define NNCASE_API __declspec(dllimport)
#else
#define NNCASE_API
#endif
#else
#define NNCASE_API __attribute__((visibility("default")))
#endif

#if defined(_MSC_VER)
#define NNCASE_UNREACHABLE() __assume(0)
#else
#define NNCASE_UNREACHABLE() __builtin_unreachable()
#endif

#define NNCASE_INLINE_VAR inline
#define NNCASE_UNUSED [[maybe_unused]]
namespace nncase {
template <class Callable, class... Args>
using invoke_result_t = std::invoke_result_t<Callable, Args...>;
}

#define NNCASE_LITTLE_ENDIAN 1

#define NNCASE_NODISCARD [[nodiscard]]
#define NNCASE_NORETURN [[noreturn]]

#define BEGIN_NS_NNCASE_RUNTIME                                                \
    namespace nncase {                                                         \
    namespace runtime {
#define END_NS_NNCASE_RUNTIME                                                  \
    }                                                                          \
    }

#define BEGIN_NS_NNCASE_RT_MODULE(MODULE)                                      \
    namespace nncase {                                                         \
    namespace runtime {                                                        \
    namespace MODULE {

#define END_NS_NNCASE_RT_MODULE                                                \
    }                                                                          \
    }                                                                          \
    }

#define BEGIN_NS_NNCASE_KERNELS                                                \
    namespace nncase {                                                         \
    namespace kernels {

#define END_NS_NNCASE_KERNELS                                                  \
    }                                                                          \
    }

#define BEGIN_NS_NNCASE_KERNELS_MODULE(MODULE)                                 \
    namespace nncase {                                                         \
    namespace kernels {                                                        \
    namespace MODULE {

#define END_NS_NNCASE_KERNELS_MODULE                                           \
    }                                                                          \
    }                                                                          \
    }

#ifndef DEFINE_ENUM_BITMASK_OPERATORS
#define DEFINE_ENUM_BITMASK_OPERATORS(ENUM)                                    \
    [[nodiscard]] inline constexpr ENUM operator~(ENUM val) noexcept {         \
        typedef typename std::underlying_type<ENUM>::type U;                   \
        return ENUM(~U(val));                                                  \
    }                                                                          \
    [[nodiscard]] inline constexpr ENUM operator|(ENUM lhs,                    \
                                                  ENUM rhs) noexcept {         \
        typedef typename std::underlying_type<ENUM>::type U;                   \
        return ENUM(U(lhs) | U(rhs));                                          \
    }                                                                          \
    [[nodiscard]] inline constexpr ENUM operator&(ENUM lhs,                    \
                                                  ENUM rhs) noexcept {         \
        typedef typename std::underlying_type<ENUM>::type U;                   \
        return ENUM(U(lhs) & U(rhs));                                          \
    }                                                                          \
    [[nodiscard]] inline constexpr ENUM operator^(ENUM lhs,                    \
                                                  ENUM rhs) noexcept {         \
        typedef typename std::underlying_type<ENUM>::type U;                   \
        return ENUM(U(lhs) ^ U(rhs));                                          \
    }                                                                          \
    inline constexpr ENUM &operator|=(ENUM &lhs, ENUM rhs) noexcept {          \
        return lhs = lhs | rhs;                                                \
    }                                                                          \
    inline constexpr ENUM &operator&=(ENUM &lhs, ENUM rhs) noexcept {          \
        return lhs = lhs & rhs;                                                \
    }                                                                          \
    inline constexpr ENUM &operator^=(ENUM &lhs, ENUM rhs) noexcept {          \
        return lhs = lhs ^ rhs;                                                \
    }
#endif

namespace nncase {
struct default_init_t {};

NNCASE_INLINE_VAR constexpr default_init_t default_init{};
} // namespace nncase
