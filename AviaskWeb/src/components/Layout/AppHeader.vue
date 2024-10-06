<script lang="tsx" async setup>
import { type SetupContext } from "vue";
import { UserHeader } from "@/components/Layout";
import useUserStore from "@/stores/userStore";
import DefaultLogo from "@/assets/icon.svg";
import PurpleLogo from "@/assets/icon-purple.svg";
import { RouterLink } from "vue-router";
import { IsFeatureEnabled } from "@/lib/utils";

const userStore = useUserStore();

const Item = (props: { to: string }, context: SetupContext) => {
    return (
        <p class="text-center font-medium text-sm sm:text-base">
            <RouterLink
                class="dark:text-neutral-100 sm:hover:bg-neutral-100 dark:sm:hover:bg-neutral-700 p-2 md:p-3 rounded-lg transition-all block"
                to={props.to}>
                {context.slots.default!()}
            </RouterLink>
        </p>
    );
};
</script>

<template>
    <nav
        class="top-0 w-full bg-transparent backdrop-blur-lg z-10 px-1.5 sm:px-8 py-4 flex justify-between gap-0.5 items-center dark:bg-dark-primary dark:text-white">
        <div class="flex items-center gap-1 sm:gap-2.5 md:gap-10">
            <RouterLink to="/" class="flex items-center gap-1 md:gap-3 [&>img]:hover:scale-110 min-w-10">
                <img
                    :src="userStore.userDetails?.isPremium ? PurpleLogo : DefaultLogo"
                    alt="Aviask logo representend by 3 aerofoils"
                    class="w-16 transition-transform md:w-24 duration-300 select-none" />
            </RouterLink>

            <div class="flex md:gap-2 items-center text-neutral-700">
                <Item to="/questions">Bank of questions</Item>

                <Item v-if="IsFeatureEnabled('leaderboard')" to="/leaderboard">Leaderboard</Item>

                <Item v-if="userStore.isAuthenticated" to="/mockExam">Practice exams</Item>
                <Item v-else to="/aboutus">About us</Item>

                <Item to="/payments/pricing">Pricing</Item>
            </div>
        </div>

        <Suspense>
            <template #default>
                <UserHeader />
            </template>
            <template #fallback>
                <span class="text-lg pointer-events-none font-semibold animate-bounce">...</span>
            </template>
        </Suspense>
    </nav>
</template>
