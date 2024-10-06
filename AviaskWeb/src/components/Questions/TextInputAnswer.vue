<script lang="ts" setup>
import { TextInput, ButtonComponent, HeroIcon } from "@/components";

const emit = defineEmits(["onCorrectAnswerSet", "update:modelValue"]);

const { correctAnswerNumber, answerNumber } = defineProps({
    correctAnswerNumber: {
        type: Number,
        required: true,
        default: 0,
    },
    answerNumber: {
        type: Number,
        required: true,
    },
    isOptional: {
        type: Boolean,
        required: false,
        default: false,
    },
});

const model = defineModel({
    required: true,
    type: String,
});

function onCorrectAnswerSet(value: number | undefined) {
    emit("onCorrectAnswerSet", value);
}

function unsetCorrectAnswerIfEmpty(newValue: string) {
    if (!newValue && correctAnswerNumber == answerNumber) {
        onCorrectAnswerSet(undefined);
    }
}
</script>

<template>
    <div class="flex">
        <TextInput
            v-model="model"
            minlegth="2"
            maxlength="128"
            :class="{
                'w-full': true,
                'rounded-tr-none rounded-br-none': model,
            }"
            :required="isOptional"
            :placeholder="isOptional ? 'Optional' : ''"
            @input="
                $emit('update:modelValue', ($event.target! as HTMLInputElement).value);
                unsetCorrectAnswerIfEmpty(($event.target! as HTMLInputElement).value);
            " />

        <div class="flex">
            <!--  Show the delete answer button only if the field is optional && it is not the correct answer -->
            <!-- <ButtonComponent
                v-if="!isOptional && correctAnswerNumber !== answerNumber"
                state="flat"
                class="flex items-center text-red-400 rounded-none h-full">
                <HeroIcon icon="TrashIcon" />
            </ButtonComponent> -->

            <!-- Show the set correct answer button only if the field has value && it is not the actual correct answer -->
            <ButtonComponent
                v-if="model && correctAnswerNumber != answerNumber"
                state="flat"
                class="flex items-center text-accent rounded-tl-none rounded-bl-none h-full"
                @click="onCorrectAnswerSet(answerNumber)">
                <HeroIcon icon="CheckIcon" />
            </ButtonComponent>

            <!-- Show the remove correct answer button only if the field has value && it is the actual correct answer -->
            <ButtonComponent
                v-else-if="model"
                class="flex items-center rounded-tl-none rounded-bl-none h-full"
                state="cta"
                @click="onCorrectAnswerSet(undefined)">
                <HeroIcon icon="MinusIcon" />
            </ButtonComponent>
        </div>
    </div>
</template>
