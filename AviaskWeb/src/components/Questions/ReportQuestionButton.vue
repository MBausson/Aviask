<script lang="ts" setup>
import { ButtonComponent, ModalComponent, SelectComponent, TextInput, HeroIcon } from "@/components";
import { ref, type PropType } from "vue";
import { QuestionReportCategory } from "@/models/aviaskQuestionReport";
import useEventBus from "@/eventBus";
import { requestModels, AviaskQuestionDetails } from "@/models";
import { ListboxOption } from "@headlessui/vue";
import useQuestionReportsRepository from "@/repositories/questionReportsRepository";

const questionReportsRepository = useQuestionReportsRepository();
const { emitBus } = useEventBus();
const { question } = defineProps({
    question: {
        type: Object as PropType<AviaskQuestionDetails>,
        required: true,
    },
});

const showModal = ref<boolean>(false);
const questionReport = ref<requestModels.CreateQuestionReportModel>({
    category: 0,
    message: "",
    questionId: question.id,
});

async function onSubmit() {
    if (!questionReport.value.message?.length) return;

    const res = await questionReportsRepository.create(questionReport.value);

    if (!res.success) return emitBus("flashOpen", res.error.message, "danger");

    emitBus("flashOpen", `Successfully sent a report for this question.`, "success");
    showModal.value = false;
}
</script>

<template>
    <ModalComponent v-model:show="showModal">
        <template #title>
            <h3 class="text-2xl font-semibold leading-6">Report a question</h3>
        </template>

        <template #description>
            <section class="flex flex-col gap-4">
                <div class="break-all text-lg">
                    <b>[{{ question.category.code }}]</b>
                    - {{ question.title }}
                </div>

                <div class="flex flex-col">
                    <label>
                        Reason
                        <b class="text-red-500">*</b>
                    </label>
                    <SelectComponent v-model="questionReport.category">
                        <template #selectTrigger>
                            {{ QuestionReportCategory.getCategory(questionReport.category)?.name }}
                        </template>
                        <template #options>
                            <ListboxOption
                                v-for="category in QuestionReportCategory.categories"
                                v-slot="{ selected }"
                                :key="category.value"
                                as="template"
                                :value="category.value">
                                <li
                                    :class="[
                                        selected ?
                                            'text-neutral-700 bg-neutral-200 dark:bg-neutral-700'
                                        :   'text-gray-900',
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

                <div class="flex flex-col">
                    <label>
                        Message
                        <b class="text-red-500">*</b>
                    </label>
                    <TextInput v-model="questionReport.message" class="break-words" :is-textarea="true" rows="3" />
                </div>

                <p class="my-2">
                    <b>Note :</b>
                    Any abuse of reporting will be sanctioned
                </p>
            </section>
        </template>

        <template #buttons>
            <ButtonComponent state="danger" @click="onSubmit()">Report</ButtonComponent>
            <ButtonComponent state="cta" @click="showModal = false">Cancel</ButtonComponent>
        </template>
    </ModalComponent>

    <button
        class="hover:bg-neutral-100 dark:hover:bg-neutral-800 focus:bg-neutral-200 dark:focus:bg-neutral-700 transition-all w-10 h-10 rounded-lg hover:drop-shadow-sm"
        @click="showModal = true">
        <HeroIcon icon="FlagIcon" class="text-red-500 dark:text-red-400 rotate-6" />
    </button>
</template>
