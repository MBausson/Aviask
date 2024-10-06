<script lang="ts" setup>
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
const emit = defineEmits(["sourceUpdate"]);

function updateSource(source: string) {
    filteringStore.source = source;
    emit("sourceUpdate");
}

function resetSource() {
    filteringStore.removeSourceFilter();
    emit("sourceUpdate");
}
</script>
<template>
    <DropdownMenu :modal="false">
        <DropdownMenuTrigger>
            <ButtonComponent state="flat" class="flex items-center gap-2">
                <HeroIcon icon="BuildingLibraryIcon" />
                <span>
                    {{ filteringStore.source === undefined ? "Any source" : filteringStore.source }}
                </span>
            </ButtonComponent>
        </DropdownMenuTrigger>

        <DropdownMenuContent class="min-w-32 p-2 dark:bg-black">
            <DropdownMenuItem @click="resetSource">
                <button class="w-full flex items-center justify-between gap-4 font-semibold">
                    <p>Any source</p>
                    <HeroIcon icon="GlobeAltIcon" size="sm" />
                </button>
            </DropdownMenuItem>

            <DropdownMenuSeparator />

            <DropdownMenuItem v-for="source in ['EASA', 'CAA', 'FAA']" :key="source" @click="updateSource(source)">
                <button
                    class="w-full flex items-center justify-between gap-4"
                    :class="{
                        'font-medium': filteringStore.source === source,
                    }">
                    <p>{{ source }}</p>
                    <HeroIcon v-show="filteringStore.source === source" icon="CheckIcon" size="sm" />
                </button>
            </DropdownMenuItem>
        </DropdownMenuContent>
    </DropdownMenu>
</template>
