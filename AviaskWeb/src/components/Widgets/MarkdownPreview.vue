<script lang="ts" setup>
import { sanitize } from "isomorphic-dompurify";
import { computed, defineModel } from "vue";

const model = defineModel({
    required: true,
    type: String,
});

const sanitizedModel = computed(() => sanitize(model.value));
</script>
<template>
    <!-- eslint-disable vue/no-v-html -->

    <div v-if="model.length > 0" class="md-preview flex flex-col gap-1 md:gap-2" v-html="sanitizedModel"></div>
</template>

<style>
.md-preview h1 {
    @apply font-bold text-xl;
}
.md-preview h2 {
    @apply font-semibold text-lg;
}

.md-preview h3 {
    @apply font-semibold;
}

.md-preview p {
    @apply font-medium;
}

.md-preview code {
    @apply p-2 inline-block my-3 bg-gray-400 rounded-lg font-mono dark:bg-slate-700;
}

.md-preview ul {
    @apply list-disc list-inside;
}

.md-preview ol ol,
.md-preview ul ul,
.md-preview ol ul,
.md-preview ul ol {
    @apply ml-4;
}

.md-preview ol {
    @apply list-decimal list-inside;
}
</style>
