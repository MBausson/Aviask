<script lang="ts" setup>
import { onMounted, ref, type Ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { TextInput, SelectComponent, ButtonComponent, SwitchComponent, HeroIcon, TitleComponent } from "@/components";
import useEventBus from "@/eventBus";
import { requestModels, UserRole } from "@/models";
import { ListboxOption } from "@headlessui/vue";
import useUsersRepository from "@/repositories/usersRepository";

const route = useRoute();
const router = useRouter();
const { emitBus } = useEventBus();
const userRepository = useUsersRepository();

const userId = route.params.id as string;
const userEdit: Ref<requestModels.EditUserModel | null> = ref(null);
const errorMessage = ref<string | null>(null);

async function onSubmit() {
    if (!userEdit.value) return;

    const editResult = await userRepository.edit(userId, userEdit.value);

    if (editResult.success) {
        router.push(`/user/${userId}/profile`);

        return emitBus("flashOpen", `Successfully updated user ${userEdit.value.username}`, "success");
    }

    emitBus("flashOpen", "Could not update the user", "danger");
}

onMounted(async () => {
    const userResult = await userRepository.update(userId);

    if (!userResult.success || !userResult.result) {
        router.push("/users");

        return emitBus("flashOpen", userResult.error.message, "danger");
    }

    userEdit.value = userResult.result;
});
</script>

<template>
    <p v-if="errorMessage" class="mt-2 mb-6 bg-red-200 font-semibold text-red-800 rounded-lg mx-2 px-6 py-5">
        <span>{{ errorMessage }}</span>
    </p>

    <template v-if="!userEdit">
        <div class="text-center dark:text-white">
            <h1 class="text-4xl font-bold my-4">User not found</h1>
            <p class="text-xl font-medium">
                Go back
                <RouterLink to="/users" class="font-bold">here</RouterLink>
            </p>
        </div>
    </template>

    <template v-else>
        <TitleComponent class="my-4">{{ userEdit.username || "Unnamed" }} &ndash; Editing</TitleComponent>

        <form
            class="flex flex-col sm:w-2/3 md:w-1/2 mx-auto my-6 gap-8 bg-neutral-100 dark:bg-neutral-800 pt-6 px-10 pb-3 rounded-lg"
            @submit.prevent="">
            <div class="flex flex-col gap-1">
                <label class="dark:text-white font-medium -ml-2 text-lg">Username</label>
                <TextInput v-model="userEdit.username" placeholder="Username" minlength="3" maxlength="24" required />
            </div>

            <div class="flex flex-col gap-1">
                <label class="dark:text-white font-medium -ml-2 text-lg">Email</label>
                <TextInput v-model="userEdit.email" placeholder="Email" maxlength="255" type="email" required />
            </div>

            <div class="flex flex-col gap-1">
                <label class="dark:text-white font-medium -ml-2 text-lg">Role</label>
                <SelectComponent v-model="userEdit.role" placeholder="Select a role...">
                    <template #selectTrigger>
                        {{ userEdit.role }}
                    </template>
                    <template #options>
                        <ListboxOption
                            v-for="role in UserRole.roles"
                            v-slot="{ selected }"
                            :key="role.value"
                            as="template"
                            :value="role.value">
                            <li
                                :class="[
                                    selected ? 'text-muted-dark bg-muted dark:bg-muted-dark' : 'text-gray-900',
                                    'relative cursor-default select-none py-2 pl-10 pr-4',
                                ]">
                                <span :class="[selected ? 'font-medium' : 'font-normal', 'block truncate']">
                                    {{ role.name }}
                                </span>

                                <span v-if="selected" class="absolute inset-y-0 left-0 flex items-center pl-3">
                                    <HeroIcon icon="CheckIcon" />
                                </span>
                            </li>
                        </ListboxOption>
                    </template>
                </SelectComponent>
            </div>

            <div class="flex items-center gap-4 px-2">
                <SwitchComponent v-model="userEdit.isPremium"></SwitchComponent>
                <label class="dark:text-white font-medium -ml-2 text-lg">Premium</label>
            </div>

            <div class="flex justify-center py-4">
                <ButtonComponent class="flex flex-row items-center gap-2" type="button" state="cta" @click="onSubmit">
                    <HeroIcon icon="PaperAirplaneIcon" type="solid" />
                    <span>Submit</span>
                </ButtonComponent>
            </div>
        </form>
    </template>
</template>
