<script lang="ts" setup>
import type { PropType } from "vue";

export type BadgeType = "red" | "green" | "orange" | "purple";

const { badgeType } = defineProps({
    badgeType: {
        type: String as PropType<BadgeType>,
        required: true,
    },
});
</script>

<template>
    <span
        v-if="badgeType == 'red'"
        :class="`bg-red-100 text-red-800 text-xs leading-relaxed font-medium mr-2 px-2.5 py-0.5 rounded-full dark:bg-red-900 dark:text-red-300`">
        <slot></slot>
    </span>

    <span
        v-else-if="badgeType == 'green'"
        :class="`bg-green-100 text-green-800 text-xs leading-relaxed font-medium mr-2 px-2.5 py-0.5 rounded-full dark:bg-green-900 dark:text-green-300`">
        <slot></slot>
    </span>

    <span
        v-else-if="badgeType === 'orange'"
        :class="`bg-orange-100 text-orange-800 text-xs leading-relaxed font-medium mr-2 px-2.5 py-0.5 rounded-full dark:bg-amber-900 dark:text-amber-300`">
        <slot></slot>
    </span>

    <span
        v-else-if="badgeType === 'purple'"
        :class="`bg-purple-300 text-purple-900 text-xs leading-relaxed font-medium mr-2 px-2.5 py-0.5 rounded-full dark:bg-purple-800 dark:text-purple-200`">
        <slot></slot>
    </span>
</template>
