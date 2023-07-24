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
#include "compiler_defs.h"
#include <nncase/runtime/runtime_module.h>

BEGIN_NS_NNCASE_RT_MODULE(cpu)

NNCASE_INLINE_VAR constexpr char FUNCTION_NAME_SECTION_IDENTIFIER[] = ".name";

NNCASE_INLINE_VAR constexpr module_kind_t cpu_module_type =
    to_module_kind("cpu");
NNCASE_INLINE_VAR constexpr uint32_t cpu_module_version = 0;

NNCASE_MODULES_CPU_API result<std::unique_ptr<runtime_module>>
create_cpu_runtime_module();

END_NS_NNCASE_RT_MODULE
