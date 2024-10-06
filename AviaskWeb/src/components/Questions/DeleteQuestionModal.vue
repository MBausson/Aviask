<script lang="ts" setup>
import { ModalComponent, ButtonComponent } from "..";
import { AviaskQuestionDetails } from "@/models";

const emit = defineEmits(["deleteConfirmed"]);

const { question } = defineProps({
    question: {
        type: AviaskQuestionDetails,
        required: true,
    },
});

const model = defineModel("show", {
    type: Boolean,
    default: true,
    required: false,
});
</script>

<template>
    <ModalComponent v-model:show="model">
        <template #title>
            <h3 class="text-2xl font-semibold leading-6">Delete a question</h3>
        </template>

        <template #description>
            <span class="text-gray-700 dark:text-gray-200">
                Are you sure to delete the question
                <b>{{ question.title }}</b>
                ?
            </span>
        </template>

        <template #buttons>
            <ButtonComponent @click="model = false">Cancel</ButtonComponent>
            <ButtonComponent
                state="danger"
                @click="
                    model = false;
                    emit('deleteConfirmed', question);
                ">
                Delete
            </ButtonComponent>
        </template>
    </ModalComponent>
</template>
