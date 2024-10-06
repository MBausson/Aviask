<script lang="ts" setup generic="T">
import { type PropType, defineModel, computed } from "vue";
import type { FilteredResult } from "@/models/DTO/responseModels";
import HeroIcon from "./Widgets/HeroIcon.vue";

const emit = defineEmits(["update:modelValue"]);

const { filtered, maxItems } = defineProps({
    filtered: {
        required: true,
        type: Object as PropType<FilteredResult<T>>,
    },
    maxItems: {
        required: false,
        type: Number,
        default: 25,
    },
});

const hasPreviousPage = computed<boolean>(() => page.value > 1);
const hasNextPage = computed<boolean>(
    () => filtered.elements.length > 0 && filtered.totalCount > page.value * maxItems,
);
const showPaginator = computed<boolean>(() => hasNextPage.value || hasPreviousPage.value);

const page = defineModel<number>("page", { required: true, default: 1 });
</script>

<template>
    <div v-show="showPaginator" class="flex items-center gap-3">
        <button v-show="hasPreviousPage" class="text-accent" @click="emit('update:modelValue', 1)">
            <HeroIcon icon="ChevronDoubleLeftIcon" />
        </button>

        <div class="flex gap-4 items-center dark:text-neutral-300 font-medium">
            <p v-show="hasPreviousPage" class="select-none cursor-pointer" @click="emit('update:modelValue', page - 1)">
                {{ page - 1 }}
            </p>

            <span class="text-accent select-none font-bold pointer-events-none underline underline-offset-4">
                {{ page }}
            </span>

            <span v-show="hasNextPage" class="select-none cursor-pointer" @click="emit('update:modelValue', page + 1)">
                {{ page + 1 }}
            </span>
        </div>

        <button
            v-show="hasNextPage"
            class="text-accent"
            @click="emit('update:modelValue', Math.floor(filtered.totalCount / maxItems) + 1)">
            <HeroIcon icon="ChevronDoubleRightIcon" />
        </button>
    </div>
</template>
