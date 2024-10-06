<script lang="ts" setup>
import { responseModels } from "@/models";
import { ModalComponent, ButtonComponent } from "..";
import { type PropType } from "vue";

const emit = defineEmits(["deleteConfirmed"]);

const { user } = defineProps({
    user: {
        type: Object as PropType<responseModels.UserExtended>,
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
            <h3 class="text-2xl font-medium leading-6">Delete a user</h3>
        </template>

        <template #description>
            <span class="text-gray-700 dark:text-gray-200">
                Are you sure to delete the user
                <b>{{ user.userName }}</b>
                ?
            </span>
        </template>

        <template #buttons>
            <ButtonComponent @click="model = false">Cancel</ButtonComponent>
            <ButtonComponent state="danger" @click="(model = false), emit('deleteConfirmed', user)">
                Delete
            </ButtonComponent>
        </template>
    </ModalComponent>
</template>
