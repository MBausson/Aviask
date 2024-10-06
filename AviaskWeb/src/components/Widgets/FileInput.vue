<script lang="ts" setup>
import { defineModel, watch } from "vue";
import { HeroIcon, IconButton } from "..";
import useEventBus from "@/eventBus";

const { emitBus } = useEventBus();
const emit = defineEmits(["change"]);

const { allowedExtensions, maxSizeKB } = defineProps({
    allowedExtensions: {
        required: false,
        type: Array as () => string[],
        default: () => ["jpg", "jpeg", "png", "gif", "pdf", "txt"],
    },
    maxSizeKB: {
        required: false,
        type: Number,
        default: () => 16000,
    },
});

const model = defineModel<File | null>({
    required: true,
});

watch(model, (v: File | null) => {
    emit("change", v);
});

function handleFileChange(event: Event) {
    const fileInput = event.target as HTMLInputElement;
    const file = fileInput.files?.[0];

    if (!file) return;

    const fileNameSplitted = file.name.split(".");

    if (fileNameSplitted.length <= 1) {
        emitBus("flashOpen", "Your file doesn't has any extensions", "danger");
        return;
    }

    const extension = fileNameSplitted[fileNameSplitted.length - 1].toLowerCase();

    if (!allowedExtensions.includes(extension)) {
        emitBus("flashOpen", `Extension '.${extension}' is not allowed`, "danger");
        return;
    }

    const sizeInKb = file.size / 1024;

    if (sizeInKb > maxSizeKB) {
        emitBus("flashOpen", `Your file exceeds the size limit of ${maxSizeKB.toLocaleString()} KB`, "danger");
        return;
    }

    model.value = file;
}
</script>
<template>
    <label
        for="file-upload"
        class="self-center sm:self-auto w-40 aspect-square bg-neutral-200 dark:bg-dark-top-gradient rounded-lg box-content group transition-all duration-500 p-0.5">
        <div
            class="border-4 border-dashed border-neutral-300 group-hover:border-neutral-400 dark:border-neutral-500 group-hover:dark:border-neutral-400 rounded-md h-full flex justify-center items-center flex-col gap-2 transition-colors duration-500">
            <template v-if="model">
                <button
                    @click="
                        (e: Event) => {
                            model = null;
                            e.preventDefault();
                        }
                    ">
                    <IconButton
                        icon="TrashIcon"
                        class="[&>*]:!text-red-500 bg-neutral-300 dark:bg-neutral-500 rounded-full block p-1"
                        size="md"
                        title="Remove this file" />
                </button>
                <p class="text-center text-neutral-600 dark:text-neutral-300">{{ model.name.slice(0, 35) }}</p>
            </template>
            <template v-else>
                <HeroIcon class="dark:text-neutral-300" icon="CloudArrowUpIcon" />
                <p class="flex flex-col items-center font-medium text-neutral-600 dark:text-neutral-300">
                    <span>Upload a file</span>
                    <span class="text-sm">(Max. {{ maxSizeKB.toLocaleString() }} KB)</span>
                </p>
            </template>
        </div>
    </label>
    <input id="file-upload" class="hidden" type="file" @change="handleFileChange" />
</template>
