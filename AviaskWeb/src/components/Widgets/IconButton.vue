<script lang="ts" setup>
import { ref, type PropType } from "vue";
import { HeroIcon } from "@/components";
import type { IconName, IconSize } from "./HeroIcon.vue";

const { disabled, icon, size } = defineProps({
    disabled: {
        type: Boolean,
        required: false,
        default: () => false,
    },
    icon: {
        type: String as PropType<IconName>,
        required: true,
    },
    size: {
        required: false,
        type: String as PropType<IconSize>,
        default: () => "md",
    },
});

const showToolTip = ref<boolean>(false);
</script>

<template>
    <span @mouseenter="showToolTip = true" @mouseleave="showToolTip = false">
        <button
            class="text-black dark:text-white disabled:opacity-40 disabled:border-0 hover:scale-110 inline-flex justify-center items-center hover:bg-transparent dark:hover:border-white transition-all duration-300 rounded-md p-2 dark:active:opacity-80"
            :disabled="disabled">
            <HeroIcon :icon="icon" :size="size" />
        </button>
    </span>
</template>

<style scoped>
.show-enter-active,
.show-leave-active {
    transition: all 0.3s ease-out;
}

.show-enter-from,
.show-leave-to {
    opacity: 0;
}
</style>
