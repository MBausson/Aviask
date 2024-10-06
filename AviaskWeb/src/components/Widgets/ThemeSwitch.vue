<script lang="ts" setup>
import { HeroIcon } from "..";
import useThemeStore from "@/stores/themeStore";

const themeStore = useThemeStore();
</script>

<template>
    <button
        class="flex items-center outline-none transition-all bg-neutral-100 dark:bg-neutral-700 p-1 rounded-md border border-neutral-200 dark:border-neutral-600 hover:bg-neutral-200 dark:hover:bg-neutral-600"
        :title="`Switch theme to ${themeStore.theme == 'dark' ? 'light' : 'dark'}`"
        @click="themeStore.updateTheme(themeStore.theme == 'dark' ? 'light' : 'dark')">
        <HeroIcon v-if="themeStore.theme == 'dark'" icon="MoonIcon" size="md" />
        <HeroIcon v-else icon="SunIcon" size="lg" />
    </button>
</template>
