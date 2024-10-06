<script lang="ts" setup>
import { QuestionForm, TitleComponent } from "@/components";
import useEventBus from "@/eventBus";
import { AviaskQuestion } from "@/models/aviaskQuestion";
import useQuestionsRepository from "@/repositories/questionsRepository";
import useUserStore from "@/stores/userStore";
import { type Ref, ref } from "vue";
import { useRouter } from "vue-router";

const questionsRepository = useQuestionsRepository();
const userStore = useUserStore();
const router = useRouter();
const { emitBus } = useEventBus();

const question: Ref<AviaskQuestion> = ref<AviaskQuestion>(AviaskQuestion.default());

async function onSubmit() {
    const res = await questionsRepository.create(question.value.toInterface());

    if (res.success) {
        router.push(`/question/${res.result.id}`);
        emitBus("flashOpen", `Created question ${res.result.title} successfully`, "success");

        return;
    }

    emitBus("flashOpen", res.error.message, "danger");
}
</script>

<template>
    <template v-if="userStore.userHasRole('manager')">
        <TitleComponent class="my-6">{{ question.title || "Untitled question" }}</TitleComponent>

        <QuestionForm v-model="question" class="py-4 px-2" @submit="onSubmit" />
    </template>
</template>
