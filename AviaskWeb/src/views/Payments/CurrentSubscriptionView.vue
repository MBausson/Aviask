<script lang="ts" setup>
import { ButtonComponent, ModalComponent, SpinnerComponent, HeroIcon, TitleComponent } from "@/components";
import useEventBus from "@/eventBus";
import type { responseModels } from "@/models";
import usePaymentsRepository from "@/repositories/paymentsRepository";
import useUserStore from "@/stores/userStore";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";

const paymentsRepository = usePaymentsRepository();
const userStore = useUserStore();
const { emitBus } = useEventBus();
const router = useRouter();

const showUnsubscribeModal = ref(false);
const informations = ref<responseModels.SubscriptionInformations | undefined>(undefined);

const isActive = computed(() => {
    return informations.value?.status == 0;
});

async function onUnsubscribe() {
    showUnsubscribeModal.value = false;

    if (!isActive.value) return;

    const result = await paymentsRepository.cancelCurrent();

    if (!result) {
        emitBus("flashOpen", "An error occured. Please contact us if the error persists.", "danger");

        return;
    }

    emitBus("flashOpen", "The subscription will now end on the due date", "success");
    router.push("/questions");
}

onMounted(async () => {
    if (!userStore.userDetails!.isPremium) return;

    const currentResult = await paymentsRepository.current();

    if (!currentResult.success) {
        router.push("/");

        return emitBus("flashOpen", currentResult.error.message, "flashOpen");
    }

    informations.value = currentResult.result;
});
</script>
<template>
    <template v-if="informations !== undefined">
        <ModalComponent v-model:show="showUnsubscribeModal">
            <template #title>
                <TitleComponent title-level="h2">Unsubscribe</TitleComponent>
            </template>

            <template #description>
                <h6 class="font-medium">Are you sure to unsubscribe from Aviask Premium ?</h6>
                <br />
                <p>
                    <b>Note :</b>
                    You will still have Premium privileges until the next due date
                </p>
            </template>

            <template #buttons>
                <ButtonComponent state="cta" @click="showUnsubscribeModal = false">Cancel</ButtonComponent>
                <ButtonComponent state="raised" @click="onUnsubscribe">Unsubscribe</ButtonComponent>
            </template>
        </ModalComponent>

        <form
            class="flex flex-col sm:w-4/5 md:w-3/5 mx-auto my-8 gap-8 bg-neutral-100 dark:bg-neutral-800 pt-6 px-10 pb-3 rounded-lg"
            @submit.prevent>
            <TitleComponent>Your current subscription</TitleComponent>

            <span class="flex gap-2 items-center dark:text-white">
                <HeroIcon icon="CheckIcon" />
                <span class="font-medium dark:text-gray-300">Aviask Premium</span>
            </span>

            <span class="flex gap-2 items-center dark:text-white">
                <HeroIcon class="fill-yellow-400 dark:fill-none" icon="BoltIcon" />
                <span class="font-medium dark:text-gray-300">
                    Your subscription is
                    <b v-if="isActive" class="text-accent">active</b>
                    <b v-else class="text-red-500">cancelled</b>
                </span>
            </span>

            <span class="flex gap-2 items-center dark:text-white">
                <HeroIcon icon="CalendarIcon" />
                <span class="font-medium dark:text-gray-300">
                    Started on
                    <b>{{ new Date(informations!.startedAt).toLocaleDateString() }}</b>
                </span>
            </span>

            <span class="flex gap-2 items-center dark:text-white">
                <HeroIcon icon="CalendarDaysIcon" />

                <span v-if="isActive" class="font-medium dark:text-gray-300">
                    Next payment on
                    <b class="text-accent">{{ new Date(informations!.nextPayment).toLocaleDateString() }}</b>
                </span>
                <span v-else>
                    Your subscription will stop on
                    <b class="text-accent">{{ new Date(informations!.nextPayment).toLocaleDateString() }}</b>
                </span>
            </span>

            <ButtonComponent v-if="isActive" state="danger" class="my-4" @click="showUnsubscribeModal = true">
                Cancel my subscription
            </ButtonComponent>
            <ButtonComponent v-else state="accent_purple" router-link="/payments/pricing" class="text-center">
                Subscribe to Premium
            </ButtonComponent>
        </form>
    </template>

    <template v-else>
        <SpinnerComponent />
    </template>
</template>
