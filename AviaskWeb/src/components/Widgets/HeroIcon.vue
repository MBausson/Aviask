<script lang="ts" setup>
import type { PropType } from "vue";

const outlinedIcons = await import("@heroicons/vue/24/outline");
const solidIcons = await import("@heroicons/vue/24/solid");

export type IconName = keyof typeof outlinedIcons;
export type IconSize = "sm" | "md" | "lg";
export type IconType = "outlined" | "solid";

const { icon, size, type } = defineProps({
    icon: {
        required: true,
        type: String as PropType<IconName>,
    },
    size: {
        required: false,
        type: String as PropType<IconSize>,
        default: () => "md"!,
    },
    type: {
        required: false,
        type: String as PropType<IconType>,
        default: () => "outlined"!,
    },
});

const iconComponent = (type === "outlined" ? outlinedIcons : solidIcons)[icon];
</script>
<template>
    <component
        :is="iconComponent"
        class="inline"
        :class="{
            'w-4': size == 'sm',
            'w-6': size == 'md',
            'w-7': size == 'lg',
        }"></component>
</template>
