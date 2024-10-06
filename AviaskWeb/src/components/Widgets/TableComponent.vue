<script lang="ts" setup generic="T">
import type { PropType } from "vue";

const { headers, data } = defineProps({
    headers: {
        type: Array<string>,
        required: true,
    },
    data: {
        required: true,
        type: Object as PropType<T[]>,
    },
});
</script>

<template>
    <div class="overflow-x-auto">
        <table class="min-w-full">
            <thead class="hidden">
                <tr>
                    <th
                        v-for="(header, i) in headers"
                        :key="i"
                        class="py-3 px-4 bg-gray-100 dark:bg-neutral-700 dark:text-gray-200 text-gray-600 font-semibold first:rounded-l-xl last:rounded-r-xl">
                        {{ header }}
                    </th>
                </tr>
            </thead>

            <tbody v-if="data.length !== 0" class="first:[&>*>td]:rounded-l-xl last:[&>*>td]:rounded-r-xl">
                <TransitionGroup
                    enter-active-class="transition transition-all duration-1000 ease-in-out"
                    enter-from-class="opacity-30"
                    enter-to-class="opacity-100">
                    <tr
                        v-for="(item, i) in data"
                        :key="i"
                        class="transition-all align-middle [&>td]:py-3 [&>*]:px-4 dark:hover:bg-white/5 hover:bg-gray-500/10 dark:text-white">
                        <slot name="row" :item="item"></slot>
                    </tr>
                </TransitionGroup>
            </tbody>
            <tbody v-else>
                <tr>
                    <td
                        :colspan="headers.length"
                        class="text-center py-8 text-lg font-medium tracking-wide dark:text-gray-100">
                        No data to show...
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>
