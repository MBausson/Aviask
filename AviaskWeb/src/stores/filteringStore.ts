import { AtplCategory } from "@/models/DTO/responseModels";
import { defineStore } from "pinia";
import { computed, ref } from "vue";

const useFilteringStore = defineStore("filteringStore", () => {
    const categories = ref<AtplCategory[]>([]);
    const source = ref<string | undefined>();

    const hasSourceFilter = computed(() => source.value !== undefined);

    function toggleCategory(category: AtplCategory) {
        const index = categories.value.indexOf(category);

        if (index !== -1) {
            categories.value.splice(categories.value.indexOf(category), 1);

            return;
        }

        categories.value.push(category);
    }

    function removeSourceFilter() {
        source.value = undefined;
    }

    return {
        categories,
        toggleCategory,
        source,
        hasSourceFilter,
        removeSourceFilter
    };
});

export default useFilteringStore;
