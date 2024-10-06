<script lang="ts" setup>
import {
    ButtonComponent,
    HeroIcon,
    TableComponent,
    TimerComponent,
    BadgeComponent,
    ModalComponent,
} from "@/components";
import useEventBus from "@/eventBus";
import { AviaskMockExam } from "@/models/aviaskMockExam";
import type { AnswerRecordDetails } from "@/models/DTO/responseModels";
import useMockExamsRepository from "@/repositories/mockExamsRepository";
import { computed, ref } from "vue";

const nLastAnswers = 10;

const { emitBus } = useEventBus();
const mockExamsRepository = useMockExamsRepository();

const model = defineModel<AviaskMockExam>({
    type: AviaskMockExam,
    required: true,
});

const displayStopConfirmation = ref(false);

const lastAnswers = computed<AnswerRecordDetails[]>(() => {
    return [...model.value.answerRecords].slice(-nLastAnswers);
});
const progress = computed<number>(
    () => model.value.answerRecords.filter(a => a.isCorrect).length / model.value.maxQuestions,
);

async function onStop() {
    const res = await mockExamsRepository.stop();

    if (!res) {
        emitBus("flashOpen", "Could not stop this practice exam", "danger");
        return;
    }

    emitBus("flashOpen", "You stopped this mock exam", "success");
    location.reload();
}
</script>
<template>
    <ModalComponent v-model:show="displayStopConfirmation">
        <template #title>
            <h3 class="text-2xl font-medium leading-6">Stop this practice exam</h3>
        </template>

        <template #description>
            <p class="flex flex-col gap-8 my-2">
                <span>Are you sure to stop this practice exam ?</span>

                <span>
                    <b>Note:</b>
                    this action is irreversible
                </span>
            </p>
        </template>

        <template #buttons>
            <ButtonComponent state="cta" @click="displayStopConfirmation = false">Cancel</ButtonComponent>
            <ButtonComponent state="danger" @click="(displayStopConfirmation = false), onStop()">
                Stop this practice exam
            </ButtonComponent>
        </template>
    </ModalComponent>

    <div class="flex flex-col gap-10 bg-neutral-100 dark:bg-neutral-800 p-5 md:p-10 rounded-xl">
        <div class="flex gap-4 sm:justify-between flex-col sm:flex-row">
            <h3 class="flex items-center gap-2 dark:text-white">
                <span class="text-2xl font-semibold text-balance">Your current practice exam</span>
            </h3>

            <span
                class="flex items-center gap-1.5 bg-neutral-200 self-center p-3 rounded-md font-semibold tracking-wide tabular-nums dark:bg-neutral-600 dark:text-white pointer-events-none">
                <HeroIcon icon="ClockIcon" />
                <TimerComponent :start-date="model.startedAt" />
            </span>
        </div>

        <div class="flex flex-col gap-6 dark:text-white">
            <div class="flex items-center gap-6">
                <span class="flex gap-2 items-center font-medium">
                    <HeroIcon icon="ChartPieIcon" size="lg" />
                    <span class="font-medoum">Progress</span>
                </span>
                <div class="w-full flex justify-between gap-4">
                    <p class="flex gap-2 items-baseline">
                        <b>{{ model.answerRecords.length }}</b>
                        <span>/</span>
                        <span>{{ model.maxQuestions }}</span>
                    </p>
                    <BadgeComponent :badge-type="progress > 0.75 ? 'green' : 'red'">
                        {{ (progress * 100).toFixed() }} %
                    </BadgeComponent>
                </div>
            </div>

            <div class="flex items-center gap-6">
                <span class="flex gap-2 items-center font-medium">
                    <HeroIcon icon="ClockIcon" size="lg" />
                    <span class="font-medium">Time limit</span>
                </span>
                <div class="tracking-wider">{{ model.maxDuration.as("minutes") }} minutes</div>
            </div>

            <div class="flex items-center gap-6">
                <span class="flex gap-2 items-center font-medium">
                    <HeroIcon icon="TagIcon" />
                    <span class="font-medium">Category</span>
                </span>

                <RouterLink
                    class="hover:underline underline-offset-4"
                    :to="`/questions?categoryFilter=${model.category.code}`">
                    {{ model.category.name }}
                    [ {{ model.category.code }} ]
                </RouterLink>
            </div>

            <div v-if="model.answerRecords.length" class="flex flex-col gap-6 my-8">
                <span class="flex gap-2 items-center font-medium">
                    <HeroIcon icon="QueueListIcon" size="lg" />
                    <span class="font-medium">Last answers ({{ nLastAnswers }})</span>
                </span>

                <TableComponent :headers="['Question title', 'Description', 'Your answer', '']" :data="lastAnswers">
                    <template #row="{ item }">
                        <td class="font-medium">
                            {{ item.question.title }}
                        </td>

                        <td class="text-pretty">
                            <span class="hidden md:inline">{{ item.question.description.slice(0, 60) }}</span>
                        </td>

                        <td>
                            <BadgeComponent :badge-type="item.isCorrect ? 'green' : 'red'">
                                {{ item.answered }}
                            </BadgeComponent>
                        </td>

                        <td>
                            <div class="flex justify-end">
                                <RouterLink :to="`/question/${item.questionId}`">
                                    <ButtonComponent state="cta">
                                        <HeroIcon icon="ArrowTopRightOnSquareIcon" />
                                    </ButtonComponent>
                                </RouterLink>
                            </div>
                        </td>
                    </template>
                </TableComponent>
            </div>

            <div class="mt-6 flex justify-end">
                <ButtonComponent state="danger" @click="displayStopConfirmation = true">Stop now</ButtonComponent>
            </div>
        </div>
    </div>
</template>
