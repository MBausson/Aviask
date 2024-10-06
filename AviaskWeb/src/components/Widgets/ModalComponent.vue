<script lang="ts" setup>
import { Dialog, DialogPanel, TransitionRoot, TransitionChild } from "@headlessui/vue";

const model = defineModel("show", {
    type: Boolean,
    required: true,
    default: true,
});

function closeModal() {
    model.value = false;
}
</script>

<template>
    <TransitionRoot appear :show="model" as="template">
        <Dialog as="div" class="relative z-10" @close="closeModal">
            <TransitionChild
                as="template"
                enter="duration-300 ease-out"
                enter-from="opacity-0"
                enter-to="opacity-100"
                leave="duration-200 ease-in"
                leave-from="opacity-100"
                leave-to="opacity-0">
                <div class="fixed inset-0 bg-black/25 dark:bg-black/20 backdrop-blur-sm" />
            </TransitionChild>

            <div class="fixed inset-0 overflow-y-auto">
                <div class="flex min-h-full items-center justify-center p-4 text-center">
                    <TransitionChild
                        as="template"
                        enter="duration-300 ease-out"
                        enter-from="opacity-0 scale-95"
                        enter-to="opacity-100 scale-100"
                        leave="duration-200 ease-in"
                        leave-from="opacity-100 scale-100"
                        leave-to="opacity-0 scale-95">
                        <DialogPanel
                            class="w-full max-w-md transform overflow-hidden rounded-xl bg-white border-black/30 dark:border-white/20 dark:bg-dark-top-gradient dark:text-white p-5 text-left align-middle shadow-xl transition-all">
                            <span class="flex items-center justify-between gap-3">
                                <slot name="title"></slot>
                                <span
                                    class="hover:cursor-pointer hover:text-neutral-600 dark:hover:text-neutral-300 font-bold self-start"
                                    @click="closeModal">
                                    &#x2715;
                                </span>
                            </span>

                            <div class="mt-6">
                                <slot name="description"></slot>
                            </div>

                            <div class="mt-6 flex justify-end gap-3">
                                <slot name="buttons"></slot>
                            </div>
                        </DialogPanel>
                    </TransitionChild>
                </div>
            </div>
        </Dialog>
    </TransitionRoot>
</template>
