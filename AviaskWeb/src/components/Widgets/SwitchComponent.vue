<script lang="ts" setup>
import { Switch } from "@headlessui/vue";
import { ref } from "vue";

const emit = defineEmits(["update:modelValue"]);

const { modelValue } = defineProps({
    modelValue: {
        required: true,
        type: Boolean,
    },
});

const enabled = ref(modelValue);
</script>

<template>
    <Switch
        v-model="enabled"
        :class="modelValue ? 'bg-accent' : 'bg-neutral-300 dark:bg-neutral-500'"
        class="relative inline-flex h-6 w-11 items-center rounded-full group"
        @update:model-value="emit('update:modelValue', $event)">
        <span
            :class="modelValue ? 'translate-x-6' : 'translate-x-1'"
            class="inline-block h-4 w-4 transform rounded-full bg-white group-hover:scale-110 transition" />
    </Switch>
</template>
