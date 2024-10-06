<script lang="ts" setup type="">
import { defineModel, type PropType } from "vue";

const emit = defineEmits(["update:modelValue", "delayedInput"]);

let lastTimeoutId: NodeJS.Timeout;

const { delay, isPassword, isTextarea } = defineProps({
    delay: {
        required: false,
        default: -1,
        type: Number,
    },
    isTextarea: {
        type: Boolean,
        default: false,
    },
    isPassword: {
        required: false,
        type: Boolean,
        default: false,
    },
});

const model = defineModel({
    required: true,
    type: String as PropType<unknown>,
});

function onInput() {
    //  Debounce
    if (delay <= 0) return;

    if (lastTimeoutId) {
        clearInterval(lastTimeoutId);
    }

    lastTimeoutId = setTimeout(() => emit("delayedInput"), delay);
}
</script>

<template>
    <component
        :is="isTextarea ? 'textarea' : 'input'"
        v-model="model"
        :value="model"
        :type="isPassword ? 'password' : 'text'"
        class="placeholder:font-medium focus:ring-0 ring-0 active:ring-0 border-l-4 border-l-transparent dark:bg-neutral-600 dark:text-white font-medium md:font-semibold bg-neutral-200 pl-6 pr-4 py-3 rounded-lg focus:outline-0 dark:focus:bg-neutral-600 focus:bg-neutral-300 transition-all"
        :class="{
            'invalid:rounded-l-none invalid:border-l-red-500': model,
            'resize-none': isTextarea,
        }"
        @input="
            emit('update:modelValue', ($event.target! as HTMLInputElement).value);
            onInput();
        " />
</template>
