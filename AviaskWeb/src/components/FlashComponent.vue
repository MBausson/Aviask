<script lang="ts" setup>
import useEventBus from "@/eventBus";
import { ref, watch } from "vue";

export type FlashType = "danger" | "success";

const { bus } = useEventBus();

const isShowing = ref(false);
const flashMessage = ref<string | undefined>(undefined);
const flashType = ref<FlashType>("success");
let lastTimeout: NodeJS.Timeout | undefined = undefined;

watch(
    () => bus.value.get("flashOpen"),
    async value => {
        if (!value || value.length < 1) return;

        const modifier = value.length == 2 ? value[1] : "success";
        const text = value[0];

        await openFlash(text, modifier as FlashType);
    },
);

async function openFlash(message: string, type: FlashType = "success") {
    isShowing.value = true;
    flashMessage.value = message;
    flashType.value = type;

    if (lastTimeout) {
        clearInterval(lastTimeout);
    }

    await new Promise(
        () =>
            (lastTimeout = setTimeout(() => {
                isShowing.value = false;
            }, 3500)),
    );
}
</script>

<template>
    <Transition name="flash">
        <div
            v-if="isShowing"
            :class="{
                'bg-red-200 text-red-900': flashType == 'danger',
                'bg-blue-200 text-blue-900': flashType == 'success',
            }"
            class="fixed w-4/5 md:w-fit bottom-16 bg-opacity-80 backdrop-blur-md left-1/2 -translate-x-1/2 rounded-md font-medium md:font-semibold shadow-md px-6 py-3 flex flex-col z-[1000]">
            <button class="absolute top-0 right-2 opacity-75 hover:opacity-100" @click="isShowing = false">
                &#x2715;
            </button>
            <span class="pr-3 cursor-default">
                {{ flashMessage }}
            </span>
        </div>
    </Transition>
</template>

<style scoped>
.flash-enter-active,
.flash-leave-active {
    animation: slide-up-in 0.4s cubic-bezier(0.21, 0.93, 0.94, 0.92);
}

.flash-enter-to {
    animation: slide-up-in 0.4s cubic-bezier(0.21, 0.93, 0.94, 0.92);
}

.flash-leave-to {
    animation: slide-up-out 0.4s cubic-bezier(0.21, 0.93, 0.94, 0.92);
}

@keyframes slide-up-in {
    from {
        translate: 0 calc(100% + 4rem);
    }
    to {
        translate: 0 0;
    }
}

@keyframes slide-up-out {
    from {
        translate: 0 0;
    }
    to {
        translate: 0 calc(100% + 4rem);
    }
}
</style>
