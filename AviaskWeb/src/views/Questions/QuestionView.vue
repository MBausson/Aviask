<script lang="ts" setup>
import useFilteringStore from "@/stores/filteringStore";
import useUserStore from "@/stores/userStore";
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { QuestionPartial, SpinnerComponent, ButtonComponent, CategorySelector, HeroIcon } from "@/components";
import useEventBus from "@/eventBus";
import useQuestionsRepository from "@/repositories/questionsRepository";

const { emitBus } = useEventBus();
const router = useRouter();
const route = useRoute();
const filteringStore = useFilteringStore();
const userStore = useUserStore();
const questionsRepository = useQuestionsRepository();

const nextQuestionDisabled = ref<boolean>(true);
const questionId = ref<string>();

onMounted(() => {
    const id = route.params.id as string;

    if (!id) {
        router.push("/404");
        return;
    }

    questionId.value = id;
});

async function onNextQuestion() {
    if (!userStore.isAuthenticated) return emitBus("flashOpen", "Please log in to use this feature", "danger");

    const randomIdResult = await questionsRepository.randomId(filteringStore.categories.map(c => c.code));

    if (!randomIdResult.success || !randomIdResult.success)
        emitBus("flashOpen", "Could not find a next question", "danger");

    router.push(`/question/${randomIdResult.result}`);
}

setTimeout(() => {
    nextQuestionDisabled.value = false;
}, 2000);
</script>

<template>
    <Suspense v-if="questionId">
        <template #default>
            <QuestionPartial :question-id="questionId">
                <template #buttons>
                    <ButtonComponent
                        v-if="userStore.userHasRole('manager')"
                        class="inline-flex gap-2 items-center"
                        :router-link="`/question/${questionId}/edit`"
                        state="cta">
                        <HeroIcon icon="PencilIcon" type="solid" />
                        <span>Edit</span>
                    </ButtonComponent>

                    <ButtonComponent
                        class="flex gap-2 items-center"
                        state="cta"
                        :disabled="nextQuestionDisabled"
                        @click="onNextQuestion">
                        <HeroIcon icon="ArrowPathIcon" />
                        <span>Next question</span>
                    </ButtonComponent>

                    <CategorySelector v-if="userStore.isAuthenticated" :to-top="true" />
                </template>
            </QuestionPartial>
        </template>

        <template #fallback>
            <SpinnerComponent />
        </template>
    </Suspense>
</template>
