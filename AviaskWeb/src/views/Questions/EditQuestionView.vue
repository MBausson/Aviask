<script lang="ts" async setup>
import { onMounted, ref, type Ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import useEventBus from "@/eventBus";
import { QuestionForm, TitleComponent } from "@/components";
import type { AviaskQuestion } from "@/models/aviaskQuestion";
import useQuestionsRepository from "@/repositories/questionsRepository";

const questionsRepository = useQuestionsRepository();
const { emitBus } = useEventBus();

const route = useRoute();
const router = useRouter();

const question = ref<AviaskQuestion | null>(null);
const errorMessage: Ref<string[]> = ref<string[]>([]);

const id = route.params.id as string;

async function onSubmit() {
    if (!question.value) return;

    const res = await questionsRepository.edit(question.value.toInterface());

    if (res.success) {
        emitBus("flashOpen", `Question ${question.value.title} has been updated`, "success");

        return router.push(
            question.value.status.name === "Accepted" ? `/question/${question.value.id}` : "/user/publications",
        );
    }

    emitBus("flashOpen", res.error.message, "danger");
}

onMounted(async () => {
    if (id == undefined) return router.push("/404");

    const res = await questionsRepository.getExtended(id);

    if (!res.success) {
        router.push("/");

        return emitBus("flashOpen", res.error.message, "danger");
    }

    question.value = res.result;
});
</script>

<template>
    <TitleComponent class="my-6">{{ question?.title || "Untitled question" }}</TitleComponent>

    <QuestionForm
        v-if="question && question.questionAnswers.correctAnswer !== undefined"
        v-model="question"
        class="py-4 px-2"
        :error-messages="errorMessage"
        @submit="onSubmit" />
</template>
