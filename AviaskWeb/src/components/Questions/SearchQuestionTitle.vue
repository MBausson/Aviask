<script lang="ts" setup>
import { TextInput } from "@/components";

defineEmits(["onInput", "update:modelValue"]);

const model = defineModel({
    required: true,
    type: String,
});
</script>

<template>
    <div>
        <TextInput
            v-model="model"
            placeholder="Search a question title..."
            class="w-full rounded-tr-none rounded-br-none"
            @input="$emit('update:modelValue', ($event.target! as HTMLInputElement).value)"
            @delayed-input="$emit('onInput')" />
    </div>
</template>
