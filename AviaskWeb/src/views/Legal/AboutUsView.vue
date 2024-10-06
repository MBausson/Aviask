<script lang="tsx" setup>
import { HeroIcon, TitleComponent } from "@/components";
import type { SetupContext } from "vue";

const Accent = (_: unknown, context: SetupContext) => {
    return <p class="text-accent dark:text-[#3a81e8] inline">{context.slots.default!()}</p>;
};
</script>

<template>
    <div class="flex flex-col gap-2 sm:gap-6">
        <section>
            <TitleComponent class="my-2">About the developer</TitleComponent>

            <p class="sm:w-3/4 md:w-2/3 md:text-lg font-medium dark:text-gray-200 text-gray-700">
                Aviask has been created and developed by a
                <a class="font-bold" href="https://github.com/MBausson" target="_blank">french student</a>
                in computer science, heavily passionated about Aviation
            </p>
        </section>

        <section>
            <TitleComponent title-level="h2" class="my-2">Motivations for Aviask</TitleComponent>

            <span class="md:text-lg font-medium dark:text-gray-200 text-gray-700">
                Looking at existing ATPL questions bank websites, two
                <b>issues</b>
                come to mind

                <ul class="list-disc list-inside my-2">
                    <li>
                        These services can be
                        <Accent>expensive</Accent>
                        and can be a barrier to accessing knowledge
                    </li>
                    <li>
                        They lack of
                        <Accent>community</Accent>
                        engagement, which could provide valuable support and motivation for student pilots
                    </li>
                </ul>

                Recognizing that these issues can hinder learning, Aviask was designed to address them
            </span>
        </section>

        <section class="my-5 md:my-16 flex flex-col items-center gap-2">
            <TitleComponent class="mb-6 tracking-tight">Contact us</TitleComponent>

            <ul class="flex flex-col gap-5 p-4 self-center dark:text-white">
                <li class="flex items-center gap-3 text-lg">
                    <HeroIcon icon="EnvelopeIcon" />
                    <a class="text-gray-700 dark:text-gray-200" href="mailto:aviaskcontact@gmail.com">
                        aviaskcontact@gmail.com
                    </a>
                </li>

                <li class="flex gap-3 items-center text-lg dark:text-white">
                    <img
                        class="inline w-7 rounded-full dark:invert-[100%]"
                        src="@/assets/github-mark.svg"
                        alt="Github logo" />
                    <a
                        href="https://github.com/MBausson"
                        class="flex gap-1 items-center text-gray-700 dark:text-gray-200"
                        target="_blank">
                        Github profile
                    </a>
                </li>
            </ul>
        </section>
    </div>
</template>
