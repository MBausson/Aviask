<script lang="ts" setup>
import type { PropType } from "vue";

type TitleLevel = "h1" | "h2" | "h3";

const { titleLevel } = defineProps({
    titleLevel: {
        type: String as PropType<TitleLevel>,
        required: false,
        default: () => "h1" as TitleLevel,
    },
});
</script>

<template>
    <component
        :is="titleLevel"
        class="dark:text-white"
        :class="{
            'scroll-m-20 text-4xl font-bold tracking-tight lg:text-5xl': titleLevel === 'h1',
            'scroll-m-20 pb-2 text-3xl font-semibold tracking-tight first:mt-0': titleLevel === 'h2',
            'scroll-m-20 text-2xl font-semibold tracking-tight': titleLevel === 'h3',
        }">
        <slot></slot>
    </component>
</template>
