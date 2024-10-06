<script lang="ts" setup>
import type { CheckAnswerResult } from "@/models/DTO/responseModels";
import { type PropType } from "vue";

const emit = defineEmits(["answered"]);

const { number, content, result } = defineProps({
    number: {
        required: true,
        type: Number,
    },
    content: {
        type: String,
        required: true,
    },
    result: {
        type: Object as PropType<CheckAnswerResult | null>,
        required: true,
    },
});

async function onAnswered() {
    if (result !== null) return;

    emit("answered");
}
</script>

<template>
    <button
        :disabled="result != null"
        class="w-full flex px-8 py-4 border-2 border-neutral-500 bg-gray-50 rounded-md dark:border-dark-top-gradient dark:bg-dark-bottom-gradient dark:text-white transition ease duration-150"
        :class="{
            'dark:hover:bg-dark-top-gradient hover:bg-neutral-100': result == null,
            //  If the user has answered
            'dark:border-opacity-30': result,
            '!border-green-600 dark:!border-green-600': result && content == result?.correctAnswer,
            '!border-red-500 border-dashed': result && content != result?.correctAnswer && !result?.isCorrect,
        }"
        @click="onAnswered">
        <span class="font-medium md:font-semibold">{{ String.fromCharCode(64 + number) }} &ndash;</span>
        <span class="mx-auto">{{ content }}</span>
    </button>
</template>
