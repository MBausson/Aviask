<script lang="ts" setup>
import { computed, type PropType } from "vue";

export type ButtonState = "raised" | "flat" | "cta" | "danger" | "accent" | "accent_purple";

const { routerLink, state } = defineProps({
    routerLink: {
        type: String,
        default: undefined,
        required: false,
    },
    state: {
        type: String as PropType<ButtonState>,
        required: false,
        default: () => "raised",
    },
});

const tagType = computed<string>(() => {
    return routerLink === undefined ? "button" : "router-link";
});
</script>

<template>
    <component
        :is="tagType"
        class="text-center px-2 md:px-4 text-sm md:text-base py-2 rounded-lg font-medium md:font-semibold transition-all overflow-hidden whitespace-nowrap"
        :class="{
            'bg-white dark:bg-transparent dark:text-white border-2 border-black dark:border-white hover:bg-neutral-50 hover:dark:bg-transparent hover:dark:text-opacity-80 active:bg-neutral-100 active:dark:opacity-75':
                state == 'raised',
            'bg-neutral-200 dark:bg-dark-top-gradient dark:text-white hover:bg-neutral-300 dark:hover:bg-dark-top-gradient dark:hover:text-neutral-200 dark:active:text-neutral-300 active:bg-neutral-200':
                state == 'flat',
            'bg-black text-white border-2 border-black dark:border-transparent hover:bg-neutral-800 hover:dark:bg-neutral-950 dark:hover:text-neutral-100 active:bg-neutral-700 active:dark:bg-black active:dark:text-neutral-400 disabled:text-gray-300':
                state == 'cta',
            'bg-red-500 text-white border-2 border-red-500 hover:border-dashed hover:bg-transparent hover:dark:bg-transparent hover:text-red-500 active:bg-neutral-50':
                state == 'danger',
            'bg-shade-accent text-white hover:text-neutral-200 disabled:text-gray-300': state == 'accent',
            'bg-shade-purple text-white hover:text-neutral-200 disabled:text-gray-300': state == 'accent_purple',
        }"
        :to="routerLink">
        <slot></slot>
    </component>
</template>
