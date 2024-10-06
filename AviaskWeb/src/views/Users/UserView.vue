<script lang="ts" setup>
import useEventBus from "@/eventBus";
import { responseModels } from "@/models";
import useUserStore from "@/stores/userStore";
import { RoleIcon, DateTime, BadgeComponent, HeroIcon, TitleComponent } from "@/components";
import { useRoute, useRouter } from "vue-router";
import useUsersRepository from "@/repositories/usersRepository";
import { onMounted, ref } from "vue";

const { emitBus } = useEventBus();
const userRepository = useUsersRepository();
const userStore = useUserStore();
const router = useRouter();
const route = useRoute();

const id = (route.params.id as string) || userStore.userDetails?.id;

if (!id) {
    router.push("/users/login");
}

const profileData = ref<responseModels.UserProfile | null>(null);

onMounted(async () => {
    const profileResult = await userRepository.profile(id!);

    if (!profileResult.success) {
        router.push("/");

        return emitBus("flashOpen", "Could not retrieve the user profile", "danger");
    }

    profileData.value = profileResult.result;
});
</script>

<template>
    <template v-if="profileData">
        <section class="my-4 flex items-start flex-col md:flex-row md:justify-between gap-2 md:items">
            <div class="flex items-center gap-1.5">
                <BadgeComponent
                    v-if="profileData.userDetails.isPremium"
                    badge-type="purple"
                    class="align-middle"
                    title="This user is subscribed to Premium">
                    <RouterLink to="/payments/pricing">Premium</RouterLink>
                </BadgeComponent>

                <TitleComponent class="inline first-letter:capitalize">
                    {{ profileData.userDetails.userName }}
                </TitleComponent>

                <RoleIcon class="!w-11 px-2 inline" :role="profileData.userDetails.role" />
            </div>
            <span class="text-lg font-medium md:font-semibold text-muted-dark dark:text-muted">
                Joined Aviask
                <DateTime
                    display="since"
                    :date="new Date(profileData.userDetails.createdAt)"
                    class="text-lg font-medium md:font-semibold" />
            </span>
        </section>

        <section class="mb-6 mt-8 flex flex-col gap-2">
            <span class="flex gap-2 items-center font-medium md:font-semibold">
                <HeroIcon icon="CheckCircleIcon" class="text-rose-500 dark:text-rose-300" />
                <span>Question suggestions</span>
            </span>

            <span v-if="profileData.acceptedSuggestions" class="text-lg text-neutral-800 dark:text-neutral-200">
                {{ profileData.userDetails.userName }} suggested
                <b>{{ profileData.acceptedSuggestions }}</b>
                questions which were accepted and added to the bank !
            </span>
            <span v-else class="text-lg text-neutral-700 dark:text-neutral-300">
                {{ profileData.userDetails.userName }} hasn't suggested a question yet
            </span>
        </section>

        <section class="my-4 flex flex-col gap-1">
            <span title-level="h3" class="flex gap-2 items-center font-medium md:font-semibold">
                <HeroIcon icon="ChartBarIcon" class="text-skyblue-dark" />
                <span>Overall statistics</span>
            </span>

            <span v-if="profileData.answersCount" class="text-lg text-neutral-800 dark:text-neutral-200">
                {{ profileData.userDetails.userName }} has answered a total of
                <b>{{ profileData.answersCount }}</b>
                answers on Aviask
            </span>
            <span v-else class="text-lg text-neutral-700 dark:text-neutral-300">
                {{ profileData.userDetails.userName }} hasn't answered any questions
            </span>
        </section>
    </template>
</template>
