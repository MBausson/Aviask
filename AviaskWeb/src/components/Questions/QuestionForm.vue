<script lang="ts" setup>
import { responseModels } from "@/models";
import { onMounted, ref, computed } from "vue";
import {
    BadgeComponent,
    TextInputAnswer,
    ButtonComponent,
    SelectComponent,
    TextInput,
    HeroIcon,
    FileInput,
    IconButton,
    MarkdownTextarea,
} from "@/components";
import { AviaskQuestion } from "@/models/aviaskQuestion";
import useEventBus from "@/eventBus";
import { AtplCategory, QuestionVisibility } from "@/models/DTO/responseModels";
import { ListboxOption } from "@headlessui/vue";
import useApiStore from "@/stores/apiStore";
import useQuestionsRepository from "@/repositories/questionsRepository";

const emit = defineEmits(["submit"]);
const { getFullUrl } = useApiStore();
const questionsRepository = useQuestionsRepository();
const { emitBus } = useEventBus();
const { errorMessages } = defineProps({
    errorMessages: {
        default: [] as string[],
        type: Array<string>,
    },
});

const questionModel = defineModel({
    required: true,
    type: AviaskQuestion,
});
const illustrationFile = ref<File | null>(null);
const correctAnswerNumber = ref<number | undefined>(undefined);

const sources = computed<string[]>(() => {
    const src = ["EASA", "FAA", "CAA"]; //  TODO: improve

    if (!src.includes(questionModel.value.source)) {
        src.push(questionModel.value.source);
    }

    return src;
});

function onCorrectAnswerSet(answerNumber: number | undefined) {
    correctAnswerNumber.value = answerNumber;
}

function onSubmit() {
    if (!correctAnswerNumber.value) {
        emitBus("flashOpen", "No correct answer is selected", "danger");
        return;
    }

    questionModel.value.questionAnswers.correctAnswer =
        questionModel.value.questionAnswers.answers[correctAnswerNumber.value! - 1];

    illustrationFile.value = null;

    emit("submit");
}

async function removeIllustration() {
    const result = await questionsRepository.editIllustration(questionModel.value.id, null);

    if (result === null || result.toString().length === 0) {
        emitBus("flashOpen", "Removed the illustration for this question", "success");

        questionModel.value.illustrationId = null;
        illustrationFile.value = null;
        return;
    }

    emitBus("flashOpen", "Could not remove the illustration", "danger");
}

async function onIllustrationFileChange(newValue: File | null) {
    if (!questionModel.value.id) return;

    const result = await questionsRepository.editIllustration(questionModel.value.id, newValue);

    if (result instanceof Error) {
        emitBus("flashOpen", "Could not upload this illustration", "danger");

        return;
    }

    questionModel.value.illustrationId = result;
    emitBus("flashOpen", "Successfully uploaded illustration", "success");
}

onMounted(() => {
    if (!questionModel.value.questionAnswers.correctAnswer) return;

    correctAnswerNumber.value =
        questionModel.value.questionAnswers.answers.findIndex(
            v => v == questionModel.value.questionAnswers.correctAnswer,
        ) + 1;
});
</script>

<template>
    <p
        v-if="errorMessages?.length"
        class="flex flex-col gap-2 mt-2 mb-6 bg-red-200 font-semibold text-red-800 rounded-lg mx-2 px-6 py-5">
        <span v-for="message in errorMessages" :key="message">
            {{ message }}
        </span>
    </p>
    <form
        enctype="multipart/form-data"
        class="w-full flex flex-col gap-6 bg-neutral-100 dark:bg-neutral-800 pt-6 px-4 sm:px-10 pb-3 rounded-xl"
        @submit.prevent="">
        <div class="grid grid-rows-2 md:grid-rows-none md:grid-cols-2 gap-10">
            <div class="flex flex-col gap-1">
                <label class="dark:text-white font-medium text-lg">
                    Title
                    <sup class="text-red-500">*</sup>
                </label>
                <TextInput
                    v-model="questionModel.title"
                    placeholder="Question's title"
                    minlength="2"
                    maxlength="128"
                    required />
            </div>

            <div class="grid grid-cols-2 gap-2">
                <div class="flex flex-col gap-1">
                    <label class="dark:text-white font-medium text-lg">Source</label>
                    <SelectComponent v-model="questionModel.source">
                        <template #selectTrigger>
                            {{ questionModel.source }}
                        </template>
                        <template #options>
                            <ListboxOption
                                v-for="source in sources"
                                v-slot="{ selected }"
                                :key="source"
                                as="template"
                                :value="source">
                                <li
                                    :class="[
                                        selected
                                            ? 'text-neutral-700 bg-neutral-200 dark:bg-neutral-700'
                                            : 'text-gray-900',
                                        'relative cursor-default select-none py-2 pl-10 pr-4',
                                    ]">
                                    <span :class="[selected ? 'font-medium' : 'font-normal', 'block truncate']">
                                        {{ source }}
                                    </span>

                                    <span v-if="selected" class="absolute inset-y-0 left-0 flex items-center pl-3">
                                        <HeroIcon icon="CheckIcon" />
                                    </span>
                                </li>
                            </ListboxOption>
                        </template>
                    </SelectComponent>
                </div>

                <div class="flex flex-col gap-1">
                    <label class="dark:text-white font-medium text-lg">Visibility</label>
                    <SelectComponent v-model="questionModel.visibility">
                        <template #selectTrigger>
                            {{ questionModel.visibility.name }}
                        </template>
                        <template #options>
                            <ListboxOption
                                v-for="visibility in QuestionVisibility.visibilities"
                                v-slot="{ selected }"
                                :key="visibility.value"
                                as="template"
                                :value="visibility">
                                <li
                                    :class="[
                                        selected
                                            ? 'text-neutral-700 bg-neutral-200 dark:bg-neutral-700'
                                            : 'text-gray-900',
                                        'relative cursor-default select-none py-2 pl-10 pr-4',
                                    ]">
                                    <span :class="[selected ? 'font-medium' : 'font-normal', 'block truncate']">
                                        {{ visibility.name }}
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

        <div class="flex flex-col gap-2">
            <label class="dark:text-white font-medium text-lg">
                Category
                <sup class="text-red-500">*</sup>
            </label>
            <SelectComponent
                v-model="questionModel.category"
                :values="responseModels.AtplCategory.categories"
                placeholder="Select an ATPL category"
                required>
                <template #selectTrigger>
                    {{ questionModel.category.name }}
                </template>
                <template #options>
                    <ListboxOption
                        v-for="category in AtplCategory.categories"
                        v-slot="{ selected }"
                        :key="category.code"
                        as="template"
                        :value="category">
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

        <div class="flex flex-col gap-2">
            <label class="dark:text-white font-medium text-lg">
                Description
                <sup class="text-red-500">*</sup>
            </label>
            <MarkdownTextarea v-model="questionModel.description" placeholder="What the question is about" required />
        </div>

        <div class="grid grid-rows-2 md:grid-cols-2 md:grid-rows-none gap-x-10 gap-y-4 auto-rows-fr">
            <div
                v-for="(_, answerIndex) in questionModel.questionAnswers.answers"
                :key="answerIndex"
                class="flex flex-col gap-2">
                <span class="dark:text-white font-medium text-lg flex justify-between gap-4">
                    <span class="flex gap-2 items-start">
                        <p class="pointer-events-none">
                            Answer {{ answerIndex + 1 }}
                            <sup v-if="answerIndex < 2" class="text-red-500">*</sup>
                        </p>

                        <button
                            v-if="answerIndex > 1 && answerIndex + 1 === questionModel.questionAnswers.answers.length"
                            class="text-red-500"
                            @click="questionModel.questionAnswers.answers.pop()">
                            <HeroIcon icon="TrashIcon" size="sm" />
                        </button>
                    </span>
                    <span>
                        <BadgeComponent v-if="correctAnswerNumber == answerIndex + 1" :badge-type="'green'">
                            Correct answer
                        </BadgeComponent>
                        <BadgeComponent v-else :badge-type="'red'">Wrong answer</BadgeComponent>
                    </span>
                </span>

                <TextInputAnswer
                    v-model="questionModel.questionAnswers.answers[answerIndex]"
                    :answer-number="answerIndex + 1"
                    :correct-answer-number="correctAnswerNumber"
                    :is-optional="answerIndex < 2"
                    @on-correct-answer-set="onCorrectAnswerSet" />
            </div>

            <div v-if="questionModel.questionAnswers.answers?.length != 4" class="flex flex-col gap-2 self-end">
                <ButtonComponent
                    class="flex justify-center gap-2 py-3"
                    state="accent"
                    @click="questionModel.questionAnswers.answers.push('')">
                    <HeroIcon icon="PlusIcon" />
                    <span>Add a new answer</span>
                </ButtonComponent>
            </div>
        </div>

        <div class="flex flex-col gap-2">
            <label class="dark:text-white font-medium text-lg flex justify-between gap-4">Explications</label>
            <MarkdownTextarea
                v-model="questionModel.questionAnswers.explications"
                placeholder="Clarify the question's correct answer"
                maxlength="512" />
        </div>

        <div v-if="questionModel.id" class="flex flex-col gap-2">
            <label class="dark:text-white font-medium text-lg flex justify-between gap-4">Illustration</label>
            <FileInput
                v-if="!questionModel.illustrationId"
                v-model="illustrationFile"
                :allowed-extensions="['png', 'jpeg', 'jpg']"
                :max-size-k-b="3000"
                @change="onIllustrationFileChange" />
            <div v-else class="w-32 relative shadow-md">
                <img
                    class="-z-10 p-2"
                    alt="Question illustration preview"
                    :src="getFullUrl(`/api/question/${questionModel.id}/illustration`)" />
                <button class="absolute top-0 right-0" @click="removeIllustration">
                    <IconButton
                        icon="TrashIcon"
                        class="[&>*]:!text-red-500 bg-neutral-300/50 dark:bg-neutral-500/50 backdrop-blur-sm rounded-full block border border-neutral-600/20 dark:border-neutral-500/50 m-0.5"
                        size="md"
                        title="Remove this file" />
                </button>
            </div>
        </div>

        <p v-else class="font-medium flex items-center gap-1 dark:text-white">
            <HeroIcon icon="QuestionMarkCircleIcon" />
            <span>Illustration can only be added when editing a question or suggestion</span>
        </p>

        <div class="flex justify-center py-4">
            <ButtonComponent class="flex flex-row items-center gap-2" type="button" state="cta" @click="onSubmit">
                <HeroIcon icon="PaperAirplaneIcon" />
                <span>Submit</span>
            </ButtonComponent>
        </div>
    </form>
</template>
