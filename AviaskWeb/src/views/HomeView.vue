<script lang="tsx" setup>
import useThemeStore from "@/stores/themeStore";
import type { SetupContext } from "vue";
import { TitleComponent } from "@/components";

const themeStore = useThemeStore();

const Accent = (props: unknown, context: SetupContext) => {
    return <p class="text-accent dark:text-[#3a81e8] inline">{context.slots.default!()}</p>;
};

const Card = (props: unknown, context: SetupContext) => {
    return (
        <div class="border md:border-2 border-opacity-40 bg-white/25 dark:bg-black/5 dark:border-white/10 border-black/5 backdrop-blur-sm p-4 rounded-md flex flex-col gap-6 dark:text-white">
            {context.slots.default!()}
        </div>
    );
};
</script>

<template>
    <div class="flex flex-col gap-6 md:gap-12">
        <div class="relative mt-4 md:mt-16 flex flex-col gap-5 items-center py-14 dark:text-white">
            <span
                class="absolute w-full h-24 blur-[95px] mt-32 bg-[radial-gradient(100%_100%_at_50%_50%,#1c71ed_0,_#00dc8200)] md:bg-[radial-gradient(62%_90%_at_50%_20%,#1c71ed_0,_#00dc8200)]"></span>

            <TitleComponent class="text-center">Learn, Together.</TitleComponent>
            <TitleComponent title-level="h2" class="text-2xl sm:text-3xl md:text-4xl md:font-bold text-center">
                The tool for future
                <Accent>pilots</Accent>
            </TitleComponent>
        </div>

        <div class="flex flex-col md:flex-row gap-8 justify-between py-5 md:py-8">
            <Card>
                <p class="flex items-center gap-8">
                    <img
                        class="accent-filter"
                        src="../assets/LandingPage/comm.svg"
                        alt="3 people connected in a network" />

                    <TitleComponent title-level="h3">Improve together</TitleComponent>
                </p>

                <div v-if="true" class="[&>*]:text-lg font-medium flex flex-col gap-2">
                    <p>Join a community-driven platform designed for future pilots !</p>
                    <p>Our modern and unique approach empowers users to shape their learning experience</p>
                </div>
            </Card>

            <Card>
                <p class="flex items-center gap-8">
                    <svg viewBox="0 0 21 21" class="w-1/4 md:1/4 lg:w-1/6 stroke-accent fill-transparent">
                        <g stroke-linecap="round" stroke-linejoin="round" transform="translate(4 3)">
                            <path d="m.5.5v11c0 1.1045695.8954305 2 2 2h11" />

                            <path d="m2.5 9.5 3-3 2 2 5-5" />

                            <path d="m12.5 6.5v-3h-3" />
                        </g>
                    </svg>

                    <TitleComponent title-level="h3">Boost your progress</TitleComponent>
                </p>

                <div class="[&>*]:text-lg font-medium flex flex-col gap-2">
                    <p>Dive deeper into your learning journey thanks to our comprehensive analytics feature</p>
                </div>
            </Card>
        </div>

        <div class="text-center dark:text-white mt-3 mb-10">
            <TitleComponent title-level="h2" class="mb-2">
                Identify your
                <Accent>weakness</Accent>
                , learn faster
            </TitleComponent>

            <span class="font-normal sm:font-medium text-lg">
                Aviask provides an intuitive dashboard to track your progress
                <br />
                <span class="text-lg font-medium sm:font-semibold">
                    So you know when you are
                    <Accent>ready</Accent>
                    for your exams
                </span>
            </span>
        </div>

        <div class="bg-gradient-to-tr from-accent to-blue-600/50 border-transparent p-0.5 rounded-lg">
            <span
                class="absolute left-0 w-full h-24 opacity-60 blur-3xl bg-[radial-gradient(100%_100%_at_50%_50%,#1c71ed_0,_#00dc8200)] md:bg-[radial-gradient(62%_90%_at_50%_20%,#1c71ed_0,_#00dc8200)]"></span>
            <img
                v-if="themeStore.theme === 'light'"
                class="rounded-md shadow-xl ring-1 relative ring-gray-300/5"
                alt="Example of Aviask dashboard"
                src="../assets/LandingPage/sample.png" />
            <img
                v-else
                class="rounded-md shadow-xl ring-1 relative ring-gray-300/5"
                alt="Example of Aviask dashboard"
                src="../assets/LandingPage/sample_dark.png" />
        </div>

        <p class="text-center text-lg font-medium dark:text-white">
            <RouterLink to="/users/register">
                <Accent class="font-bold text-xl">Register</Accent>
            </RouterLink>
            and join a strong community of dedicated learners willing to succeed at ATPL exams
        </p>
    </div>
</template>

<style scoped>
.title-shadow {
    background: radial-gradient(#1c71ed 0, #00dc8200);
    filter: blur(120px);
}

.accent-filter {
    filter: invert(35%) sepia(51%) saturate(2369%) hue-rotate(202deg) brightness(92%) contrast(102%);
}
</style>
