<script lang="ts" setup>
import { AviaskMockExam } from "@/models/aviaskMockExam";
import HeroIcon from "../Widgets/HeroIcon.vue";
import DateTime from "../Widgets/DateTime.vue";
import { BadgeComponent, TableComponent } from "..";
import moment from "moment";

const emit = defineEmits(["onDetailsExpand"]);

const { mockExam, showDetails } = defineProps({
    mockExam: {
        type: AviaskMockExam,
        required: true,
    },
    showDetails: {
        type: Boolean,
        default: false,
        required: false,
    },
});

const dateTimeFormat: Intl.DateTimeFormatOptions = {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
};
</script>
<template>
    <div
        class="px-3 md:px-5 py-3 rounded-md dark:text-white bg-neutral-100 dark:bg-dark-bottom-gradient transition-all"
        :class="{ 'hover:bg-opacity-70 hover:bg-neutral-200 dark:hover:bg-neutral-700': !showDetails }">
        <div class="flex justify-between items-center">
            <h3 class="font-medium text-sm md:text-base">
                {{ mockExam.category.name }} [ {{ mockExam.category.code }} ]
            </h3>

            <div class="flex gap-3 items-center">
                <BadgeComponent :badge-type="mockExam.correctnessRatio >= 0.75 ? 'green' : 'red'">
                    <span class="font-bold">{{ (mockExam.correctnessRatio * 100).toFixed() }} %</span>
                </BadgeComponent>

                <span @click="emit('onDetailsExpand')">
                    <HeroIcon v-if="showDetails" icon="ChevronUpIcon" size="lg" class="cursor-pointer" />
                    <HeroIcon v-else icon="ChevronDownIcon" size="lg" class="cursor-pointer" />
                </span>
            </div>
        </div>

        <div v-show="showDetails" class="my-6">
            <section class="flex justify-between gap-3 flex-wrap">
                <div class="flex items-center gap-5">
                    <span class="flex gap-2 items-center font-medium md:font-semibold">
                        <HeroIcon icon="ChartPieIcon" size="lg" />
                        <span>Progress</span>
                    </span>
                    <p class="flex gap-2 items-baseline">
                        <b>{{ mockExam.answerRecords.length }}</b>
                        <span>/</span>
                        <span>{{ mockExam.maxQuestions }}</span>
                    </p>
                </div>

                <div class="flex items-center gap-5">
                    <span class="flex gap-2 items-center font-medium md:font-semibold">
                        <HeroIcon icon="ClockIcon" size="lg" />
                        <span>Time limit</span>
                    </span>
                    <div class="tracking-wider">{{ moment.duration(mockExam.maxDuration).as("minutes") }} minutes</div>
                </div>

                <div class="flex items-center gap-5">
                    <span class="flex gap-2 items-center font-medium md:font-semibold">
                        <HeroIcon icon="CalendarDaysIcon" size="lg" />
                        <span>Started on</span>
                    </span>
                    <div class="tracking-wider">
                        <DateTime :date="mockExam.startedAt" :date-time-format />
                    </div>
                </div>
            </section>

            <section v-if="mockExam.answerRecords.length" class="mt-4">
                <h3 class="text-lg font-semibold my-8">Your answers</h3>

                <TableComponent
                    :headers="['Question title', 'Description', 'Your answer', '']"
                    :data="mockExam.answerRecords"
                    class="bg-neutral-200 dark:bg-dark-top-gradient rounded-lg">
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
            </section>
        </div>
    </div>
</template>
