<script setup lang="ts">
import { TitleComponent } from "@/components";
import { IsFeatureEnabled } from "@/lib/utils";
import type { AviaskUserDetails } from "@/models/DTO/responseModels";
import useUsersRepository from "@/repositories/usersRepository";
import useUserStore from "@/stores/userStore";
import { computed } from "vue";
import { useRouter } from "vue-router";

if (!IsFeatureEnabled("leaderboar")) {
    const router = useRouter();

    router.push("/");
}

const userStore = useUserStore();
const usersRepository = useUsersRepository();

const leaderboard = (await usersRepository.leaderboard())?.result;

const isCurrentUserInLeaderboard = computed<boolean>(() =>
    leaderboard.users.map(u => u.user.id).includes(userStore.userDetails!.id),
);

function isCurrentUser(user: AviaskUserDetails): boolean {
    return userStore.userDetails?.id === user.id;
}
</script>
<template>
    <section class="py-6 mb-8 flex flex-col gap-6">
        <TitleComponent>Leaderboard</TitleComponent>

        <p class="text-neutral-800 dark:text-neutral-200">
            This ranking thanks the users who contribute the most to the Aviask questions bank
        </p>
    </section>

    <div v-if="leaderboard" class="flex flex-col gap-3">
        <div
            v-for="userLeaderboard in leaderboard.users"
            :key="userLeaderboard.rank"
            class="px-10 py-4 rounded-lg bg-neutral-100 dark:bg-neutral-600 dark:text-white flex items-center gap-8 hover:brightness-95 transition-all"
            :class="{
                '!bg-yellow-300 dark:!bg-yellow-500 text-xl': userLeaderboard.rank == 1,
                '!bg-slate-300 dark:!bg-gray-400 text-lg': userLeaderboard.rank == 2,
                '!bg-orange-300 dark:!bg-orange-500': userLeaderboard.rank == 3,
                'dark:text-neutral-300': userLeaderboard.rank > 3,
                'border-2 border-black/20 dark:border-white/50': isCurrentUser(userLeaderboard.user),
            }">
            <span class="font-semibold text-lg">
                <span>{{ userLeaderboard.rank }}</span>
            </span>

            <div class="flex justify-between w-full">
                <RouterLink :to="`/user/${userLeaderboard.user.id}/profile`" class="hover:underline underline-offset-4">
                    <span>{{ userLeaderboard.user.userName }}</span>
                    <span v-if="isCurrentUser(userLeaderboard.user)" class="ml-1 font-semibold">(You)</span>
                </RouterLink>

                <p class="font-medium">
                    {{ userLeaderboard.questionsCount }}
                    <span class="hidden sm:inline">contributions</span>
                </p>
            </div>
        </div>

        <div
            v-if="userStore.isAuthenticated && !isCurrentUserInLeaderboard"
            class="px-10 py-4 rounded-lg bg-neutral-100 dark:bg-neutral-600 dark:text-white flex items-center gap-8 mt-3 md:mt-5">
            <span class="font-semibold text-lg">
                <span>?</span>
            </span>

            <div class="flex justify-between w-full">
                <RouterLink :to="`/user/${userStore.userDetails!.id}/profile`" class="flex-1">
                    {{ userStore.userDetails!.username }}
                    <span class="font-semibold">(You)</span>
                </RouterLink>

                <p class="font-medium">{{ leaderboard.currentUserCount }}</p>
            </div>
        </div>
    </div>
    <div v-else>Could not load leaderboard...</div>
</template>
