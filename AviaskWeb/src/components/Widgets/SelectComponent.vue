<script lang="ts" setup generic="T extends string | number | boolean | object | null">
import { type PropType } from "vue";
import { Listbox, ListboxButton, ListboxOptions } from "@headlessui/vue";
import { HeroIcon } from "..";

const { placeholder } = defineProps({
    placeholder: {
        type: String,
        default: undefined,
        required: false,
    },
});

const model = defineModel({
    type: Object as PropType<T>,
    required: true,
});
</script>

<template>
    <Listbox v-model="model">
        <div class="relative">
            <ListboxButton
                class="w-full cursor-default rounded-lg bg-neutral-200 dark:bg-neutral-600 dark:text-white py-3 pl-3 pr-10 text-left focus:outline-none focus-visible:ring-2 sm:text-sm">
                <span class="block truncate font-medium">
                    <span v-if="model === undefined && placeholder !== undefined">{{ placeholder }}</span>
                    <slot v-else name="selectTrigger"></slot>
                </span>
                <span class="pointer-events-none absolute inset-y-0 right-0 flex items-center pr-2">
                    <HeroIcon icon="ChevronDownIcon" />
                </span>
            </ListboxButton>

            <Transition
                leave-active-class="transition duration-100 ease-in"
                leave-from-class="opacity-100"
                leave-to-class="opacity-0">
                <ListboxOptions
                    class="absolute my-1 z-10 max-h-60 w-full overflow-auto rounded-lg bg-neutral-100 hover:[&>*]:bg-neutral-200 dark:hover:[&>*]:bg-neutral-800 [&>*]:dark:!text-white dark:bg-dark-secondary shadow-md ring-1 ring-white/10 focus:outline-none sm:text-sm">
                    <slot name="options"></slot>
                </ListboxOptions>
            </Transition>
        </div>
    </Listbox>
</template>
