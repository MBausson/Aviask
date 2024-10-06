<script lang="ts" setup>
import { ref } from "vue";
import { useRoute } from "vue-router";
import { ButtonComponent, HeroIcon, TitleComponent } from "@/components";

const route = useRoute();

const success = ref<boolean>(route.query.success?.toString().toLocaleLowerCase() === "true" ? true : false);
</script>

<template>
    <div v-if="success">
        <section
            class="fixed w-screen h-screen top-8 right-0 bg-white dark:bg-dark-primary py-14 px-16 flex flex-col [&>*]:h-1/3">
            <section class="my-4 text-center">
                <TitleComponent>Congratulations !</TitleComponent>
                <p class="text-lg font-medium text-neutral-500 dark:text-neutral-400">
                    You successfully subscribed to Premium
                </p>
            </section>

            <div
                id="image-container"
                class="[&>*]:w-56 [&>*]:select-none [&>*]:absolute flex justify-center items-center row-span-3">
                <img id="last" src="/src/assets/icon.svg" alt="Blue default Aviask logo" />
                <img class="-z-10" src="/src/assets/icon-purple.svg" alt="Purple premium Aviask logo" />
            </div>

            <div>
                <ButtonComponent
                    id="home-button"
                    state="accent_purple"
                    class="flex items-center shadow-sm text-sm sm:text-base gap-2 mt-12 mx-auto absolute left-1/2 -bottom-10 -translate-x-1/2"
                    :router-link="'/'">
                    <HeroIcon icon="HomeIcon" />
                    <span>Start using Aviask Premium</span>
                </ButtonComponent>
            </div>
        </section>
    </div>

    <div v-else>
        <section class="my-4 text-center">
            <TitleComponent>Checkout failure</TitleComponent>
            <p class="text-lg font-medium text-muted-dark dark:text-muted">
                Your attempt to subscribe to Premium failed
            </p>
        </section>

        <p class="text-lg font-medium my-8 text-center dark:text-white">
            If your payment failure persists, please
            <RouterLink to="/aboutus" class="underline underline-offset-4">contact us</RouterLink>
        </p>

        <ButtonComponent state="accent" class="flex items-center gap-2 mx-auto" :router-link="'/'">
            <HeroIcon icon="HomeIcon" type="solid" />
            <span>Go back to Aviask</span>
        </ButtonComponent>
    </div>
</template>

<style scoped>
#last {
    animation: 1s cubic-bezier(0.26, 0.07, 0.62, 0.8) top-leave;
    animation-delay: 1.15s;
    animation-fill-mode: forwards;
    clip-path: inset(0);
}

#image-container {
    animation: 1s cubic-bezier(0.52, 0.17, 0.6, 0.99) appear-fade;
}

#home-button {
    animation: 0.5s ease-out appear-slide-in;
    animation-fill-mode: forwards;
    animation-delay: 2.3s;
}

@keyframes top-leave {
    to {
        clip-path: inset(0 0 100% 0);
    }
}

@keyframes appear-fade {
    from {
        opacity: 0;
        scale: 0.5;
    }
    to {
        opacity: 100%;
        scale: 1;
    }
}

@keyframes appear-slide-in {
    from {
        bottom: 0;
        opacity: 50%;
    }
    to {
        bottom: 16%;
        opacity: 1;
    }
}
</style>
