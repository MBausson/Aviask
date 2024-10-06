<script lang="ts" setup>
import { HeroIcon, MarkdownPreview, TextInput } from "..";
import { computed, ref, useAttrs } from "vue";
import { marked } from "marked";

const attrs = useAttrs();

const model = defineModel({
    required: true,
    type: String,
    default: "",
});

const showPreview = ref(true);

const markdownResult = computed(() => {
    return marked(model.value || "", { gfm: true }).toString();
});
</script>

<template>
    <div class="flex flex-col gap-2 relative">
        <TextInput v-model="model" :is-textarea="true" v-bind="attrs" rows="5" />

        <div class="absolute right-0 flex items-center gap-1 mr-1">
            <img
                src="../../assets/markdown_icon.svg"
                class="w-6 aspect-square inline dark:invert"
                alt="Markdown logo" />
            <button class="dark:text-white" title="Toggle markdown preview" @click="showPreview = !showPreview">
                <HeroIcon v-if="showPreview" icon="EyeSlashIcon" type="solid" />
                <HeroIcon v-else icon="EyeIcon" type="solid" />
            </button>
        </div>

        <Transition
            enter-active-class="transition-all"
            leave-active-class="transition-all"
            enter-from-class="opacity-0"
            leave-to-class="opacity-0">
            <MarkdownPreview
                v-if="showPreview"
                v-model="markdownResult"
                class="text-black dark:text-white bg-slate-200 dark:bg-slate-600 p-4 rounded-md" />
        </Transition>
    </div>
</template>
