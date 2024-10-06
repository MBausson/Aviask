<script lang="ts" setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { QuestionPartial, SpinnerComponent, ButtonComponent, HeroIcon, TimerComponent } from "@/components";
import useEventBus from "@/eventBus";
import useMockExamRepository from "@/repositories/mockExamsRepository";
import useMockExamStore from "@/stores/mockExamStore";

const { emitBus } = useEventBus();
const router = useRouter();
const route = useRoute();
const mockExamRepository = useMockExamRepository();
const mockExamStore = useMockExamStore();

const showNextQuestion = ref<boolean>(false);
const nextQuestionId = ref<string | null>(null);
const questionId = ref<string>();

async function onNextQuestion() {
    if (!nextQuestionId.value) nextQuestionId.value = (await mockExamRepository.next()).result;

    if (!nextQuestionId.value) {
        emitBus("flashOpen", "There's no questions left", "success");
        router.push("/mockExam");

        return;
    }

    await router.push(`/mockExam/question/${nextQuestionId.value}`);
}

async function onAnswer() {
    showNextQuestion.value = true;

    await mockExamStore.updateCurrent();

    if (!mockExamStore.hasNextQuestion) {
        router.push("/mockExam");
        emitBus("flashOpen", "You finished your practice exam", "success");
    }
}

onMounted(() => {
    const id = route.params.id as string;

    if (!id) {
        router.push("/404");
        return;
    }

    questionId.value = id;
});

await mockExamStore.updateCurrent();
</script>

<template>
    <Suspense v-if="questionId">
        <template #default>
            <QuestionPartial :question-id="questionId" :is-mock-exam="true" @answered="onAnswer">
                <template #buttons>
                    <span
                        class="flex items-center gap-1.5 bg-neutral-200 self-center py-3 px-4 rounded-md font-bold tracking-wide tabular-nums dark:bg-neutral-600 dark:text-white pointer-events-none">
                        <HeroIcon icon="ClockIcon" />
                        <TimerComponent v-if="mockExamStore.current" :start-date="mockExamStore.current.startedAt" />
                    </span>

                    <ButtonComponent
                        v-if="showNextQuestion"
                        class="flex gap-2 items-center"
                        state="cta"
                        title="Available once you answer this  question"
                        @click="onNextQuestion">
                        <span>Next question</span>
                        <HeroIcon icon="ArrowRightCircleIcon" />
                    </ButtonComponent>

                    <ButtonComponent class="flex items-center gap-2" state="flat" router-link="/mockExam">
                        <HeroIcon icon="ArrowTopRightOnSquareIcon" />
                        <span>Practice exam</span>
                    </ButtonComponent>
                </template>
            </QuestionPartial>
        </template>

        <template #fallback>
            <SpinnerComponent />
        </template>
    </Suspense>
</template>
