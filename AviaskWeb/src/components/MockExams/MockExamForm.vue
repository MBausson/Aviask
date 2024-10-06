<script lang="ts" setup>
import { SelectComponent, ButtonComponent, HeroIcon } from "@/components";
import { ListboxOption } from "@headlessui/vue";
import { type CreateMockExamModel } from "@/models/DTO/requestModels";
import { responseModels } from "@/models";
import { ref } from "vue";
import { AtplCategory } from "@/models/DTO/responseModels";
import { z } from "zod";
import useEventBus from "@/eventBus";
import moment from "moment";

const emit = defineEmits(["formSent"]);
const { emitBus } = useEventBus();

const modelSchema = z.object({
    maxQuestions: z.coerce.number().min(20).max(100).catch(20),
    category: z.coerce.number().positive().max(100),
    timeLimit: z.coerce.string(),
});

const model = ref<CreateMockExamModel>({} as CreateMockExamModel);

const timeLimitOptions = [
    moment.duration("00:30:00"),
    moment.duration("01:00:00"),
    moment.duration("01:30:00"),
    moment.duration("02:00:00"),
];
const questionsNumberOptions = [20, 50, 75, 100];

async function onFormSend() {
    const res = await modelSchema.safeParseAsync(model.value);

    if (!res.success) {
        emitBus("flashOpen", "Invalid practice exam parameters", "danger");
        return;
    }

    emit("formSent", model.value);
}
</script>
<template>
    <div class="flex flex-col gap-10 bg-neutral-100 dark:bg-neutral-800 p-5 md:p-8 rounded-xl">
        <div class="flex items-center gap-4 justify-between">
            <h3 class="flex items-center gap-2 dark:text-white">
                <HeroIcon icon="Cog8ToothIcon" size="lg" class="hidden sm:inline" />
                <span class="text-2xl font-semibold text-pretty">Start a new practice exam</span>
            </h3>

            <ButtonComponent state="cta" class="flex items-center gap-2" @click="onFormSend">
                <span>Start</span>
                <HeroIcon icon="ChevronRightIcon" />
            </ButtonComponent>
        </div>

        <div class="flex gap-1 flex-col">
            <label class="dark:text-white font-medium text-lg">Target category</label>

            <SelectComponent
                v-model="model.category"
                :values="responseModels.AtplCategory.categories"
                placeholder="Select an ATPL category"
                required>
                <template #selectTrigger>
                    {{ AtplCategory.getCategory(model.category)?.name ?? "Select an ATPL category" }}
                </template>
                <template #options>
                    <ListboxOption
                        v-for="category in AtplCategory.categories"
                        v-slot="{ selected }"
                        :key="category.code"
                        as="template"
                        :value="category.code">
                        <li
                            :class="[
                                selected ? 'text-neutral-700 bg-muted dark:bg-muted-dark' : 'text-gray-900',
                                'relative cursor-default select-none py-2 pl-10 pr-4',
                            ]">
                            <span :class="[selected ? 'font-medium' : 'font-normal', 'block truncate']">
                                {{ category.name }}
                            </span>

                            <span v-if="selected" class="absolute inset-y-0 left-0 flex items-center pl-3">
                                <HeroIcon icon="CheckIcon" />
                            </span>
                        </li>
                    </ListboxOption>
                </template>
            </SelectComponent>
        </div>

        <div class="grid grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
                <label class="dark:text-white font-medium text-lg flex justify-between gap-4">
                    Number of questions
                </label>

                <SelectComponent v-model="model.maxQuestions">
                    <template #selectTrigger>
                        {{ model.maxQuestions ? `${model.maxQuestions} questions` : "Select a number of questions" }}
                    </template>

                    <template #options>
                        <ListboxOption
                            v-for="amount in questionsNumberOptions"
                            v-slot="{ selected }"
                            :key="amount"
                            as="template"
                            :value="amount">
                            <li
                                :class="[
                                    selected ? 'text-neutral-700 bg-muted dark:bg-muted-dark' : 'text-gray-900',
                                    'relative cursor-default select-none py-2 pl-10 pr-4',
                                ]">
                                <span :class="[selected ? 'font-medium' : 'font-normal', 'block truncate']">
                                    {{ `${amount} questions` }}
                                </span>

                                <span v-if="selected" class="absolute inset-y-0 left-0 flex items-center pl-3">
                                    <HeroIcon icon="CheckIcon" />
                                </span>
                            </li>
                        </ListboxOption>
                    </template>
                </SelectComponent>
            </div>

            <div class="flex flex-col gap-2">
                <label class="dark:text-white font-medium text-lg flex justify-between gap-4">Time limit</label>

                <SelectComponent v-model="model.timeLimit">
                    <template #selectTrigger>
                        {{
                            model.timeLimit ?
                                `${moment.duration(model.timeLimit).as("minutes")} minutes`
                            :   "Practice exam time limit"
                        }}
                    </template>

                    <template #options>
                        <ListboxOption
                            v-for="duration in timeLimitOptions"
                            v-slot="{ selected }"
                            :key="duration.milliseconds"
                            as="template"
                            :value="moment.utc(duration.asMilliseconds()).format('HH:mm:ss')">
                            <li
                                :class="[
                                    selected ? 'text-neutral-700 bg-muted dark:bg-muted-dark' : 'text-gray-900',
                                    'relative cursor-default select-none py-2 pl-10 pr-4',
                                ]">
                                <span :class="[selected ? 'font-medium' : 'font-normal', 'block truncate']">
                                    {{ duration.as("minutes") }} minutes
                                </span>

                                <span v-if="selected" class="absolute inset-y-0 left-0 flex items-center pl-3">
                                    <HeroIcon icon="CheckIcon" />
                                </span>
                            </li>
                        </ListboxOption>
                    </template>
                </SelectComponent>
            </div>
        </div>
    </div>
</template>
