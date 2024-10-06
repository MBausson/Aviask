<script lang="ts" setup>
import useEventBus from "@/eventBus";
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { QuestionForm, TitleComponent } from "@/components";
import useSuggestionsRepository from "@/repositories/suggestionsRepository";
import type { AviaskQuestion } from "@/models/aviaskQuestion";

const { emitBus } = useEventBus();
const router = useRouter();
const route = useRoute();
const suggestionsRepository = useSuggestionsRepository();

const suggestionId = route.params.id as string;
const question = ref<AviaskQuestion | undefined>();

async function onSubmit() {
    if (!question.value) return;

    const res = await suggestionsRepository.edit(question.value.id, question.value.toInterface());

    if (res.success) {
        emitBus("flashOpen", `Suggestion ${question.value.title} has been updated`, "success");

        return router.push(`/suggestions`);
    }

    emitBus("flashOpen", res.error.message, "danger");
}

onMounted(async () => {
    const res = await suggestionsRepository.get(suggestionId);

    if (!res.success) {
        router.push("/404");

        return emitBus("flashOpen", res.error.message, "danger");
    }

    question.value = res.result;
});
</script>

<template>
    <template v-if="question">
        <section class="my-6 dark:text-white flex items-center justify-between">
            <TitleComponent>{{ question?.title || "Untitled question" }}</TitleComponent>
            <span class="text-lg font-semibold pointer-events-none">Editing a suggestion</span>
        </section>

        <QuestionForm v-if="question" v-model="question" class="py-4 px-2" @submit="onSubmit" />
    </template>
</template>
