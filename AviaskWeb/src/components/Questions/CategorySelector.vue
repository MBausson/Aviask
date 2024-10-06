<script lang="ts" setup>
import { AtplCategory } from "@/models/DTO/responseModels";
import useFilteringStore from "@/stores/filteringStore";
import { ButtonComponent, HeroIcon } from "..";
import {
    DropdownMenu,
    DropdownMenuTrigger,
    DropdownMenuContent,
    DropdownMenuSeparator,
    DropdownMenuItem,
} from "@/components/ui/dropdown-menu";

const filteringStore = useFilteringStore();
const emit = defineEmits(["categoryUpdate"]);

function onResetCategory() {
    filteringStore.categories = [];
    emit("categoryUpdate");
}

function onSourceSet(category: AtplCategory) {
    filteringStore.toggleCategory(category);
    emit("categoryUpdate");
}
</script>

<template>
    <DropdownMenu :modal="false">
        <DropdownMenuTrigger>
            <ButtonComponent class="flex gap-2 items-center" state="flat">
                <HeroIcon icon="FunnelIcon" />
                <span class="overflow-hidden">
                    {{
                        filteringStore.categories.length == 0
                            ? "Any categories"
                            : `Categories - ${filteringStore.categories.length}`
                    }}
                </span>
            </ButtonComponent>
        </DropdownMenuTrigger>

        <DropdownMenuContent class="min-w-56 max-w-80 p-2 dark:bg-black">
            <DropdownMenuItem @click="onResetCategory">
                <button class="flex items-center justify-between w-full gap-8">
                    <p class="font-medium md:font-semibold">Any categories</p>
                    <HeroIcon icon="TrashIcon" size="sm" />
                </button>
            </DropdownMenuItem>

            <DropdownMenuSeparator />

            <DropdownMenuItem
                v-for="category in AtplCategory.categories"
                :id="category.name"
                :key="category.code"
                class="py-2 mb-0.5"
                @click="onSourceSet(category)">
                <button class="flex items-center justify-between gap-4 w-full">
                    <p>
                        <span classs="font-medium">[{{ category.code }}]</span>
                        {{ category.name }}
                    </p>
                    <HeroIcon v-show="filteringStore.categories.includes(category)" icon="CheckIcon" size="sm" />
                </button>
            </DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenu>
</template>
