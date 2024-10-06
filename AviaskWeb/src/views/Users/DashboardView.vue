<script lang="ts" setup>
import {
    TrendsLinePerformance,
    DashboardCard,
    PolarAreaCategories,
    HeroIcon,
    TitleComponent,
    DateTime,
} from "@/components";
import { AtplCategory, type UserStatistics } from "@/models/DTO/responseModels";
import useUsersRepository from "@/repositories/usersRepository";
import { ref } from "vue";

const userRepository = useUsersRepository();

const statistics = ref<UserStatistics | null>((await userRepository.statistics()).result ?? null);
</script>
<template>
    <template v-if="statistics">
        <TitleComponent>Dashboard</TitleComponent>
        <p class="text-gray-500 dark:text-gray-400 mb-10 font-medium text-lg">
            <DateTime :show-since="false" />
        </p>

        <div class="flex flex-col gap-5 mb-10">
            <div class="grid md:grid-cols-2 gap-5">
                <DashboardCard class="bg-green-100 dark:bg-green-800">
                    <template #title>
                        <HeroIcon icon="TrophyIcon" />
                        <span>Ready for exam</span>
                    </template>

                    <template v-if="statistics.readyForExamCategories.length > 0" #content>
                        <span
                            v-for="category in statistics.readyForExamCategories"
                            :key="category.category"
                            class="flex justify-between gap-3 items-center">
                            <RouterLink
                                :to="`/questions?categoryFilter=${category.category}`"
                                class="font-medium hover:underline underline-offset-2">
                                [ {{ category.category }} ]
                                {{ AtplCategory.getCategory(category.category)!.name }}
                            </RouterLink>
                            <span class="whitespace-nowrap">
                                <span class="font-medium">{{ category.answerCount }}</span>
                                answers &ndash;
                                <span class="text-emerald-700 dark:text-emerald-400">
                                    {{ (category.correctnessRatio * 100).toFixed(1) }} %
                                </span>
                            </span>
                        </span>
                    </template>

                    <template v-else #content>
                        <p class="flex flex-col gap-2 items-center md:font-semibold">
                            <span>Not enough data</span>
                            <RouterLink to="/questions">
                                <u class="underline-offset-2">Start</u>
                                answering questions !
                            </RouterLink>
                        </p>
                    </template>
                </DashboardCard>

                <DashboardCard class="bg-pink-100 dark:bg-fuchsia-900">
                    <template #title>
                        <span class="flex items-center gap-2 dark:text-white">
                            <HeroIcon icon="ArrowTrendingDownIcon" />
                            <span>Weak categories</span>
                        </span>
                    </template>

                    <template v-if="statistics.weakestCategories.length > 0" #content>
                        <span
                            v-for="category in statistics.weakestCategories"
                            :key="category.category"
                            class="flex justify-between gap-3 dark:text-white">
                            <RouterLink
                                :to="`/questions?categoryFilter=${category.category}`"
                                class="font-medium text-gray-700 dark:text-gray-200 hover:underline underline-offset-2">
                                [ {{ category.category }} ]
                                {{ AtplCategory.getCategory(category.category)!.name }}
                            </RouterLink>
                            <span class="whitespace-nowrap">
                                <span class="font-medium">{{ category.answerCount }}</span>
                                answers &ndash;
                                <span class="text-red-700 dark:text-red-400">
                                    {{ (category.correctnessRatio * 100).toFixed(1) }} %
                                </span>
                            </span>
                        </span>
                    </template>

                    <template v-else #content>
                        <p class="flex flex-col gap-2 items-center md:font-semibold">
                            <span>Not enough data</span>
                            <RouterLink to="/questions">
                                <u class="underline-offset-2">Start</u>
                                answering questions !
                            </RouterLink>
                        </p>
                    </template>
                </DashboardCard>
            </div>
            <div class="flex gap-5 flex-col md:flex-row justify-evenly">
                <DashboardCard class="self-center w-full h-full bg-sky-100 dark:bg-sky-900">
                    <template #title>
                        <section class="flex flex-col gap-1 lg:flex-row lg:items-center lg:gap-6">
                            <span class="flex gap-2 items-center">
                                <HeroIcon icon="PresentationChartLineIcon" />
                                <span>Performance trends</span>
                            </span>
                            <span class="text-base text-neutral-700 dark:text-neutral-200">
                                Correct answers for the past
                                <b>30</b>
                                days
                            </span>
                        </section>
                    </template>

                    <template
                        v-if="statistics.last30DaysCorrectAnswers || statistics.last30DaysCorrectAnswers"
                        #content>
                        <TrendsLinePerformance
                            v-if="statistics.last30DaysCorrectAnswers.length"
                            class="dark:text-black"
                            :correct-data="statistics.last30DaysCorrectAnswers"
                            :wrong-data="statistics.last30DaysWrongAnswers" />
                    </template>

                    <template v-else #content>
                        <p class="flex flex-col gap-2 items-center md:font-semibold">
                            <span>Not enough data</span>
                            <RouterLink to="/questions">
                                <u class="underline-offset-2">Start</u>
                                answering questions !
                            </RouterLink>
                        </p>
                    </template>
                </DashboardCard>

                <DashboardCard class="bg-purple-50 h-fit dark:bg-purple-800 [&>*]:dark:text-black">
                    <template #title>
                        <span class="flex gap-2 items-center dark:text-white">
                            <HeroIcon icon="TagIcon" />
                            <span>Categories</span>
                        </span>
                    </template>

                    <template v-if="statistics.totalCategories.length > 0" #content>
                        <PolarAreaCategories
                            class="w-60 self-center"
                            :data="statistics.totalCategories.filter(v => v.correctnessRatio >= 0.01)" />
                    </template>

                    <template v-else #content>
                        <p class="flex flex-col gap-2 items-center md:font-semibold dark:text-white text-center">
                            <span>Not enough data</span>
                            <RouterLink to="/questions">
                                <u class="underline-offset-2">Start</u>
                                answering questions !
                            </RouterLink>
                        </p>
                    </template>
                </DashboardCard>
            </div>
        </div>
    </template>

    <template v-else>
        <TitleComponent class="my-6">Could not load your dashboard</TitleComponent>

        <p class="font-semibold text-2xl text-neutral-800 dark:text-neutral-200 text-center">
            Please retry later, or contact us.
        </p>
    </template>
</template>
