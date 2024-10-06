<script lang="ts" setup>
import { QuestionForm, TitleComponent } from "@/components";
import useEventBus from "@/eventBus";
import { AviaskQuestion } from "@/models/aviaskQuestion";
import { ref } from "vue";
import { useRouter } from "vue-router";
import useSuggestionsRepository from "@/repositories/suggestionsRepository";

const { emitBus } = useEventBus();
const router = useRouter();
const question = ref<AviaskQuestion>(AviaskQuestion.default());
const suggestionsRepository = useSuggestionsRepository();
const errorMessages = ref<string[]>([]);

async function onSubmit() {
    const res = await suggestionsRepository.create(question.value.toInterface());

    if (res.success) {
        router.push("/questions");

        return emitBus("flashOpen", "Thanks for submitting your question !", "success");
    }

    emitBus("flashOpen", res.error.message, "danger");
}
</script>

<template>
    <section class="my-6">
        <TitleComponent>{{ question.title || "Untitled question" }}</TitleComponent>

        <p class="text-gray-600 dark:text-gray-300 font-medium text-lg">
            After submitting your question, a staff member will review it and could make changes to your suggestion.
        </p>
    </section>

    <QuestionForm v-model="question" :error-messages="errorMessages" class="py-4 px-2" @submit="onSubmit" />
</template>
