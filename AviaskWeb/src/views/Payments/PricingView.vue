<script lang="tsx" setup>
import { ButtonComponent, HeroIcon, TitleComponent } from "@/components";
import useEventBus from "@/eventBus";
import usePaymentsRepository from "@/repositories/paymentsRepository";
import useUserStore from "@/stores/userStore";
import type { SetupContext } from "vue";
import { useRouter } from "vue-router";

const advantages = [
    "Unrestricted access to the questions bank",
    "Unlimited daily answers",
    "Prioritized suggestions review",
    "Access to practice exams",
    "Support the developer",
];

const userStore = useUserStore();
const paymentsRepository = usePaymentsRepository();
const router = useRouter();
const { emitBus } = useEventBus();

async function onBuy() {
    if (!userStore.isAuthenticated) {
        emitBus("flashOpen", "You need to be authenticated in order to subscribe to Premium", "danger");
        router.push("/users/login");

        return;
    }

    const checkoutResult = await paymentsRepository.checkout();

    if (!checkoutResult.success) return emitBus("flashOpen", checkoutResult.error.message, "flashOpen");

    window.open(checkoutResult.result);
}

const Card = (props: unknown, context: SetupContext) => {
    return (
        <div class="p-4 border-2 border-opacity-40 bg-white/25 dark:bg-dark-top-gradient/5 dark:border-white/10 border-black/5 backdrop-blur-sm p-4 rounded-md flex flex-col gap-6 dark:text-white">
            {context.slots.default!()}
        </div>
    );
};
</script>

<template>
    <section class="relative flex flex-col gap-14 text-center my-8 dark:text-white">
        <span
            class="absolute w-full h-28 mt-36 dark:mt-44 opacity-40 dark:opacity-30 blur-[90px] bg-[radial-gradient(50%_50%_at_50%_50%,#ad30f2_0,_#ad30f2)] dark:bg-[radial-gradient(50%_50%_at_50%_50%,#ad30f2_0,#ad30f2)]"></span>

        <TitleComponent>Unlock the full power of Aviask</TitleComponent>

        <p class="text-xl md:text-2xl font-medium">A subscription to boost your knowledge</p>
    </section>

    <Card class="mx-auto mt-20 w-full sm:w-3/4 lg:w-2/3">
        <div class="flex justify-center md:justify-normal">
            <p
                class="hidden md:inline absolute right-4 top-4 border-2 border-[#ad30f2] rounded-md text-lg text-[#ad30f2] px-2 py-0.5 font-bold uppercase tracking-wider dark:bg-purple-800/20"
                style="transition-behavior: overlay">
                Premium
            </p>

            <img
                class="self-start h-12 pointer-events-none"
                src="../../assets/icon-purple.svg"
                alt="Purple premium Aviask logo" />
        </div>

        <div class="flex flex-col items-center gap-8 md:grid md:grid-cols-3">
            <p class="flex items-baseline gap-3 md:flex-col my-auto">
                <span class="text-5xl font-bold tracking-tight">4.99€</span>
                <span class="text-center sm:text-left">per month</span>
            </p>

            <ul class="col-span-2">
                <li v-for="(item, i) in advantages" :key="i" class="flex gap-2 mb-2.5">
                    <HeroIcon icon="CheckCircleIcon" class="text-violet-500 dark:text-violet-300" type="solid" />
                    <span class="font-medium text-base sm:text-xl">{{ item }}</span>
                </li>
            </ul>
        </div>

        <ButtonComponent
            v-if="!userStore.userDetails?.isPremium"
            state="accent_purple"
            class="self-center min-w-1/2 px-8 flex justify-center gap-4 shadow-md"
            @click="onBuy">
            <HeroIcon class="hidden sm:inline" icon="ShoppingCartIcon" />
            <span>Subscribe to Premium</span>
        </ButtonComponent>

        <ButtonComponent
            v-else
            state="accent_purple"
            class="self-center w-full sm:w-fit sm:min-w-2/3 md:min-w-1/2 px-8 flex items-center justify-center gap-4 shadow-md"
            disabled>
            <HeroIcon class="hidden sm:inline" icon="CheckIcon" />
            <span>You are already subscribed to Premium</span>
        </ButtonComponent>
    </Card>
</template>
