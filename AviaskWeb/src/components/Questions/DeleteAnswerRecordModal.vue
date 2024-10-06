<script lang="ts" setup>
import { type PropType } from "vue";
import { ModalComponent, ButtonComponent } from "..";
import type { AviaskAnswerRecordDetails } from "@/models/aviaskAnswerRecordDetails";

const emit = defineEmits(["deleteConfirmed"]);

const { answerRecord } = defineProps({
    answerRecord: {
        type: Object as PropType<AviaskAnswerRecordDetails>,
        required: true,
    },
});

const model = defineModel("show", {
    type: Boolean,
    required: true,
});
</script>

<template>
    <ModalComponent v-model:show="model">
        <template #title>
            <h3 class="text-2xl font-semibold leading-6">Delete a question record</h3>
        </template>

        <template #description>
            <span class="text-gray-700 dark:text-gray-200">
                Are you sure to delete the question record
                <b>{{ answerRecord?.question.title }}</b>
                ?
            </span>
        </template>

        <template #buttons>
            <ButtonComponent @click="model = false">Cancel</ButtonComponent>
            <ButtonComponent
                state="danger"
                @click="
                    model = false;
                    emit('deleteConfirmed', answerRecord);
                ">
                Delete
            </ButtonComponent>
        </template>
    </ModalComponent>
</template>
