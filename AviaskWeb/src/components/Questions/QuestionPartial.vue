<script async lang="ts" setup>
import useApiStore from "@/stores/apiStore";
import { computed, ref, type Ref } from "vue";
import { AviaskQuestionDetails } from "@/models";
import { useRouter } from "vue-router";
import { marked } from "marked";
import { AnswerItem, ReportQuestionButton, HeroIcon, DateTime, ModalComponent, MarkdownPreview } from "@/components";
import { type CheckAnswerResult, type UserProfile } from "@/models/DTO/responseModels";
import useUsersRepository from "@/repositories/usersRepository";
import useQuestionsRepository from "@/repositories/questionsRepository";
import useUserStore from "@/stores/userStore";
import useEventBus from "@/eventBus";

/*
    TODO: Add a boolean prop to indicate if we show (or not) the explications
*/

const emit = defineEmits(["answered"]);
const { emitBus } = useEventBus();
const { getFullUrl } = useApiStore();
const router = useRouter();
const userStore = useUserStore();
const userRepository = useUsersRepository();
const questionsRepository = useQuestionsRepository();

const { questionId } = defineProps({
    questionId: {
        type: String,
        required: true,
    },
    isMockExam: {
        type: Boolean,
        required: false,
        default: () => false,
    },
});

//  Lets us have a non-falsy question ref, so we don't have to use question! everywhere
//  If the question is not found, the page is not rendered anyway
const question = ref<AviaskQuestionDetails>({} as AviaskQuestionDetails);
const publisher = ref<UserProfile>();
const answered: Ref<string | null> = ref(null);
const answerResult: Ref<CheckAnswerResult | null> = ref(null);
const showIllustrationModal = ref(false);

const markdownDescription = computed(() => marked(question.value.description));
const markdownExplications = computed(() => marked(answerResult?.value?.explications ?? ""));

async function fetchQuestion() {
    const questionResult = await questionsRepository.get(questionId);
    if (!questionResult.success || !questionResult.result) return router.push("/404");

    const publisherResult = await userRepository.profile(questionResult.result.publisherId);
    if (!publisherResult.success || !publisherResult.result) return router.push("/404");

    question.value = questionResult.result;
    publisher.value = publisherResult.result as UserProfile;
}

async function onAnswer(answer: string) {
    if (answered.value) return;
    const answerCheckResult = await questionsRepository.check(question.value.id, answer);

    if (!answerCheckResult.success) return emitBus("flashOpen", "Could not verify your answer", "danger");

    answerResult.value = answerCheckResult.result;
    answered.value = answer;

    emit("answered");
}

await fetchQuestion();
</script>

<template>
    <!-- eslint-disable vue/no-v-html -->
    <ModalComponent v-if="question.illustrationId" v-model:show="showIllustrationModal">
        <template #title>
            <div class="flex gap-1 flex-col">
                <h1 class="text-xl font-semibold dark:text-white">Illustration</h1>
                <h3 class="text-sm font-medium text-neutral-700 dark:text-neutral-300">Click to open in a new tab</h3>
            </div>
        </template>

        <template #description>
            <a target="_blank" :href="getFullUrl(`/api/question/${question.id}/illustration`)">
                <img
                    :src="getFullUrl(`/api/question/${question.id}/illustration`)"
                    title="Click to open in a new tab"
                    alt="Question's illustration preview"
                    class="rounded-sm max-h-72 sm:max-h-80 md:max-h-96" />
            </a>
        </template>
    </ModalComponent>

    <div class="h-full">
        <div class="flex justify-between items-center">
            <div class="flex items-center gap-2 md:gap-4">
                <RouterLink to="/questions">
                    <HeroIcon
                        icon="ChevronLeftIcon"
                        size="lg"
                        class="dark:text-white hover:scale-110 transition-all"
                        title="Questions list" />
                </RouterLink>

                <h1 class="text-xl sm:text-4xl font-semibold my-4 dark:text-white flex gap-1 items-center">
                    {{ question.title }}

                    <ReportQuestionButton v-if="userStore.isAuthenticated" :question="question" />
                </h1>
            </div>

            <RouterLink
                v-if="publisher"
                :to="`/user/${question.publisherId}/profile`"
                class="text-lg font-regular dark:text-gray-200 flex gap-1 items-center font-bold"
                :title="`Published by ${publisher?.userDetails.userName}`">
                <HeroIcon icon="UserCircleIcon" type="solid" />
                <span class="hidden sm:inline first-letter:capitalize">
                    {{ publisher!.userDetails.userName }}
                </span>
            </RouterLink>
        </div>

        <div class="grid lg:grid-cols-2 min-h-[60vh] gap-6">
            <h3 class="mt-6 lg:mt-16 text-pretty sm:text-lg dark:text-white break-words pr-3">
                <MarkdownPreview v-model="markdownDescription" />
            </h3>

            <div class="flex flex-col justify-center gap-5 py-7">
                <AnswerItem
                    v-for="(answer, i) in question.answers"
                    v-show="!answered || answer == answerResult?.correctAnswer || answer == answered"
                    :key="i"
                    :content="answer"
                    :result="answerResult"
                    :number="i + 1"
                    @answered="onAnswer(answer)" />

                <Transition enter-from-class="opacity-0" enter-to-class="transition-all duration-500 opacity-100">
                    <MarkdownPreview v-if="answerResult" v-model="markdownExplications" />
                </Transition>
            </div>
        </div>

        <div class="flex gap-6 flex-col-reverse items-center sm:items-start sm:justify-between sm:flex-row my-6">
            <span class="dark:text-neutral-200 font-medium">
                <RouterLink :to="`/questions?categoryFilter=${question.category.code}`">
                    {{ question.category.name }} [{{ question.category.code }}]
                </RouterLink>
                <span>
                    &ndash;
                    <DateTime :date="question.publishedAt"></DateTime>
                </span>
            </span>

            <img
                v-if="question.illustrationId"
                :src="getFullUrl(`/api/question/${question.id}/illustration`)"
                title="Click to open this image"
                alt="Question's illustration preview"
                class="overflow-hidden block w-40 relative mr-4 border border-neutral-600 p-1 dark:border-neutral-500 hover:dark:border-neutral-300 rounded-md origin-top-right transition-all duration-500 delay-150 ease-in-out"
                @click="showIllustrationModal = true" />
        </div>

        <div class="flex gap-1 md:gap-2 justify-center md:justify-end my-6 md:my-8 items-center">
            <slot name="buttons" />
        </div>
    </div>
</template>
